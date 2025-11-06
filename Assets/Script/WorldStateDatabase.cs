using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

// Class-class ini digunakan untuk menstrukturkan data save/load
[System.Serializable]
public class DroppedItemSaveData
{
    public string itemID;
    public Vector3 position;
    public string uniqueInstanceID;
}

[System.Serializable]
public class SceneSaveData
{
    public string sceneName;
    public List<string> pickedUpStaticItemIdsInScene = new List<string>();
    public List<DroppedItemSaveData> droppedItemsInScene = new List<DroppedItemSaveData>();
    public List<string> triggeredStoryIdsInScene = new List<string>();
}

[System.Serializable]
public class WorldSaveWrapper
{
    public List<SceneSaveData> allSceneData;
}


[CreateAssetMenu(fileName = "WorldStateDatabase", menuName = "Inventory/World State Database")]
public class WorldStateDatabase : ScriptableObject
{
    public List<SceneSaveData> allSceneData = new List<SceneSaveData>();
    public ItemDatabase itemDatabase;

    private void OnEnable()
    {
        // Berlangganan ke event sceneLoaded untuk memicu spawning
        SceneManager.sceneLoaded -= OnSceneLoaded; // Hapus dulu untuk mencegah duplikasi listener
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("WorldStateDatabase now listening for scene loads.");
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private SceneSaveData GetCurrentSceneData()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneSaveData sceneData = allSceneData.FirstOrDefault(s => s.sceneName == sceneName);
        if (sceneData == null)
        {
            sceneData = new SceneSaveData { sceneName = sceneName };
            allSceneData.Add(sceneData);
        }
        return sceneData;
    }
    
    public void RegisterStaticPickup(string uniqueId)
    {
        GetCurrentSceneData().pickedUpStaticItemIdsInScene.Add(uniqueId);
        SaveState();
    }

    public bool HasBeenPickedUp(string uniqueId)
    {
        return GetCurrentSceneData().pickedUpStaticItemIdsInScene.Contains(uniqueId);
    }
    
    public void RegisterDroppedItem(ItemData itemData, Vector3 position)
    {
        var saveData = new DroppedItemSaveData
        {
            itemID = itemData.id,
            position = position,
            uniqueInstanceID = System.Guid.NewGuid().ToString()
        };
        
        GetCurrentSceneData().droppedItemsInScene.Add(saveData);
        SpawnDroppedItem(saveData);
        SaveState();
    }

    public void UnregisterDroppedItem(string uniqueInstanceID)
    {
        GetCurrentSceneData().droppedItemsInScene.RemoveAll(item => item.uniqueInstanceID == uniqueInstanceID);
        SaveState();
    }

    public void RegisterStoryTrigger(string uniqueId)
    {
        if (!GetCurrentSceneData().triggeredStoryIdsInScene.Contains(uniqueId))
        {
            GetCurrentSceneData().triggeredStoryIdsInScene.Add(uniqueId);
            SaveState();
        }
    }

    public bool HasStoryBeenTriggered(string uniqueId)
    {
        foreach (var sceneData in allSceneData)
        {
            if (sceneData.triggeredStoryIdsInScene.Contains(uniqueId))
            {
                return true;
            }
        }
        
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("<color=yellow>EVENT: OnSceneLoaded event fired for scene: </color>" + scene.name);
        SpawnDroppedItemsForCurrentScene();
    }
    
    private void SpawnDroppedItemsForCurrentScene()
    {
        Debug.Log("SPAWNER: Checking for dropped items to spawn...");
        SceneSaveData sceneData = GetCurrentSceneData();

        if (sceneData.droppedItemsInScene != null && sceneData.droppedItemsInScene.Count > 0)
        {
            Debug.Log("SPAWNER: Found " + sceneData.droppedItemsInScene.Count + " items to spawn in this scene.");
            foreach (var itemDataToSpawn in sceneData.droppedItemsInScene)
            {
                SpawnDroppedItem(itemDataToSpawn);
            }
        }
        else
        {
            Debug.Log("SPAWNER: No dropped items recorded for this scene.");
        }
    }

    private void SpawnDroppedItem(DroppedItemSaveData dataToSpawn)
    {
        if (dataToSpawn == null)
        {
            Debug.LogError("<color=red>GAGAL SPAWN: Data item yang akan di-spawn null!</color>");
            return;
        }

        Debug.Log("SPAWNER: Attempting to spawn item with ItemID: " + dataToSpawn.itemID);

        if (itemDatabase == null)
        {
            Debug.LogError("<color=red>GAGAL SPAWN: Referensi ItemDatabase di WorldStateDatabase belum di-set!</color>");
            return;
        }
        
        ItemData itemInfo = itemDatabase.GetItemById(dataToSpawn.itemID);

        if (itemInfo != null)
        {
            Debug.Log("SPAWNER: ItemData '" + itemInfo.name + "' found in database.");
            if (itemInfo.itemPrefab != null)
            {
                Debug.Log("<color=green>SPAWNER: Prefab found! Instantiating now at position </color>" + dataToSpawn.position);
                GameObject itemGO = Instantiate(itemInfo.itemPrefab, dataToSpawn.position, Quaternion.identity);
                
                UniqueId uniqueIdComp = itemGO.GetComponent<UniqueId>();
                if (uniqueIdComp != null)
                {
                    uniqueIdComp.id = dataToSpawn.uniqueInstanceID;
                }
                else
                {
                    Debug.LogError("<color=red>ERROR: Prefab '" + itemInfo.itemPrefab.name + "' kekurangan skrip UniqueId!</color>");
                }

                Pickupable pickupableComp = itemGO.GetComponent<Pickupable>();
                if (pickupableComp != null)
                {
                    pickupableComp.InitializeAsDynamicItem();
                }
                else
                {
                    Debug.LogError("<color=red>ERROR: Prefab '" + itemInfo.itemPrefab.name + "' kekurangan skrip Pickupable!</color>");
                }
            }
            else
            {
                Debug.LogError("<color=red>GAGAL SPAWN: ItemData '" + itemInfo.name + "' ditemukan, tetapi slot Item Prefab-nya KOSONG!</color>");
            }
        }
        else
        {
            Debug.LogError("<color=red>GAGAL SPAWN: Tidak bisa menemukan ItemData dengan ID '" + dataToSpawn.itemID + "' di dalam ItemDatabase Anda!</color>");
        }
    }

    public void SaveState()
    {
        WorldSaveWrapper wrapper = new WorldSaveWrapper { allSceneData = this.allSceneData };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("WorldState", json);
        PlayerPrefs.Save();
        Debug.Log("World State Saved to PlayerPrefs.");
    }

    public void LoadState()
    {
        if (PlayerPrefs.HasKey("WorldState"))
        {
            string json = PlayerPrefs.GetString("WorldState");
            if (!string.IsNullOrEmpty(json))
            {
                WorldSaveWrapper wrapper = JsonUtility.FromJson<WorldSaveWrapper>(json);
                this.allSceneData = wrapper.allSceneData;
                Debug.Log("World State Loaded from PlayerPrefs.");
            }
        }
        else
        {
            this.allSceneData = new List<SceneSaveData>();
            Debug.Log("No saved world state found. Initializing a new one.");
        }
    }
    
    public void Clear()
    {
        allSceneData.Clear();
    }
}