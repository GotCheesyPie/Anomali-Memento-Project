using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    // --- Referensi yang perlu di-set di Inspector ---
    [Header("Referensi Objek")]
    public RectTransform uiElement;
    public Transform targetToFollow;
    public RectTransform canvasRectTransform; 

    // --- Pengaturan ---
    [Header("Pengaturan")]
    public Vector3 worldOffset = new Vector3(0, 3f, 0);
    [Range(1f, 10f)]
    public float smoothSpeed = 10f;
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (canvasRectTransform == null)
        {
            canvasRectTransform = uiElement.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
    }

    void LateUpdate()
    {
        if (uiElement == null || targetToFollow == null || mainCamera == null || canvasRectTransform == null)
        {
            return;
        }

        Vector3 targetWorldPosition = targetToFollow.position + worldOffset;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(targetWorldPosition);
        
        if (screenPoint.z > 0)
        {
            uiElement.gameObject.SetActive(true);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, 
                screenPoint,               
                mainCamera,              
                out localPoint           
            );

            uiElement.localPosition = Vector2.Lerp(uiElement.localPosition, localPoint, smoothSpeed * Time.deltaTime);
        }
        else
        {
            uiElement.gameObject.SetActive(false);
        }
    }
}