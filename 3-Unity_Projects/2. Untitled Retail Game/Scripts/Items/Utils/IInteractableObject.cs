
// literally just used for detecting InteractableNetworkObjects and InteractableClientObjects as one
public interface IInteractableObject
{
    public void OnInteract(PlayerController player);
    public void OnInteractSecondary(PlayerController player);
    public void OnInteractHeld(PlayerController player, float time);
    public void OnInteractHeldLookAway(PlayerController player);
    public void OnAlternateHeld(PlayerController player, float time);
    public void OnAlternateHeldLookAway(PlayerController player);
    public void OnHovered();
    public void OnUnhovered();

    public void Hover();
    public void Unhover();

    public bool IsHovered();
}
