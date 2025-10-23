// File: DraggableItem.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private WorldStateDatabase worldStateDatabase;
    [SerializeField] ItemData itemData;
    [SerializeField] Image image;
    [SerializeField] Transform parentAfterDrag;

    public void SetItem(ItemData data)
    {
        itemData = data;
        image = GetComponent<Image>();
        image.sprite = itemData.icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Simpan parent asli (slot)
        parentAfterDrag = transform.parent;
        // Pindahkan item ke root Canvas agar bisa digambar di atas semua UI lain
        transform.SetParent(transform.root); 
        // Agar item tidak memblokir raycast ke objek di bawahnya
        image.raycastTarget = false; 
        
        // Debug.Log("Mulai drag: " + itemData.itemName);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Camera renderCamera = (GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay) 
                                ? null 
                                : GetComponentInParent<Canvas>().worldCamera;

        if (renderCamera != null)
        {
            Vector3 mousePoint = eventData.position;
            mousePoint.z = GetComponentInParent<Canvas>().planeDistance; 
            transform.position = renderCamera.ScreenToWorldPoint(mousePoint);
        }
        else
        {
            transform.position = eventData.position;
        }
    }

 
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        if (!IsPointerOverInventoryPanel(eventData))
        {
            PlayerInventory.instance.RemoveItem(itemData);
            
            if (itemData.itemPrefab != null)
            {
                 Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                 worldPoint.z = 0;
                 
                 // MINTA DATABASE UNTUK MENCATAT DAN MEMBUAT ITEM
                 worldStateDatabase.RegisterDroppedItem(itemData, worldPoint);
            }
            
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
    }

    private bool IsPointerOverInventoryPanel(PointerEventData eventData)
    {
        // Buat list untuk menampung hasil raycast UI
        var results = new List<RaycastResult>();
        // Lakukan raycast dari event system pada posisi pointer
        EventSystem.current.RaycastAll(eventData, results);

        // Cek apakah salah satu hasil raycast adalah panel inventory kita
        foreach(var result in results)
        {
            if(result.gameObject.CompareTag("InventoryPanelTag"))
            {
                // Pointer ada di atas panel inventory
                return true; 
            }
        }
        
        // Jika loop selesai dan tidak menemukan panel, berarti pointer di luar
        return false;
    }
}