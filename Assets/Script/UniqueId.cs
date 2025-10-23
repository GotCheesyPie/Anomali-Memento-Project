using UnityEngine;

[ExecuteInEditMode]
public class UniqueId : MonoBehaviour
{
    [Tooltip("ID unik ini dibuat secara otomatis di Editor. Jangan diubah.")]
    public string id;

    // HAPUS SELURUH METHOD AWAKE() DARI SINI

    // Method OnValidate ini akan memastikan ID dibuat saat Anda menempatkan
    // objek di dalam Scene Editor.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id) && !Application.isPlaying)
        {
            id = System.Guid.NewGuid().ToString();
        }
    }
}