using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanPlayerTest : BasePlayer
{
    private PlayerInput _playerInput;
    private InputAction _throwAction;
    private bool _areDicesFollowCursor = false;

    [HideInInspector] public bool IsWaitingToRoll;
    [HideInInspector] public bool IsRerolling;

    private void Start()
    {
        Initialize();
        _playerInput = GetComponent<PlayerInput>();
        _throwAction = _playerInput.actions["Throw"];
        _throwAction.performed += OnThrowStarted;
        _throwAction.canceled += OnThrowReleased;

        IsWaitingToRoll = true;
        // attach dices to cursor position
        _areDicesFollowCursor = true;
    }

    private void OnDisable()
    {
        _throwAction.performed -= OnThrowStarted;
        _throwAction.canceled -= OnThrowReleased;
    }

    protected override void ResultReadyAllDicesEventHandler(object sender, EventArgs e)
    {
        _resultsReceivedCounter++;

        if (_resultsReceivedCounter >= NUMBER_OF_DICES)
        {
            for (int i = 0; i < NUMBER_OF_DICES; i++)
            {
                Hand.Mask[i] = _rolls[i].RollResult;
                _rolls[i].ResultReadyEvent -= ResultReadyAllDicesEventHandler;
            }
            _resultsReceivedCounter = 0;
            EvaluateHand();
            if (GameManager.Instance.State == GameState.PlayerTurn)
            {
                UpdateUI((DiceFace[])Hand.Mask.Clone());
            }
            //StartCoroutine(GameManager.Instance.ChangeState(GameState.Reroll, true));
        }
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        if (IsWaitingToRoll)
        {
            Debug.Log("Shake!");
            for (int i = 0; i < NUMBER_OF_DICES; i++)
            {
                _rolls[i].StartShaking();
            }
        }
    }

    private void OnThrowReleased(InputAction.CallbackContext ctx)
    {
        if (IsWaitingToRoll)
        {
            _areDicesFollowCursor = false;
            IsWaitingToRoll = false;
            ThrowDices();
        }
    }

    public override void ThrowDices()
    {
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            _rolls[i].ResultReadyEvent += ResultReadyAllDicesEventHandler;
            _rolls[i].ApplyForces();
        }
    }

    private void Update()
    {
        if (_areDicesFollowCursor)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            }
        }
    }

    public override void SelectAndReroll()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateUI(DiceFace[] arr)
    {
        throw new NotImplementedException();
    }
}
