using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _exitDialog;

    public void OnNewGameClick()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnExitClick()
    {
        _exitDialog.SetActive(!_exitDialog.activeSelf);
    }

    public void OnYesExitClick()
    {
        #if UNITY_STANDALONE
        Application.Quit();
        #endif
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }

    public void OnOptionsClick()
    {

    }
}
