using UnityEngine;

[RequireComponent(typeof(UniqueId))]
public class Pickupable : MonoBehaviour, IInteractable
{
    [Header("Referensi")]
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    public GameObject pickupableText;
    public ItemData itemData;

    private UniqueId uniqueId;
    private bool isDynamicallySpawned = false;

    private void Awake()
    {
        uniqueId = GetComponent<UniqueId>();
    }

    private void Start()
    {
        if (!isDynamicallySpawned && worldStateDatabase.HasBeenPickedUp(uniqueId.id))
        {
            Destroy(gameObject);
        }
    }
    
    public void InitializeAsDynamicItem()
    {
        isDynamicallySpawned = true;
    }

    public void Interact()
    {
        PlayerInventory.instance.AddItem(itemData);

        if (isDynamicallySpawned)
        {
            worldStateDatabase.UnregisterDroppedItem(uniqueId.id);
        }
        else
        {
            worldStateDatabase.RegisterStaticPickup(uniqueId.id);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            pickupableText.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            pickupableText.SetActive(false);
        }
    }
}