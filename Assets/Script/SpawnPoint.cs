using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Identifier unik untuk pintu masuk ini. Harus cocok dengan 'Exit Identifier' dari scene sebelumnya.")]
    public string entranceIdentifier;
    
    [Tooltip("Centang jika pemain harus menghadap ke kanan saat spawn di sini.")]
    public bool spawnFacingRight = true;
}