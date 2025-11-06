using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // <-- Kita butuh ini untuk Image

// Skrip ini diletakkan di UI Image yang transparan
// yang berfungsi sebagai "drop zone" di tepi kubangan.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))] // Image ini bisa transparan
public class ItemDropTarget : MonoBehaviour, IDropHandler
{
    [Header("Referensi")]
    [Tooltip("Database yang menyimpan state dunia game.")]
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    [Header("Sprite")]
    [Tooltip("Image/Sprite yang akan MUNCUL setelah item ditaruh (misal: gambar tangga).")]
    [SerializeField] private Image visualFeedbackSprite;

    [Header("Pengaturan Objective")]
    [Tooltip("ID dari item yang diperlukan (misal: 'tangga').")]
    [SerializeField] private string requiredItemId;

    [Tooltip("Story ID yang akan dipicu saat item yang benar di-drop (misal: 'tangga_terpasang').")]
    [SerializeField] private string objectiveStoryIdToTrigger;

    [Tooltip("Apakah item akan dikonsumsi (dihapus) setelah di-drop?")]
    [SerializeField] private bool consumeItem = true;

    private Image dropZoneImage; // Komponen Image dari drop zone ini

    private void Awake()
    {
        dropZoneImage = GetComponent<Image>();

        if (worldStateDatabase == null)
        {
            Debug.LogError("Error: WorldStateDatabase belum di-assign!", this);
            return;
        }

        // Cek status awal saat scene load
        InitializeTargetState();
    }

    private void InitializeTargetState()
    {
        // Cek apakah objective ini SUDAH selesai (dari save file)
        if (worldStateDatabase.HasStoryBeenTriggered(objectiveStoryIdToTrigger))
        {
            // Jika sudah, langsung tampilkan tangga
            if (visualFeedbackSprite != null)
                visualFeedbackSprite.enabled = true;
            
            // Matikan kemampuan drop, karena sudah selesai
            dropZoneImage.raycastTarget = false;
        }
        else
        {
            // Jika belum, pastikan sprite tangga mati
            if (visualFeedbackSprite != null)
                visualFeedbackSprite.enabled = false;
            
            // Aktifkan kemampuan drop
            dropZoneImage.raycastTarget = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Cek apakah drop zone ini aktif
        if (!dropZoneImage.raycastTarget) return;

        // Dapatkan skrip DraggableItem dari item yang di-drag
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // Cek apakah item yang di-drag adalah item yang benar
            if (draggableItem.GetItemData().id == requiredItemId)
            {
                // BENAR!
                Debug.Log("Item yang benar (" + requiredItemId + ") telah diletakkan!");

                // 1. Picu Objective/Story ID di database
                if (!string.IsNullOrEmpty(objectiveStoryIdToTrigger))
                {
                    worldStateDatabase.RegisterStoryTrigger(objectiveStoryIdToTrigger);
                }

                // 2. Tampilkan sprite tangga
                if (visualFeedbackSprite != null)
                    visualFeedbackSprite.enabled = true;

                // 3. Matikan drop zone ini agar tidak bisa dipakai lagi
                dropZoneImage.raycastTarget = false;

                // 4. Beri tahu DraggableItem bahwa drop berhasil
                draggableItem.SetDropSuccessful(true);

                // 5. Hapus item dari inventori
                if (consumeItem)
                {
                    PlayerInventory.instance.RemoveItem(draggableItem.GetItemData());
                    Destroy(eventData.pointerDrag); // Hancurkan UI item yang di-drag
                }
            }
            else
            {
                // SALAH!
                Debug.Log("Item yang salah! Diperlukan: " + requiredItemId);
                draggableItem.SetDropSuccessful(false);
            }
        }
    }
}

