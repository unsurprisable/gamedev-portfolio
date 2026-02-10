using UnityEngine;

public class BroomItem : HoldableItem
{
    public override void OnPickup(PlayerController player)
    {
        Debug.Log("I, a broom, was picked up! :o");
    }

    public override void OnDrop()
    {
        Debug.Log("oh no!! I, a broom, was dropped! D:");
    }

    public override void OnUse(PlayerController player)
    {
        Debug.Log("wow! I, a broom, was used!! how nice :D");
    }
}
