using UnityEngine;

public abstract class Menu : MonoBehaviour, IMenu
{
    [SerializeField] protected GameObject visual;
    protected bool isEnabled;

    protected virtual void OnShow(){}
    protected virtual void OnHide(){}

    public void Show(bool changeMouseState = true) 
    {
        visual.SetActive(true);
        isEnabled = true;
        MenuManager.Instance.OnMenuOpen(this, changeMouseState);
        OnShow();
    }
    public void Hide(bool changeMouseState = true)
    {
        visual.SetActive(false);
        isEnabled = false;
        MenuManager.Instance.OnMenuClose(changeMouseState);
        OnHide();
    }
}
