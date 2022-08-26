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
    Draw,
    Lose
}

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }
    public GameObject player;
    public CinemachineVirtualCamera diceZoomInCamera;
    public CinemachineTargetGroup diceTargetGroup;

    private HumanPlayer _humanPlayer;

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
                HandleReroll();
                break;
            case GameState.Win:
                break;
            case GameState.Draw:
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
        _humanPlayer = player.GetComponent<HumanPlayer>();
        Transform[] diceTransforms = player.GetComponentsInChildren<Transform>();
        foreach(var transform in diceTransforms)
        {
            if (transform == diceTransforms[0])
                continue;
            diceTargetGroup.AddMember(transform, 1, 0);
        }

        StartCoroutine(ChangeState(GameState.PlayerTurn, 1.0f));
    }

    private void HandlePlayerTurn()
    {
        _humanPlayer.ThrowDices();
        diceZoomInCamera.Priority = 11;

        StartCoroutine(ChangeState(GameState.Reroll));
    }

    private void HandleReroll()
    {
        // Every reroll is optional
        _humanPlayer.isRerolling = true;
    }
}