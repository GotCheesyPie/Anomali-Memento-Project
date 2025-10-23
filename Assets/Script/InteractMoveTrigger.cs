using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractMoveTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [Tooltip("GameObject yang akan muncul saat pemain bisa berinteraksi.")]
    [SerializeField] private GameObject visualCue;
    
    [Header("Pengaturan Transisi")]
    [Tooltip("Nama scene yang akan dimuat.")]
    [SerializeField] private string sceneToLoad;

    [Tooltip("Identifier untuk pintu keluar ini, akan digunakan sebagai pintu masuk di scene berikutnya.")]
    [SerializeField] private string thisExitIdentifier;

    private bool playerInRange;

    private void Awake()
    {
        // Pastikan visual cue tidak aktif saat permainan dimulai
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SceneTransitionManager.instance.StartTransition(sceneToLoad, Vector2.zero, thisExitIdentifier);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek jika objek yang masuk adalah Player
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            visualCue.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cek jika objek yang keluar adalah Player
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            visualCue.SetActive(false);
        }
    }
}