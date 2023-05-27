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
    [SerializeField] private float _rotationSpeed;
    private float _target;
    private float _prevRotationTime = 0f;

    protected override void Awake()
    {
        base.Awake();

        Debug.Log("Level Manager has AWAKENED!");
        Debug.Log(SceneManager.GetActiveScene().name);

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            _mainCam = Camera.main;
            _menuCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            _loadingCanvas = GameObject.Find("LoadingCanvas").GetComponent<Canvas>();
            _rotatingIndicator = _loadingCanvas.transform.GetChild(1).gameObject.GetComponent<Image>();
            _prompt = _loadingCanvas.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
            _loadingCanvas.gameObject.SetActive(false);
            Debug.Log("Tied everything: " + _prompt.text);
        }
    }

    public async void LoadScene(string sceneName)
    {
        Cursor.visible = false;
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

        await Task.Delay(5000);

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
        if (_rotatingIndicator != null && _rotatingIndicator.isActiveAndEnabled && Time.time - _prevRotationTime > _rotationSpeed)
        {
            _prevRotationTime = Time.time;

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
