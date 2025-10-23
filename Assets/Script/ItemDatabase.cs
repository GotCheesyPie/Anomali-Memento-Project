// File: ItemDatabase.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Dibutuhkan untuk .FirstOrDefault

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // List yang berisi SEMUA kemungkinan ItemData yang ada di game Anda
    public List<ItemData> allGameItems;

    // Method untuk mencari ItemData berdasarkan ID-nya
    public ItemData GetItemById(string id)
    {
        // Cari di dalam list, item pertama yang ID-nya cocok
        return allGameItems.FirstOrDefault(item => item.id == id);
    }
}