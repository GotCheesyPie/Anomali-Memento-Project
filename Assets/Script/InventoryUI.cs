using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;
    
    [Header("Komponen UI")]
    public GameObject inventoryPanel;
    public Transform slotContainer;
    public GameObject itemUIPrefab;

    // [Header("Referensi Lain")]
    // private PlayerMovement playerMovement;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject.transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject.transform.root.gameObject);
        }
    }

    private void Start()
    {
        // playerMovement = FindObjectOfType<PlayerMovement>();
        inventoryPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }
    
    private void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateUI();
            // if (playerMovement != null) playerMovement.isAbleToMove = false; // Matikan gerakan pemain
        }
        // else
        // {
        //     if (playerMovement != null) playerMovement.isAbleToMove = true; // Aktifkan lagi gerakan pemain
        // }
    }

    public void UpdateUI()
    {
        if (slotContainer == null) return;

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        if (PlayerInventory.instance == null) return;
        
        foreach (ItemData item in PlayerInventory.instance.Bag)
        {
            if (item != null)
            {
                GameObject itemGO = Instantiate(itemUIPrefab, slotContainer);
                DraggableItem draggableItem = itemGO.GetComponent<DraggableItem>();
                if (draggableItem != null)
                {
                    draggableItem.SetItem(item);
                }
            }
        }
    }
}