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
    HandsComparing,
    Win,
    Defeat
}

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }
    public CinemachineVirtualCamera DiceZoomInCamera;
    public CinemachineTargetGroup DiceTargetGroup;

    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _computerEnemy;
    [SerializeField] private Sound _winSFX;
    [SerializeField] private Sound _loseSFX;

    private HumanPlayer _humanPlayer;
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
            case GameState.HandsComparing:
                HandleHandsComparing();
                break;
            case GameState.Win:
                HandleWin();
                break;
            case GameState.Defeat:
                HandleDefeat();
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
            case GameState.HandsComparing:
                HandleHandsComparing();
                break;
            case GameState.Win:
                HandleWin();
                break;
            case GameState.Defeat:
                HandleDefeat();
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

        int i = 0;
        foreach (var transf in humanDiceTransforms)
        {
            if (transf.gameObject.CompareTag("Dice"))
            {
                _humanPlayerDiceTransforms[i] = transf;
                i++;
            }
        }
        int j = 0;
        foreach (var transf in compDiceTransforms)
        {
            if (transf.gameObject.CompareTag("Dice"))
            {
                _computerPlayerDiceTransforms[j] = transf;
                j++;
            }
        }

        StartCoroutine(CanvasManager.Instance.ShowAnnoucement("Round " + _roundNumber));
        StartCoroutine(ChangeState(GameState.PlayerTurn, 1.0f));
    }

    private void HandlePlayerTurn()
    {
        DiceZoomInCamera.Priority = 5;
        _humanPlayer.KeepDices();
        _computerPlayer.KeepDices();
        foreach (var transform in _computerPlayerDiceTransforms)
        {
            DiceTargetGroup.RemoveMember(transform);
        }
        foreach (var transform in _humanPlayerDiceTransforms)
        {
            DiceTargetGroup.AddMember(transform, 1, 0);
        }

        _humanPlayer.IsWaitingToRoll = true;
    }

    private void HandleReroll(bool isHumanPlayerRerolling)
    {
        DiceZoomInCamera.Priority = 11;

        // Every reroll is optional
        if (isHumanPlayerRerolling)
        {
            _humanPlayer.CurrentHighlightedDice = 0;
            _humanPlayer.IsRerolling = true;
        }
        else
        {
            _computerPlayer.SelectAndReroll();
        }
    }

    private void HandleOpponentTurn()
    {
        DiceZoomInCamera.Priority = 5;
        _humanPlayer.KeepDices();
        _computerPlayer.KeepDices();
        foreach (var transform in _humanPlayerDiceTransforms)
        {
            DiceTargetGroup.RemoveMember(transform);
        }
        foreach (var transform in _computerPlayerDiceTransforms)
        {
            DiceTargetGroup.AddMember(transform, 1, 0);
        }

        _computerPlayer.ThrowDices();
    }

    private void HandleHandsComparing()
    {
        DiceZoomInCamera.Priority = 5;
        var humanHandPower = _humanPlayer.Hand.HandPower;
        var computerHandPower = _computerPlayer.Hand.HandPower;
        if (humanHandPower.Item1 == computerHandPower.Item1) // hand combinations are the same
        {
            if (humanHandPower.Item2 == computerHandPower.Item2) // if senior kickers are equal
            {
                if (humanHandPower.Item3 == computerHandPower.Item3) // and if junior kickers are equal as well
                {
                    _roundsWonByHuman++;
                    _roundsWonByComputer++;
                    StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"Round {_roundNumber} ended with a draw!\n " +
                        $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
                }
                else if (humanHandPower.Item3 > computerHandPower.Item3)
                {
                    _roundsWonByHuman++;
                    StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"You won round {_roundNumber}!\n " +
                        $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
                }
                else
                {
                    _roundsWonByComputer++;
                    StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"Opponent has defeated you in round {_roundNumber}!\n " +
                        $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
                }
            }
            else if (humanHandPower.Item2 > computerHandPower.Item2) // human senior kicker is stronger
            {
                _roundsWonByHuman++;
                StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"You won round {_roundNumber}!\n " +
                    $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
            }
            else
            {
                _roundsWonByComputer++;
                StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"Opponent has defeated you in round {_roundNumber}!\n " +
                    $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
            }
        }
        else if (humanHandPower.Item1 > computerHandPower.Item1) // human combination is better
        {
            _roundsWonByHuman++;
            StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"You won round {_roundNumber}!\n " +
                $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
        }
        else // computer hand is stronger
        {
            _roundsWonByComputer++;
            StartCoroutine(CanvasManager.Instance.ShowAnnoucement($"Opponent has defeated you in round {_roundNumber}!\n " +
                $"Your hand: {humanHandPower.Item1} vs Opponent hand: {computerHandPower.Item1}", _roundNumber + 1));
        }

        if (_roundNumber <= 2 && _roundsWonByHuman < 2 && _roundsWonByComputer < 2)
        {
            StartCoroutine(ChangeState(GameState.PlayerTurn));
        }
        else // it`s time to conclude the results
        {
            if (_roundsWonByHuman == _roundsWonByComputer) // one more round must be played
            {
                StartCoroutine(CanvasManager.Instance.ShowAnnoucement("One more round must be played!", _roundNumber + 1));
                StartCoroutine(ChangeState(GameState.PlayerTurn));
            }
            else if (_roundsWonByHuman > _roundsWonByComputer)
            {
                StartCoroutine(ChangeState(GameState.Win, 1f));
            }
            else
            {
                StartCoroutine(ChangeState(GameState.Defeat, 1f)); // you lose :(
            }
        }

        _roundNumber++;
        CanvasManager.Instance.UpdateRoundsInfo(_roundsWonByHuman, _roundsWonByComputer);
    }

    private void HandleWin()
    {
        #if !DEBUG
        AudioManager.Instance.PlayEffect(_winSFX);
        #endif
        StartCoroutine(CanvasManager.Instance.ShowAnnoucement("You won!"));
    }

    private void HandleDefeat()
    {
        #if !DEBUG
        AudioManager.Instance.PlayEffect(_loseSFX);
        #endif
        StartCoroutine(CanvasManager.Instance.ShowAnnoucement("You lost!"));
    }
}