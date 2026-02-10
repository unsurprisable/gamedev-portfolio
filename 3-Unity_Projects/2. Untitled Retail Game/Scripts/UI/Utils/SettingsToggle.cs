using UnityEngine;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    public int State {
        get => toggle.isOn ? 1 : 0;
        set => toggle.isOn = value != 0;
    }
}
