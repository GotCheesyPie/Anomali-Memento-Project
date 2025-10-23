using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1f;

    public string exitIdentifierForNextScene { get; private set; }

    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartTransition(string sceneName, Vector2 moveDirection, string exitIdentifier)
    {
        if (!isTransitioning)
        {
            // Simpan identifier untuk digunakan di scene berikutnya
            this.exitIdentifierForNextScene = exitIdentifier;
            StartCoroutine(TransitionCoroutine(sceneName, moveDirection));
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector2 moveDirection)
    {
        isTransitioning = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.isAbleToMove = false;
            player.ForceMove(moveDirection.x);
        }
        float alpha = 0;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(sceneName);
        
        yield return new WaitForSeconds(0.1f);

        alpha = 1;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.isAbleToMove = true;
        }

        isTransitioning = false;
    }
}