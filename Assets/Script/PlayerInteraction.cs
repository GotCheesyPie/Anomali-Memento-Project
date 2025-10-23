using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact();
}

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    float leftRightValue;
    void Update()
    {
        PerformInteractionCheck();
    }

    private void PerformInteractionCheck()
    {
        leftRightValue = Input.GetAxisRaw("Horizontal");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, leftRightValue * transform.right,
                                            interactionDistance, interactableLayer);

        Debug.DrawRay(transform.position, leftRightValue * transform.right * interactionDistance, Color.red, 0.2f);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
        }
    }
}
