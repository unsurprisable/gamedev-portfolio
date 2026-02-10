using UnityEngine;

public abstract class SubMenu : MonoBehaviour
{
    [SerializeField] protected GameObject visual;
    protected bool isEnabled;

    protected virtual void OnShow(){}
    protected virtual void OnHide(){}

    public void Show()
    {
        visual.SetActive(true);
        isEnabled = true;
        MenuManager.Instance.OnSubMenuOpen(this);
        OnShow();
    }
    public void Hide()
    {
        visual.SetActive(false);
        isEnabled = false;
        MenuManager.Instance.OnSubMenuClose();
        OnHide();
    }
}
