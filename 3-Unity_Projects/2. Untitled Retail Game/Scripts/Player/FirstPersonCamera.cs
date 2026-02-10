using Unity.Netcode;
using UnityEngine;

public class FirstPersonCamera : NetworkBehaviour
{
    public static FirstPersonCamera LocalInstance { get; private set; }

    private float sensX;
    private float sensY;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraAnchor;

    [SerializeField] private float maxLookAngle;

    private float xRotation;
    private float yRotation;

    private bool isEnabled;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            LocalInstance = this;
        } else {
            enabled = false;
        }
    }

    private void Start()
    {
        xRotation = orientation.rotation.eulerAngles.x;
        yRotation = orientation.rotation.eulerAngles.y;

        SettingsMenuUI.Instance.OnSensitivityChanged += SettingsMenuUI_OnSensitivityChanged;
        SettingsMenuUI.Instance.OnFOVChanged += SettingsMenuUI_OnFOVChanged;
    }

    private void SettingsMenuUI_OnFOVChanged(object sender, SettingsMenuUI.OnSettingsValueChangedEventArgs e)
    {
        Camera.main.fieldOfView = e.intArgs[0];
    }

    private void SettingsMenuUI_OnSensitivityChanged(object sender, SettingsMenuUI.OnSettingsValueChangedEventArgs e)
    {
        sensX = e.intArgs[0] / 10f; // divide by 10 again so its a nicer number
        sensY = e.intArgs[1] / 10f; // divide by 10 again so its a nicer number
    }

    private void LateUpdate()
    {
        if (isEnabled)
        {
            Vector2 mouseInput = GameInput.Instance.GetMouseDelta();
            float mouseX = mouseInput.x * sensX / 10f; // divide by 10 just so sensitivity is a nicer number
            float mouseY = mouseInput.y * sensY / 10f; // divide by 10 just so sensitivity is a nicer number

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        }

        orientation.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Camera.main.transform.rotation = orientation.rotation;
        Camera.main.transform.position = cameraAnchor.position;
    }

    public void Enable(bool changeMouseState)
    {
        isEnabled = true;
        if (changeMouseState) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void Disable(bool changeMouseState)
    {
        isEnabled = false;
        if (changeMouseState) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}
