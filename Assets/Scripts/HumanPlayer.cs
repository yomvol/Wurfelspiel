using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanPlayer : BasePlayer
{
    private SpriteRenderer[] _selectionRingRenderers;
    private Color32 _selectedCol;
    private Color32 _selectedAndCursorHoveringCol;
    
    public int CurrentHighlightedDice;
    [HideInInspector]
    public bool isRerolling;

    protected override void Initialize()
    {
        base.Initialize();
        _selectionRingRenderers = new SpriteRenderer[NUMBER_OF_DICES];
        _selectedCol = new Color32(255, 120, 0, 255);
        _selectedAndCursorHoveringCol = new Color32(255, 49, 49, 255);

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            _selectionRingRenderers[i] = hand.dices[i].GetComponentInChildren<SpriteRenderer>();
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
                hand.mask[i] = _rolls[i].rollResult;
                _rolls[i].resultReadyEvent -= ResultReadyAllDicesEventHandler;
            }
            _resultsReceivedCounter = 0;
            StartCoroutine(GameManager.Instance.ChangeState(GameState.Reroll, true));
        }
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
            isRerolling = false;
            currentRenderer.enabled = false;
            if (_dicesToReroll.Count > 0)
            {
                GameManager.Instance.diceZoomInCamera.Priority = 11;
                SpriteRenderer renderer;
                for (int i = 0; i < _dicesToReroll.Count; i++)
                {
                    renderer = _dicesToReroll[i].gameObject.GetComponentInChildren<SpriteRenderer>();
                    renderer.color = Color.white;
                    renderer.enabled = false;
                    _dicesToReroll[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
                    _dicesToReroll[i].resultReadyEvent += ResultReadyRerollDicesEventHandler;
                    _dicesToReroll[i].ThrowDice();
                }
            }
            else
            {
                EvaluateHand();
                Debug.Log($"{gameObject.name} got {hand.handPower.Item1} of {hand.handPower.Item2}");
            }
            
            StartCoroutine(GameManager.Instance.ChangeState(GameState.OpponentTurn, 3f));
        }
    }

    private void Update()
    {
        if (isRerolling)
        {
            SelectAndReroll();
        }
    }
}
