using UnityEngine;

// Wajibkan komponen yang dibutuhkan agar tidak lupa dipasang
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(UniqueId))]
public class StoryTrigger : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Database yang menyimpan state dunia game.")]
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    [Header("Ink JSON")]
    [Tooltip("File .json dari cerita Ink yang akan dimainkan.")]
    [SerializeField] private TextAsset inkJSON;

    private UniqueId uniqueId;

    private void Awake()
    {
        uniqueId = GetComponent<UniqueId>();

        if (worldStateDatabase.HasStoryBeenTriggered(uniqueId.id))
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") && !Dialogue.GetInstance().dialogueIsPlaying)
        {
            Dialogue.GetInstance().EnterDialogueMode(inkJSON);

            worldStateDatabase.RegisterStoryTrigger(uniqueId.id);

            GetComponent<Collider2D>().enabled = false;

            // collider.gameObject.GetComponent<PlayerMovement>().isAbleToMove = false;
        }
    }
    
}