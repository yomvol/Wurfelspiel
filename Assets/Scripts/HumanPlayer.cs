using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanPlayer : BasePlayer
{
    // these components handle logic interacting with a dice
    private DiceRoll[] _rolls;
    private int _resultsReceivedCounter;
    private SpriteRenderer[] _selectionRingRenderers;
    private List<DiceRoll> _dicesToReroll;
    private Color32 _selectedCol;
    private int _currentHighlightedDice;

    protected override void Initialize()
    {
        base.Initialize();
        _rolls = new DiceRoll[NUMBER_OF_DICES];
        _selectionRingRenderers = new SpriteRenderer[NUMBER_OF_DICES];
        _dicesToReroll = new List<DiceRoll>();
        _selectedCol = new Color32(255, 49, 49, 255);
        _currentHighlightedDice = 0;
        _resultsReceivedCounter = 0;

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            hand.mask[i] = DiceFace.Two;
            hand.dices[i] = Instantiate(dicePrefab, transform.position + new Vector3(0, 0, i * 0.5f), Quaternion.identity, transform);
            _rolls[i] = hand.dices[i].GetComponent<DiceRoll>();
            _selectionRingRenderers[i] = hand.dices[i].GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void ResultReadyEventHandler(object sender, EventArgs e)
    {
        _resultsReceivedCounter++;

        if (_resultsReceivedCounter >= NUMBER_OF_DICES)
        {
            for (int i = 0; i < NUMBER_OF_DICES; i++)
            {
                hand.mask[i] = _rolls[i].rollResult;
                _rolls[i].resultReadyEvent -= ResultReadyEventHandler;
            }
            _resultsReceivedCounter = 0;
            EvaluateHand();
            Debug.Log($"You`ve got {hand.handPower.Item1} of {hand.handPower.Item2}");
        }
    }

    public override void ThrowDices()
    {
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            _rolls[i].resultReadyEvent += ResultReadyEventHandler;
            _rolls[i].ThrowDice();
        }
    }

    protected override void SelectAndReroll()
    {
        SpriteRenderer currentRenderer = _selectionRingRenderers[_currentHighlightedDice];
        currentRenderer.transform.rotation = Quaternion.LookRotation(transform.up, transform.forward);
        currentRenderer.enabled = true;
        
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            _selectionRingRenderers[_currentHighlightedDice].color = _selectedCol;
            Debug.Log(_selectedCol);
            Debug.Log(_selectionRingRenderers[_currentHighlightedDice].color);
            _dicesToReroll.Add(_rolls[_currentHighlightedDice]);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (!_dicesToReroll.Contains(_rolls[_currentHighlightedDice]))
            {
                _selectionRingRenderers[_currentHighlightedDice].enabled = false;
            }
            _currentHighlightedDice++;
            if (_currentHighlightedDice >= NUMBER_OF_DICES)
            {
                _currentHighlightedDice = 0;
            }
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (!_dicesToReroll.Contains(_rolls[_currentHighlightedDice]))
            {
                _selectionRingRenderers[_currentHighlightedDice].enabled = false;
            }
            _currentHighlightedDice--;
            if (_currentHighlightedDice < 0)
            {
                _currentHighlightedDice = NUMBER_OF_DICES - 1;
            }
        }
        else if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            isRerolling = false;
            GameManager.Instance.diceZoomInCamera.Priority = 5;
            SpriteRenderer renderer;
            foreach (var dice in _dicesToReroll)
            {
                renderer = dice.gameObject.GetComponentInChildren<SpriteRenderer>();
                renderer.color = Color.white;
                renderer.enabled = false;
                dice.transform.position = transform.position;
                dice.ThrowDice();
            }
            GameManager.Instance.diceZoomInCamera.Priority = 11;
            StartCoroutine(GameManager.Instance.ChangeState(GameState.Win));
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
