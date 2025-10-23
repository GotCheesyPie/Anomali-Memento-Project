using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    
    public List<ItemData> Bag = new List<ItemData>();
    public static PlayerInventory instance;

    [System.Serializable]
    private class InventorySaveData
    {
        public List<string> itemIds;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }

    public void AddItem(ItemData item)
    {
        Bag.Add(item);
        if (InventoryUI.instance != null) InventoryUI.instance.UpdateUI();
        SaveInventory(); 
    }

    public void RemoveItem(ItemData item)
    {
        Bag.Remove(item);
        if (InventoryUI.instance != null) InventoryUI.instance.UpdateUI();
        SaveInventory(); 
    }

    private void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData
        {
            itemIds = new List<string>()
        };

        foreach (ItemData item in Bag)
        {
            saveData.itemIds.Add(item.id);
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("PlayerInventory", json);
        PlayerPrefs.Save();
        Debug.Log("Inventory Saved to PlayerPrefs.");
    }

    private void LoadInventory()
    {
        if (PlayerPrefs.HasKey("PlayerInventory"))
        {
            string json = PlayerPrefs.GetString("PlayerInventory");
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
            
            Bag.Clear();

            foreach (string id in saveData.itemIds)
            {
                ItemData item = itemDatabase.GetItemById(id);
                if (item != null)
                {
                    Bag.Add(item);
                }
            }
            Debug.Log("Inventory Loaded from PlayerPrefs.");
        }
    }
}