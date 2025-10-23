using UnityEngine;
using System.Linq; // Dibutuhkan untuk menggunakan .FirstOrDefault()

public class PlayerAndNPCSpawner : MonoBehaviour
{
    // Referensi ke NPC, bisa di-set di Inspector jika NPC ada di scene dari awal
    // atau dicari saat runtime.
    [SerializeField] private NPCFollow npcToPosition; 

    private void Awake()
    {
        // Cari SceneTransitionManager yang persisten
        SceneTransitionManager transitionManager = SceneTransitionManager.instance;
        if (transitionManager == null)
        {
            Debug.LogWarning("SceneTransitionManager tidak ditemukan. Spawning di posisi default.");
            return;
        }

        // Dapatkan identifier dari mana kita datang
        string entranceId = transitionManager.exitIdentifierForNextScene;
        
        // Temukan semua spawn point di scene ini
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();
        
        // Cari spawn point yang cocok dengan identifier
        SpawnPoint targetSpawnPoint = allSpawnPoints.FirstOrDefault(p => p.entranceIdentifier == entranceId);
        
        // Jika spawn point yang cocok ditemukan
        if (targetSpawnPoint != null)
        {
            // Temukan Player dan NPC
            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player == null) 
            {
                Debug.LogError("Player tidak ditemukan di scene!");
                return;
            }
            if (npcToPosition == null)
            {
                npcToPosition = FindObjectOfType<NPCFollow>();
            }

            // Posisikan Player dan NPC
            PositionCharacters(player.transform, (npcToPosition != null) ? npcToPosition.transform : null, targetSpawnPoint);
        }
        else
        {
            Debug.LogWarning("Tidak ada spawn point yang cocok untuk identifier: " + entranceId + ". Player tidak dipindahkan.");
        }
    }

    private void PositionCharacters(Transform player, Transform npc, SpawnPoint spawnPoint)
    {
        // 1. Atur Posisi Player
        player.position = spawnPoint.transform.position;

        // 2. Atur Arah Hadap Player
        float facingDirection = spawnPoint.spawnFacingRight ? 1f : -1f;
        player.localScale = new Vector3(Mathf.Abs(player.localScale.x) * facingDirection, player.localScale.y, player.localScale.z);
        
        Debug.Log($"Player spawned at {spawnPoint.name}, facing {(spawnPoint.spawnFacingRight ? "Right" : "Left")}");

        // 3. Atur Posisi dan Arah Hadap NPC (jika ada)
        if (npc != null)
        {
            // Gunakan offset dari skrip NPCFollow itu sendiri
            NPCFollow npcFollowScript = npc.GetComponent<NPCFollow>();
            Vector3 desiredOffset = new Vector3(npcFollowScript.offset.x * facingDirection, npcFollowScript.offset.y, npcFollowScript.offset.z);
            npc.position = player.position - desiredOffset;
            
            // Samakan arah hadap NPC
            npc.localScale = new Vector3(Mathf.Abs(npc.localScale.x) * facingDirection, npc.localScale.y, npc.localScale.z);
        }
    }
}