using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    private List<Interactable> interactables = new List<Interactable>();
    private Interactable closestInteracble;

    #region Interaction Logic

    private void Start()
    {
        // Register interact input event
        Player player = GetComponent<Player>();
        player.controls.Character.Interact.performed += ctx => InteractWithClosest();
    }

    // Find and highlight the closest interactable
    public void UpdateClosestInteracble()
    {
        closestInteracble?.Highlight(false);

        closestInteracble = null;
        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in interactables)
        {
            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteracble = interactable;
            }
        }

        closestInteracble?.Highlight(true);
    }

    // Interact with the closest interactable and remove it from the list
    private void InteractWithClosest()
    {
        closestInteracble?.Interact();
        interactables.Remove(closestInteracble);
        UpdateClosestInteracble();
    }

    // Return the list of interactables
    public List<Interactable> GetInteractables()
    {
        return interactables;
    }

    #endregion
}

