using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenuUI : SubMenu
{
    public static SettingsMenuUI Instance { get; private set; }

    public class OnSettingsValueChangedEventArgs : EventArgs {
        public float[] floatArgs;
        public int[] intArgs;
        public string[] stringArgs;
    }

    public event EventHandler<OnSettingsValueChangedEventArgs> OnSensitivityChanged;
    [SerializeField] private SettingsSlider sensX;
    private int SensX => (int)sensX.Value;
    [SerializeField] private SettingsSlider sensY;
    private int SensY => (int)sensY.Value;

    public const string PLAYER_PREFS_SENS_X = "MouseSensitivityX";
    public const string PLAYER_PREFS_SENS_Y = "MouseSensitivityY";

    private void InvokeOnSensitivityChanged() {
        OnSensitivityChanged?.Invoke(this, new OnSettingsValueChangedEventArgs {
            intArgs = new int[]{SensX, SensY}
        });
    }

    public event EventHandler<OnSettingsValueChangedEventArgs> OnFOVChanged;
    [SerializeField] private SettingsSlider fov;
    private int FOV => (int)fov.Value;

    public const string PLAYER_PREFS_FOV = "CameraFOV";

    private void InvokeOnFOVChanged() {
        OnFOVChanged?.Invoke(this, new OnSettingsValueChangedEventArgs {
            intArgs = new int[]{FOV}
        });
    }

    [SerializeField] private SettingsSlider fpsLimit;
    private int FpsLimit => (int)fpsLimit.Value;
    [SerializeField] private SettingsToggle vsync;
    private int VSync => vsync.State;
    
    public const string PLAYER_PREFS_FPS_LIMIT = "FpsLimit";
    public const string PLAYER_PREFS_USE_VSYNC = "UseVSync";

    private void InvokeOnFpsChanged() {
        QualitySettings.vSyncCount = VSync;
        Application.targetFrameRate = FpsLimit;
    }

    [SerializeField] private SettingsDropdown resolution;
    [SerializeField] private SettingsToggle fullscreen;
    private int Fullscreen => (int)fullscreen.State;
    [SerializeField] private SettingsToggle borderless;
    private int Borderless => (int)borderless.State;
    private List<Resolution> filteredResolutions;

    public void InvokeOnDisplaySettingsChanged() {
        Screen.fullScreen = Fullscreen == 1;
        if (Fullscreen == 1) {
            if (Borderless == 1) {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            } else {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }
            Screen.fullScreen = true;
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }


    public void OnPlayerSpawned() {
        LoadSettings();
    }

    private void Awake()
    {
        Instance = this;

        InitializeResolutionDropdown();

        SetMissingSettingsToDefault();
    }

    protected override void OnShow()
    {
        LoadSettings();
    }
    protected override void OnHide()
    {
        SaveSettings();
    }

    #region PlayerPrefs

    private void SetMissingSettingsToDefault()
    {
        SetMissingInt(PLAYER_PREFS_SENS_X, 10);
        SetMissingInt(PLAYER_PREFS_SENS_Y, 8);

        SetMissingInt(PLAYER_PREFS_FOV, 70);

        SetMissingInt(PLAYER_PREFS_FPS_LIMIT, 240);
        SetMissingInt(PLAYER_PREFS_USE_VSYNC, 1);

        PlayerPrefs.Save();
    }
    private void SetMissingInt(string key, int value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetInt(key, value);
        }
    }
    private void SetMissingFloat(string key, float value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetFloat(key, value);
        }
    }
    private void SetMissingString(string key, string value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetString(key, value);
        }
    }


    private void LoadSettings()
    {
        sensX.Value = PlayerPrefs.GetInt(PLAYER_PREFS_SENS_X);
        sensY.Value = PlayerPrefs.GetInt(PLAYER_PREFS_SENS_Y);
        InvokeOnSensitivityChanged();

        fov.Value = PlayerPrefs.GetInt(PLAYER_PREFS_FOV);
        InvokeOnFOVChanged();

        fpsLimit.Value = PlayerPrefs.GetInt(PLAYER_PREFS_FPS_LIMIT);
        vsync.State = PlayerPrefs.GetInt(PLAYER_PREFS_USE_VSYNC);
        InvokeOnFpsChanged();

        fullscreen.State = Screen.fullScreen ? 1 : 0;
        borderless.State = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? 1 : 0;
    }
    public void SaveSettings()
    {
        Debug.Log("Writing settings to PlayerPrefs...");

        PlayerPrefs.SetInt(PLAYER_PREFS_SENS_X, SensX);
        PlayerPrefs.SetInt(PLAYER_PREFS_SENS_Y, SensY);
        InvokeOnSensitivityChanged();

        PlayerPrefs.SetInt(PLAYER_PREFS_FOV, FOV);
        InvokeOnFOVChanged();

        PlayerPrefs.SetInt(PLAYER_PREFS_FPS_LIMIT, FpsLimit);
        PlayerPrefs.SetInt(PLAYER_PREFS_USE_VSYNC, VSync);
        InvokeOnFpsChanged();

        PlayerPrefs.Save();
    }

    #endregion
    
    private void InitializeResolutionDropdown()
    {
        int currentResolutionIndex = 0;

        var resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolution.dropdown.ClearOptions();
        var currentRefreshRate = Screen.currentResolution.refreshRateRatio;
        
        foreach (Resolution res in resolutions)
        {
            if (res.refreshRateRatio.Equals(currentRefreshRate))
            {
                filteredResolutions.Add(res);
            }
        }
        
        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            var resolution = filteredResolutions[i];
            string resolutionOption = $"{resolution.width} x {resolution.height}";
            options.Add(resolutionOption);
            if (resolution.width == Screen.width && resolution.height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolution.dropdown.AddOptions(options);
        resolution.dropdown.value = currentResolutionIndex;
        resolution.dropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
