﻿using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _exitDialog;
    [SerializeField] TMP_Dropdown _resDropdown;
    private Resolution[] _resolutions;

    private void Start()
    {
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
}
