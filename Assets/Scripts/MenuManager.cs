using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _exitDialog;
    [SerializeField] TMP_Dropdown _resDropdown;
    [SerializeField] Texture2D[] _cursors;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _exitButton;
    private Resolution[] _resolutions;

    private void Start()
    {
        Cursor.visible = true;
        _resolutions = Screen.resolutions;
        _resDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentRes = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            var res = _resolutions[i];
            string option = res.width + " x " + res.height;
            options.Add(option);

            if (res.width == Screen.width && res.height == Screen.height) currentRes = i;
        }

        _resDropdown.AddOptions(options);
        _resDropdown.value = currentRes;
        _resDropdown.RefreshShownValue();
    }

    public void OnNewGameClick()
    {
        LevelManager.Instance.LoadScene("Main");
        AudioManager.Instance.PlayEffect("UI_Confirm");
    }

    public void OnExitClick()
    {
        _exitDialog.SetActive(!_exitDialog.activeSelf);
        AudioManager.Instance.PlayEffect("UI_Move");
    }

    public void OnYesExitClick()
    {
        #if UNITY_STANDALONE
        Application.Quit();
        #endif
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
        AudioManager.Instance.PlayEffect("UI_Confirm");
    }

    public void OnOptionsClick()
    {
        AudioManager.Instance.PlayEffect("UI_Move");
        _newGameButton.interactable = !_newGameButton.interactable;
        _exitButton.interactable = !_exitButton.interactable;
    }

    public void SetMasterVolume(float volume)
    {
        AudioManager.Instance.Mixer.SetFloat("master", Mathf.Log10(volume) * 20f);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.Mixer.SetFloat("music", Mathf.Log10(volume) * 20f);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }
    public void SetSFXVolume(float volume)
    {
        AudioManager.Instance.Mixer.SetFloat("sfx", Mathf.Log10(volume) * 20f);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }
    public void SetUIVolume(float volume)
    {
        AudioManager.Instance.Mixer.SetFloat("ui", Mathf.Log10(volume) * 20f);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }

    public void SetFullscreen(bool toggle)
    {
        Screen.fullScreen = toggle;
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }

    public void SetResolution(int resIndex)
    {
        var res = _resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        AudioManager.Instance.PlayEffect("UI_Twinkle");
    }

    public void OnHoverIn()
    {
        Cursor.SetCursor(_cursors[1], Vector2.zero, CursorMode.Auto);
    }

    public void OnHoverOut()
    {
        Cursor.SetCursor(_cursors[0], Vector2.zero, CursorMode.Auto);
    }
}
