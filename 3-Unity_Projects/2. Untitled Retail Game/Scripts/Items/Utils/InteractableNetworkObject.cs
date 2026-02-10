using Unity.Netcode;
using UnityEngine;

public class InteractableNetworkObject : NetworkBehaviour, IInteractableObject
{
    [Header("Interactable Object")]
    [Tooltip("Leave empty if you don't want it to have an outline.")]
    [SerializeField] private bool enableOutline = true;
    private Outline outline;
    protected bool isHovered;
    
    public virtual void OnHovered(){}
    public virtual void OnInteract(PlayerController player){}
    public virtual void OnInteractSecondary(PlayerController player){}
    public virtual void OnInteractHeld(PlayerController player, float time){}
    public virtual void OnInteractHeldLookAway(PlayerController player){}
    public virtual void OnAlternateHeld(PlayerController player, float time){}
    public virtual void OnAlternateHeldLookAway(PlayerController player){}
    public virtual void OnUnhovered(){}

    private void Start()
    {
        if (enableOutline) {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = PlayerController.LocalInstance.outlineMode;
            outline.OutlineColor = PlayerController.LocalInstance.outlineColor;
            outline.OutlineWidth = PlayerController.LocalInstance.outlineWidth;
            outline.enabled = false;
        }
    }

    public void Hover()
    {
        OnHovered();
        if (enableOutline) outline.enabled = true;
        isHovered = true;
    }

    public void Unhover()
    {
        OnUnhovered();
        if (enableOutline) outline.enabled = false;
        isHovered = false;
    }
    
    public bool IsHovered() {
        return isHovered;
    }
}
