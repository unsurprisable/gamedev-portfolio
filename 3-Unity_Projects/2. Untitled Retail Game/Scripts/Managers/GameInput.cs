using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler MainAction;
    public event EventHandler MainActionReleased;
    public event EventHandler SecondaryAction;
    public event EventHandler SecondaryActionReleased;
    public event EventHandler OnDrop;
    public event EventHandler OnScroll;

    public event EventHandler OnRotateLeft;
    public event EventHandler OnRotateRight;
    
    public event EventHandler OnBuildMenu;
    public event EventHandler OnPauseMenu;
    public event EventHandler OnEnterChat;
    

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed             += (context) => {OnJump?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.MainAction.performed       += (context) => {MainAction?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.MainAction.canceled        += (context) => {MainActionReleased?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.SecondaryAction.performed  += (context) => {SecondaryAction?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.SecondaryAction.canceled   += (context) => {SecondaryActionReleased?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.Scroll.performed           += (context) => {OnScroll?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.Drop.performed             += (context) => {OnDrop?.Invoke(this, EventArgs.Empty);};

        playerInputActions.Player.RotateLeft.performed       += (context) => {OnRotateLeft?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.RotateRight.performed      += (context) => {OnRotateRight?.Invoke(this, EventArgs.Empty);};
        
        playerInputActions.Menu.Enable();
        playerInputActions.Menu.BuildMenu.performed          += (context) => {OnBuildMenu?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Menu.PauseMenu.performed          += (context) => {OnPauseMenu?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Menu.EnterChat.performed          += (context) => {OnEnterChat?.Invoke(this, EventArgs.Empty);};
    }

    public Vector2 GetMovementVectorNormalized() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta() {
        return playerInputActions.Player.Camera.ReadValue<Vector2>();
    }

    public bool GetIsSprinting() {
        return playerInputActions.Player.Sprint.IsPressed();
    }

    public int GetScrollDirection() {
        float scrollRaw = playerInputActions.Player.Scroll.ReadValue<float>();
        if (scrollRaw < 0) return -1;
        if (scrollRaw > 0) return 1;
        return 0;
    }

    public int GetRotateDirection() {
        int rotateDirection = 0;
        if (playerInputActions.Player.RotateLeft.IsPressed()) rotateDirection--;
        if (playerInputActions.Player.RotateRight.IsPressed()) rotateDirection++;
        return rotateDirection;
    }

    private void OnDestroy()
    {    
        OnJump = null;
        MainAction = null;
        SecondaryAction = null;
        OnDrop = null;
        OnBuildMenu = null;
        OnPauseMenu = null;
        OnScroll = null;
    }

    public void SetPlayerInputActive(bool active)
    {
        if (active) {
            playerInputActions.Player.Enable();
        } else {
            playerInputActions.Player.Disable();
        }
    }
}
