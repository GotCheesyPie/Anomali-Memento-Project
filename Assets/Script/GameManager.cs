using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Referensi ke database Anda, seret dari Project window ke Inspector
    [SerializeField] private WorldStateDatabase worldStateDatabase;

    private void Awake()
    {
        // Setup Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // --- INI BAGIAN PALING PENTING ---
            // Muat semua data persisten saat game pertama kali dijalankan
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGameData()
    {
        Debug.Log("GameManager is loading all game data...");
        // Pastikan database tidak null sebelum memuat
        if (worldStateDatabase != null)
        {
            worldStateDatabase.LoadState();
        }

        // Panggil juga LoadInventory() dari PlayerInventory
        // Pastikan PlayerInventory sudah ada di scene
        if (PlayerInventory.instance != null)
        {
            // PlayerInventory sudah otomatis Load di Awake-nya sendiri, jadi baris ini opsional
            // PlayerInventory.instance.LoadInventory(); 
        }
    }
}