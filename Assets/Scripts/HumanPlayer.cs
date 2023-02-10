using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanPlayer : BasePlayer
{
    private SpriteRenderer[] _selectionRingRenderers;
    private Color32 _selectedCol;
    private Color32 _selectedAndCursorHoveringCol;
    private int _holdSurrenderCounter = 0;

    [HideInInspector] public int CurrentHighlightedDice;
    [HideInInspector] public bool IsWaitingToRoll;
    [HideInInspector] public bool IsRerolling;

    protected override void Initialize()
    {
        base.Initialize();
        _selectionRingRenderers = new SpriteRenderer[NUMBER_OF_DICES];
        _selectedCol = new Color32(255, 120, 0, 255);
        _selectedAndCursorHoveringCol = new Color32(255, 49, 49, 255);

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            _selectionRingRenderers[i] = Hand.Dices[i].GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        Initialize();
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
        CanvasManager.Instance.PlayerHandCombinationName.text = Hand.HandPower.Item1.ToString();
    }

    public override void SelectAndReroll()
    {
        SpriteRenderer currentRenderer = _selectionRingRenderers[CurrentHighlightedDice];
        currentRenderer.transform.rotation = Quaternion.LookRotation(transform.up, transform.forward);
        currentRenderer.enabled = true;
        
        if (Keyboard.current.eKey.wasPressedThisFrame)
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
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (!_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
            {
                currentRenderer.enabled = false;
            }
            else
            {
                currentRenderer.color = _selectedCol;
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
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (!_dicesToReroll.Contains(_rolls[CurrentHighlightedDice]))
            {
                currentRenderer.enabled = false;
            }
            else
            {
                currentRenderer.color = _selectedCol;
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
        else if (Keyboard.current.fKey.wasPressedThisFrame)
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
                    _dicesToReroll[i].ThrowDice();
                }
            }
            else
            {
                Debug.Log($"{gameObject.name} got {Hand.HandPower.Item1} of {Hand.HandPower.Item2}");
            }
            
            StartCoroutine(GameManager.Instance.ChangeState(GameState.OpponentTurn, 4f));
        }
    }

    private void Update()
    {
        if (Keyboard.current.lKey.isPressed)
        {
            _holdSurrenderCounter++;
            // TODO add some kind of indicator of button holding, make it depend on time
            if (_holdSurrenderCounter > 120)
            {
                Debug.Log("You have surrendered.");
                StartCoroutine(GameManager.Instance.ChangeState(GameState.Defeat));
                return;
            }
        }

        if (IsWaitingToRoll && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            IsWaitingToRoll = false;
            ThrowDices();
        }

        if (IsRerolling)
        {
            SelectAndReroll();
        }
    }
}
