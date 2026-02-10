using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private bool inMenu;
    [SerializeField] private Stack<SubMenu> openSubMenus;
    [SerializeField] private IMenu activeMenu;

    private void Awake()
    {
        Instance = this;

        openSubMenus = new Stack<SubMenu>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseMenu += (sender, args) => {
            if (openSubMenus.Count > 0) {
                openSubMenus.Peek().Hide();
            } else if (inMenu) {
                activeMenu.Hide();
            } else {
                PauseMenuUI.Instance.Show();
            }
        };
    }

    public void OnSubMenuOpen(SubMenu subMenu)
    {
        openSubMenus.Push(subMenu);
    }

    public void OnSubMenuClose()
    {
        openSubMenus.Pop();
    }

    public void OnMenuOpen(IMenu menu, bool changeMouseState) {
        inMenu = true;
        activeMenu = menu;
        PlayerController.LocalInstance.DisableControls(changeMouseState);
    }
    public void OnMenuClose(bool changeMouseState) {
        inMenu = false;
        activeMenu = null;
        PlayerController.LocalInstance.EnableControls(changeMouseState);
    }
    public bool IsInMenu() {
        return inMenu;
    }

    internal void OnMenuOpen(Menu menu)
    {
        throw new NotImplementedException();
    }
}
