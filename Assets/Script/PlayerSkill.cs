using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerSkill : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Image redScreenImage;

    [Header("Power Settings")]
    [SerializeField] private float maxChargeTime = 5f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDistance = 10f;
    [SerializeField] private float stunAreaHeight = 3f; 
    [SerializeField] private LayerMask monsterLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private enum PowerState { Idle, Charging, Releasing }
    private PowerState currentState = PowerState.Idle;
    
    private List<EnemyAI> currentlyStunnedEnemies = new List<EnemyAI>();

    private float chargeTimer = 0f;
    
    private void Awake()
    {
        // Find the GameObject with the tag and get the Image component from it.
        GameObject redScreenObject = GameObject.FindWithTag("RedScreenUI");
        if (redScreenObject != null)
        {
            redScreenImage = redScreenObject.GetComponent<Image>();
        }
        else
        {
            // This error helps with debugging if you forget to tag the object
            Debug.LogError("PlayerSkill could not find an object with the tag 'RedScreenUI'.");
        }
    }

    private void Update()
    {
        HandleInput();
        
        if (currentState == PowerState.Charging)
        {
            HandleCharging();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentState == PowerState.Idle)
        {
            StartCharging();
        }

        if (Input.GetKeyUp(KeyCode.Q) && currentState == PowerState.Charging)
        {
            ReleasePower();
        }
    }

    private void StartCharging()
    {
        currentState = PowerState.Charging;
        chargeTimer = 0f;
        redScreenImage.color = new Color(1f, 0f, 0f, 0.2f);
    }

    private void HandleCharging()
    {
        chargeTimer += Time.deltaTime;
        
        UpdateVisibleStunnedMonsters();
        
        if (chargeTimer >= maxChargeTime)
        {
            ReleasePower();
        }
    }

    private void ReleasePower()
    {
        if (currentState != PowerState.Charging) return;
        
        foreach (var enemy in currentlyStunnedEnemies)
        {
            if (enemy != null)
            {
                enemy.StopStun();
            }
        }
        currentlyStunnedEnemies.Clear();

        currentState = PowerState.Releasing;
        StartCoroutine(ReleaseEffectCoroutine(chargeTimer));
    }

    private void UpdateVisibleStunnedMonsters()
    {
        Vector2 origin = transform.position;
        // Dapatkan arah hadap (1 untuk kanan, -1 untuk kiri)
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        // Ukuran 'balok' yang kita tembakkan. Lebar bisa dibuat kecil.
        Vector2 boxSize = new Vector2(0.1f, stunAreaHeight);

        // Tembakkan 'balok' dan kumpulkan semua musuh yang terkena
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, boxSize, 0f, direction, stunDistance, monsterLayer);

        foreach (RaycastHit2D hit in hits)
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null && !currentlyStunnedEnemies.Contains(enemy))
            {
                // Line of sight check sederhana
                RaycastHit2D obstacleHit = Physics2D.Linecast(transform.position, hit.point, obstacleLayer);
                if (obstacleHit.collider == null)
                {
                    enemy.StartStun();
                    currentlyStunnedEnemies.Add(enemy);
                }
            }
        }
    }
    private IEnumerator ReleaseEffectCoroutine(float duration)
    {
        redScreenImage.color = new Color(1f, 0f, 0f, 1f);
        yield return new WaitForSeconds(duration);

        float fadeDuration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            redScreenImage.color = new Color(1f, 0f, 0f, newAlpha);
            yield return null;
        }

        redScreenImage.color = new Color(1f, 0f, 0f, 0f);
        currentState = PowerState.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        Vector2 boxSize = new Vector2(0.1f, stunAreaHeight);
        
        // Menggambar kotak sebagai visualisasi area stun
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(origin + (direction * (stunDistance / 2)), new Vector3(stunDistance, boxSize.y, 1));
    }
}