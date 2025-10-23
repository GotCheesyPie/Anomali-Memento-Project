using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DatabaseAutoClear
{
    static DatabaseAutoClear()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Membersihkan ScriptableObject (data in-memory)
            ClearDatabaseAsset();

            // Membersihkan PlayerPrefs untuk Inventory
            Debug.Log("Clearing PlayerInventory from PlayerPrefs...");
            PlayerPrefs.DeleteKey("PlayerInventory");

            // --- TAMBAHAN: Membersihkan PlayerPrefs untuk World State ---
            Debug.Log("Clearing WorldState from PlayerPrefs...");
            PlayerPrefs.DeleteKey("WorldState");
        }
    }

    private static void ClearDatabaseAsset()
    {
        // Ganti nama pencarian menjadi WorldStateDatabase
        string[] guids = AssetDatabase.FindAssets("t:WorldStateDatabase");
        
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var database = AssetDatabase.LoadAssetAtPath<WorldStateDatabase>(path);

        if (database != null)
        {
            database.Clear();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }
}