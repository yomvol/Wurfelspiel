using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : PersistentSingleton<LevelManager>
{
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Canvas _menuCanvas;
    [SerializeField] private Canvas _loadingCanvas;
    [SerializeField] private Image _rotatingIndicator;
    [SerializeField] private TextMeshProUGUI _prompt;
    private float _target;

    public async void LoadScene(string sceneName)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        if (_menuCanvas != null)
        {
            _menuCanvas.gameObject.SetActive(false);
            _mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            _loadingCanvas.gameObject.SetActive(true);
        }

        do
        {
            //await Task.Delay(100);
            _target = scene.progress;
        } while (scene.progress < 0.9f);

        _prompt.text = "Press any key";
        _target = 1.0f;
        var tcs = new TaskCompletionSource<bool>();

        Action<InputControl> onAnyButtonPressed = delegate (InputControl ctrl)
        {
            //Debug.Log($"{ctrl} pressed");
            scene.allowSceneActivation = true;
            tcs.TrySetResult(true);
        };

        InputSystem.onAnyButtonPress.CallOnce(onAnyButtonPressed);
        await tcs.Task;
    }

    private void Update()
    {
        if (_rotatingIndicator != null && _rotatingIndicator.isActiveAndEnabled)
        {
            if (_target < 1.0f)
            {
                _rotatingIndicator.transform.RotateAround(_rotatingIndicator.transform.position, Vector3.back, _target * 360f);
            }
            else
            {
                _rotatingIndicator.transform.rotation = Quaternion.identity;
            }
        }
    }
}
