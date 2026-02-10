using UnityEngine;

public class HoldableItem : InteractableNetworkObject
{
    public Vector3 heldPositionOffset;
    public Vector3 heldRotationValues;
    public bool hasUse;

    protected bool isPickedUp;

    public virtual void OnUse(PlayerController player) {}
    public virtual void OnUseSecondary(PlayerController player) {}
    public virtual void OnPickup(PlayerController player) {}
    public virtual void OnDrop() {}
    
    public virtual void HeldUpdate(PlayerController player) {}
}
