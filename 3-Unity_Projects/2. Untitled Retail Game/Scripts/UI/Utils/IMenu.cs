public interface IMenu
{
    protected virtual void OnShow(){}
    protected virtual void OnHide(){}

    public void Show(bool changeMouseState = true) ;
    public void Hide(bool changeMouseState = true);
}
