using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanPlayer : BasePlayer
{
    private PlayerInput _playerInput;
    private InputAction _throwStartAction;
    private InputAction _throwReleasedAction;
    private InputAction _confirmAction;
    private InputAction _throwSelectedAction;
    private InputAction _surrenderAction;
    private InputAction _MenuAction;
    private InputAction _cycleAction;

    private bool _areDicesFollowCursor = false;
    private SpriteRenderer[] _selectionRingRenderers;
    private Vector3 _initPos;

    [SerializeField] private Color32 _selectedColor;
    [SerializeField] private Color32 _selectedAndCursorHoveringCol;
    [SerializeField] private LayerMask _raycastWhiteList;

    [HideInInspector] public int CurrentHighlightedDice;
    [HideInInspector] public bool IsWaitingToRoll;
    [HideInInspector] public bool IsRerolling;

    protected override void Initialize()
    {
        base.Initialize();
        _initPos = transform.position;

        _playerInput = GetComponent<PlayerInput>();
        _throwStartAction = _playerInput.actions["ThrowStart"];
        _throwReleasedAction = _playerInput.actions["ThrowReleased"];
        _confirmAction = _playerInput.actions["ConfirmSelection"];
        _throwSelectedAction = _playerInput.actions["ThrowSelected"];
        _surrenderAction = _playerInput.actions["Surrender"];
        _MenuAction = _playerInput.actions["Menu"];
        _cycleAction = _playerInput.actions["CycleSelection"];
        _throwStartAction.performed += OnThrowStarted;
        _throwReleasedAction.performed += OnThrowReleased;
        _surrenderAction.performed += OnSurrender;
        _MenuAction.performed += OnMenu;
        
        _selectionRingRenderers = new SpriteRenderer[NUMBER_OF_DICES];

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            _selectionRingRenderers[i] = Hand.Dices[i].GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        _throwStartAction.performed -= OnThrowStarted;
        _throwReleasedAction.performed -= OnThrowReleased;
        _surrenderAction.performed -= OnSurrender;
        _MenuAction.performed -= OnMenu;
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
            Debug.Log("Player Hand: " + Hand.ToString());
            if (GameManager.Instance.State == GameState.PlayerTurn)
            {
                UpdateUI((DiceFace[])Hand.Mask.Clone());
            }
            StartCoroutine(GameManager.Instance.ChangeState(GameState.Reroll, true));
        }
    }

    protected override void UpdateUI(DiceFace[] arr)
    {
        Array.Sort(arr);
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            switch (arr[i])
            {
                case DiceFace.One:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[0];
                    break;
                case DiceFace.Two:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[1];
                    break;
                case DiceFace.Three:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[2];
                    break;
                case DiceFace.Four:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[3];
                    break;
                case DiceFace.Five:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[4];
                    break;
                case DiceFace.Six:
                    CanvasManager.Instance.PlayerDiceIcons[i].sprite = CanvasManager.Instance.WhiteDiceSprites[5];
                    break;
            }
        }
        CanvasManager.Instance.PlayerHandCombinationName.text = Hand.HandPower.Item1.ToString().Replace('_', ' ');
    }

    public override void SelectAndReroll()
    {
        SpriteRenderer currentRenderer = _selectionRingRenderers[CurrentHighlightedDice];
        currentRenderer.transform.rotation = Quaternion.LookRotation(transform.up, transform.forward);
        currentRenderer.enabled = true;

        if (_confirmAction.triggered)
        {
            if (!_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
            {
                currentRenderer.color = _selectedAndCursorHoveringCol;
                _dicesToReroll.Add(_rolls[CurrentHighlightedDice]);
            }
            else
            {
                currentRenderer.color = Color.white;
                _dicesToReroll.Remove(_rolls[CurrentHighlightedDice]);
            }
        }
        else if (_cycleAction.triggered) // d key
        {
            Vector2 vectorInput = _cycleAction.ReadValue<Vector2>();

            if (vectorInput == Vector2.right)
            {
                if (!_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
                {
                    currentRenderer.enabled = false;
                }
                else
                {
                    currentRenderer.color = _selectedColor;
                }

                CurrentHighlightedDice++;
                if (CurrentHighlightedDice >= NUMBER_OF_DICES)
                {
                    CurrentHighlightedDice = 0;
                }

                if (_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
                {
                    _selectionRingRenderers[CurrentHighlightedDice].color = _selectedAndCursorHoveringCol;
                }
            }
            else if (vectorInput == Vector2.left)
            {
                if (!_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
                {
                    currentRenderer.enabled = false;
                }
                else
                {
                    currentRenderer.color = _selectedColor;
                }

                CurrentHighlightedDice--;
                if (CurrentHighlightedDice < 0)
                {
                    CurrentHighlightedDice = NUMBER_OF_DICES - 1;
                }

                if (_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
                {
                    _selectionRingRenderers[CurrentHighlightedDice].color = _selectedAndCursorHoveringCol;
                }
            }
        }
        else if (_throwSelectedAction.triggered)
        {
            IsRerolling = false;
            currentRenderer.enabled = false;
            if (_dicesToReroll.Count > 0)
            {
                SpriteRenderer renderer;
                for (int i = 0; i < _dicesToReroll.Count; i++)
                {
                    renderer = _dicesToReroll[i].gameObject.GetComponentInChildren<SpriteRenderer>();
                    renderer.color = Color.white;
                    renderer.enabled = false;
                    _dicesToReroll[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
                    _dicesToReroll[i].ResultReadyEvent += ResultReadyRerollDicesEventHandler;
                    _dicesToReroll[i].ThrowDiceShaken();
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} got {Hand.HandPower.Item1} of {Hand.HandPower.Item2}");
            }

            StartCoroutine(GameManager.Instance.ChangeState(GameState.OpponentTurn, 4f));
        }
    }

    private void ReturnToInitPos()
    {
        transform.position = _initPos;
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        if (IsWaitingToRoll)
        {
            _areDicesFollowCursor = true;
            for (int i = 0; i < NUMBER_OF_DICES; i++)
            {
                _rolls[i].StartShaking();
            }
        }
    }

    private void OnThrowReleased(InputAction.CallbackContext ctx)
    {
        if (IsWaitingToRoll && _areDicesFollowCursor)
        {
            _areDicesFollowCursor = false;
            IsWaitingToRoll = false;
            ReturnToInitPos();
            for (int i = 0; i < NUMBER_OF_DICES; i++)
            {
                _rolls[i].ResultReadyEvent += ResultReadyAllDicesEventHandler;
                _rolls[i].ApplyForces();
                #if !DEBUG
                AudioManager.Instance.Play("Dice_Thrown");
                #endif
            }
        }
    }

    private void OnSurrender(InputAction.CallbackContext ctx)
    {
        Debug.Log("You have surrendered.");
        _areDicesFollowCursor = false;
        IsWaitingToRoll = false;
        ReturnToInitPos();
        StartCoroutine(GameManager.Instance.ChangeState(GameState.Defeat));
    }

    private void OnMenu(InputAction.CallbackContext ctx)
    {
        // TODO implement menu call
        // resume, return to main menu, exit to desktop etc
    }

    private void Update()
    {
        if (IsRerolling)
        {
            SelectAndReroll();
        }
    }

    private void FixedUpdate()
    {
        if (_areDicesFollowCursor)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, _raycastWhiteList))
            {
                if (hit.transform.CompareTag("TableTop"))
                {
                    transform.position = new Vector3(hit.point.x, 1.2f, hit.point.z);
                }
            }
        }
    }
}
