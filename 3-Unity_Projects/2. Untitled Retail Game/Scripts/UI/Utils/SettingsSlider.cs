using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private TMP_InputField valueInputField;
    [SerializeField] private Slider slider;
    public float Value {
        get => slider.value;
        set { slider.value = value; SyncInputFieldFromSlider(value); }
    }

    private void Awake()
    {
        valueInputField.characterLimit = 3;
        valueInputField.lineLimit = 1;

        valueInputField.onValidateInput += ValidateInput;
        valueInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        valueInputField.onDeselect.AddListener(FinalizeInputField);
        valueInputField.onSubmit.AddListener((text) => { EventSystem.current.SetSelectedGameObject(null); });
    }

    private void OnInputFieldValueChanged(string text)
    {
        if (int.TryParse(text, out int result))
        {
            if (result > slider.minValue && result < slider.maxValue) 
                SyncSliderFromInputField();
        }
        else if (text != "")
        {
            SyncInputFieldFromSlider();
        }
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (!int.TryParse(addedChar.ToString(), out _)) {
            return '\0'; // not an integer
        }
        return addedChar;
    }

    private void FinalizeInputField(string text)
    {
        if (text == "" || !int.TryParse(text, out int result)) {
            SyncInputFieldFromSlider();
        } else {
            if (result > slider.maxValue) {
                valueInputField.text = slider.maxValue.ToString();
            } else if (result < slider.minValue) {
                valueInputField.text = slider.minValue.ToString();
            }
        }
        SyncSliderFromInputField();
    }

    public void SyncSliderFromInputField()
    {
        slider.value = int.Parse(valueInputField.text);
    }
    public void SyncInputFieldFromSlider(float newValue = -1)
    {
        if (newValue == -1) {
            valueInputField.text = slider.value.ToString();
        } else {
            valueInputField.text = newValue.ToString();
        }
    }
}
