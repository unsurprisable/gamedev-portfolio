
public class ProductOrderItem : InteractableClientObject
{
    public override void OnInteract(PlayerController player)
    {
        OrderMenuUI.Instance.Show();
    }
}
