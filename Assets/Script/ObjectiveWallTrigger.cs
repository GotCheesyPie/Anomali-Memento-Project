using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pastikan GameObject ini memiliki Collider2D
[RequireComponent(typeof(Collider2D))]
public class ObjectiveWallTrigger : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Database yang menyimpan state dunia game.")]
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    [Header("Pengaturan Objective")]
    [Tooltip("Masukkan Unique ID dari Story Trigger yang harus selesai agar tembok ini nonaktif.")]
    [SerializeField] private string storyIdToCheck;

    private Collider2D wallCollider;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();

        if (worldStateDatabase == null)
        {
            Debug.LogError("Error: WorldStateDatabase belum di-assign di Inspector!", this);
            return;
        }

        if (string.IsNullOrEmpty(storyIdToCheck))
        {
            Debug.LogWarning("Warning: 'Story Id To Check' masih kosong. Tembok ini mungkin tidak akan pernah nonaktif.", this);
            return;
        }

        // Cek apakah story yang dimaksud SUDAH dipicu
        if (worldStateDatabase.HasStoryBeenTriggered(storyIdToCheck))
        {
            // Jika sudah, nonaktifkan collider tembok ini
            wallCollider.enabled = false;
        }
        else
        {
            // Jika belum, pastikan collider-nya aktif
            wallCollider.enabled = true;
        }
    }
}
