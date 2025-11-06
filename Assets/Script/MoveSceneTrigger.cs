using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MoveSceneTrigger : MonoBehaviour
{
    private enum MoveDirection
    {
        Kanan,
        Kiri
    }

    [Header("Pengaturan Transisi")]
    [Tooltip("Nama scene yang akan dimuat.")]
    [SerializeField] private string sceneToLoad;
    
    [Tooltip("Arah pemain akan berjalan saat transisi.")]
    [SerializeField] private MoveDirection transitionDirection;

    [Tooltip("Identifier untuk pintu keluar ini, akan digunakan sebagai pintu masuk di scene berikutnya.")]
    [SerializeField] private string thisExitIdentifier;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 moveDirection = (transitionDirection == MoveDirection.Kanan) ? Vector2.right : Vector2.left;

            SceneTransitionManager.instance.StartTransition(sceneToLoad, moveDirection, thisExitIdentifier);

            GetComponent<Collider2D>().enabled = false;
        }
    }

    void Update()
    {
        if (Dialogue.GetInstance().dialogueIsPlaying)
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<Collider2D>().enabled = true;
        }
    }
}