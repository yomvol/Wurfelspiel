using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

[Serializable]
public enum GameState
{
    Starting,
    PlayerTurn,
    OpponentTurn,
    Reroll,
    HandsEvaluation,
    Win,
    Lose
}

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }
    public CinemachineVirtualCamera diceZoomInCamera;
    public CinemachineTargetGroup diceTargetGroup;

    [SerializeField]
    private GameObject _player;
    private HumanPlayer _humanPlayer;
    [SerializeField]
    private GameObject _computerEnemy;
    private ComputerPlayer _computerPlayer;
    private Transform[] _humanPlayerDiceTransforms;
    private Transform[] _computerPlayerDiceTransforms;
    private int _roundNumber;
    private int _roundsWonByHuman = 0;
    private int _roundsWonByComputer = 0;

    // Kick the game off with the first state
    void Start() => StartCoroutine(ChangeState(GameState.Starting, 0));

    public IEnumerator ChangeState(GameState newState, float delayTime = 0f)
    {
        yield return new WaitForSeconds(delayTime);

        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState)
        {
            case GameState.Starting:
                HandleStarting();
                break;
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.Reroll:
                HandleReroll(false);
                break;
            case GameState.OpponentTurn:
                HandleOpponentTurn();
                break;
            case GameState.HandsEvaluation:
                HandleHandsEvaluation();
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);

        Debug.Log($"New state: {newState}");
    }
	
	public IEnumerator ChangeState(GameState newState, bool isHumanPlayerRerolling, float delayTime = 0f)
    {
        yield return new WaitForSeconds(delayTime);

        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState)
        {
            case GameState.Starting:
                HandleStarting();
                break;
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.Reroll:
                HandleReroll(isHumanPlayerRerolling);
                break;
            case GameState.OpponentTurn:
                HandleOpponentTurn();
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);

        Debug.Log($"New state: {newState}");
    }

    private void HandleStarting()
    {
        _roundNumber = 1;
        _humanPlayer = _player.GetComponent<HumanPlayer>();
        _computerPlayer = _computerEnemy.GetComponent<ComputerPlayer>();
        Transform[] humanDiceTransforms = _player.GetComponentsInChildren<Transform>();
        Transform[] compDiceTransforms = _computerEnemy.GetComponentsInChildren<Transform>();
        _humanPlayerDiceTransforms = new Transform[BasePlayer.NUMBER_OF_DICES];
        _computerPlayerDiceTransforms = new Transform[BasePlayer.NUMBER_OF_DICES];

        _humanPlayerDiceTransforms[0] = humanDiceTransforms[1];
        for (int i = 3; i < humanDiceTransforms.Length;)
        {
            _humanPlayerDiceTransforms[i / 2] = humanDiceTransforms[i];
            i += 2;
        }
        for (int k = 0; k < compDiceTransforms.Length; k++)
        {
            if (k == 0)
                continue;
            _computerPlayerDiceTransforms[k - 1] = compDiceTransforms[k];
        }

        StartCoroutine(ChangeState(GameState.PlayerTurn, 1.0f));
    }

    private void HandlePlayerTurn()
    {
        diceZoomInCamera.Priority = 5;
        _humanPlayer.KeepDicesNearby(true);
        _computerPlayer.KeepDicesNearby(false);
        foreach (var transform in _computerPlayerDiceTransforms)
        {
            diceTargetGroup.RemoveMember(transform);
        }
        foreach (var transform in _humanPlayerDiceTransforms)
        {
            diceTargetGroup.AddMember(transform, 1, 0);
        }

        _humanPlayer.ThrowDices();
    }

    private void HandleReroll(bool isHumanPlayerRerolling)
    {
        diceZoomInCamera.Priority = 11;

        // Every reroll is optional
        if (isHumanPlayerRerolling)
        {
			_humanPlayer.CurrentHighlightedDice = 0;
			_humanPlayer.isRerolling = true;
        }
        else
        {
            _computerPlayer.SelectAndReroll();
        }
    }

    private void HandleOpponentTurn()
    {
        diceZoomInCamera.Priority = 5;
        _humanPlayer.KeepDicesNearby(true);
        _computerPlayer.KeepDicesNearby(false);
        foreach (var transform in _humanPlayerDiceTransforms)
        {
            diceTargetGroup.RemoveMember(transform);
        }
        foreach (var transform in _computerPlayerDiceTransforms)
        {
            diceTargetGroup.AddMember(transform, 1, 0);
        }

        _computerPlayer.ThrowDices();
        StartCoroutine(ChangeState(GameState.Reroll, false));
    }

    private void HandleHandsEvaluation()
    {
        diceZoomInCamera.Priority = 5;
        var humanHandPower = _humanPlayer.hand.handPower;
        var computerHandPower = _computerPlayer.hand.handPower;
        if (humanHandPower.Item1 == computerHandPower.Item1)
        {
            if (humanHandPower.Item2 == computerHandPower.Item2)
            {
                if (humanHandPower.Item3 == computerHandPower.Item3)
                {
                    _roundsWonByHuman++;
                    _roundsWonByComputer++;
                }
                else if (humanHandPower.Item3 > computerHandPower.Item3)
                {
                    _roundsWonByHuman++;
                }
                else
                {
                    _roundsWonByComputer++;
                }
            }
            else if (humanHandPower.Item2 > computerHandPower.Item2)
            {
                _roundsWonByHuman++;
            }
            else
            {
                _roundsWonByComputer++;
            }
        }
        else if (humanHandPower.Item1 > computerHandPower.Item1)
        {
            _roundsWonByHuman++;
        }
        else // computer hand is stronger
        {
            _roundsWonByComputer++;
        }

        if (_roundNumber <= 2)
        {
            StartCoroutine(ChangeState(GameState.PlayerTurn));
        }
        else
        {
            if (_roundsWonByHuman == _roundsWonByComputer)
            {
                StartCoroutine(ChangeState(GameState.PlayerTurn));
            }
            else if (_roundsWonByHuman > _roundsWonByComputer)
            {
                StartCoroutine(ChangeState(GameState.Win));
            }
            else
            {
                StartCoroutine(ChangeState(GameState.Lose));
            }
        }

        _roundNumber++;
    }
}