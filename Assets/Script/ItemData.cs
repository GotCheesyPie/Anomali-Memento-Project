// File: ItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // --- TAMBAHKAN BARIS INI ---
    [Tooltip("ID unik untuk item ini, misal: 'potion_health_01', 'key_castle_main'.")]
    public string id; 
    
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
}