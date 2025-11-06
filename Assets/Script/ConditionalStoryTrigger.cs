using UnityEngine;

// Wajibkan komponen yang dibutuhkan
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(UniqueId))]
public class ConditionalStoryTrigger : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Database yang menyimpan state dunia game.")]
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    [Header("Ink JSON")]
    [Tooltip("File .json dari cerita Ink yang akan dimainkan.")]
    [SerializeField] private TextAsset inkJSON;

    [Header("Pengaturan Objective")]
    [Tooltip("Story ID yang HARUS selesai (dari ItemDropTarget) agar collider ini AKTIF.")]
    [SerializeField] private string requiredStoryIdToEnable;

    private UniqueId uniqueId;
    private Collider2D storyCollider;
    private bool isColliderEnabled = false; // Flag untuk optimasi

    private void Awake()
    {
        uniqueId = GetComponent<UniqueId>();
        storyCollider = GetComponent<Collider2D>();

        if (worldStateDatabase == null)
        {
            Debug.LogError("Error: WorldStateDatabase belum di-assign!", this);
            return;
        }

        if (string.IsNullOrEmpty(requiredStoryIdToEnable))
        // Cek skrip
        {
            Debug.LogWarning("Warning: 'Required Story Id To Enable' masih kosong. Collider ini mungkin tidak akan pernah aktif.", this);
        }

        // Cek 1: Apakah story INI SENDIRI sudah pernah terpicu?
        if (worldStateDatabase.HasStoryBeenTriggered(uniqueId.id))
        {
            // Jika sudah, matikan permanen
            storyCollider.enabled = false;
            isColliderEnabled = true; // Anggap saja sudah "aktif" (tapi sudah selesai)
        }
        // Cek 2: Jika belum terpicu, apakah syaratnya sudah terpenuhi saat scene load?
        else if (worldStateDatabase.HasStoryBeenTriggered(requiredStoryIdToEnable))
        {
            // Syarat sudah terpenuhi, langsung aktifkan
            storyCollider.enabled = true;
            isColliderEnabled = true;
        }
        // Cek 3: Jika belum terpicu DAN syarat belum terpenuhi
        else
        {
            // Matikan collider, biarkan Update() yang bekerja
            storyCollider.enabled = false;
            isColliderEnabled = false;
        }
    }

    private void Update()
    {
        // Jika collider sudah aktif (atau sudah selesai), hentikan pengecekan
        if (isColliderEnabled || worldStateDatabase == null)
        {
            return;
        }

        // Terus periksa apakah syarat sudah terpenuhi
        if (worldStateDatabase.HasStoryBeenTriggered(requiredStoryIdToEnable))
        {
            Debug.Log("Syarat terpenuhi! Mengaktifkan collider untuk " + uniqueId.id, this);
            storyCollider.enabled = true;
            isColliderEnabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Fungsi ini sama persis dengan StoryTrigger biasa
        if (collider.gameObject.CompareTag("Player") && !Dialogue.GetInstance().dialogueIsPlaying)
        {
            Dialogue.GetInstance().EnterDialogueMode(inkJSON);

            // Daftarkan story INI SENDIRI agar tidak terpicu dua kali
            worldStateDatabase.RegisterStoryTrigger(uniqueId.id);

            // Nonaktifkan collider setelah terpicu
            storyCollider.enabled = false;
        }
    }
}
