using System;
using UnityEngine;

[Serializable]
public enum GameState
{
    Starting,
    PlayerTurn,
    OpponentTurn,
    HandsEvaluation,
    Win,
    Draw,
    Lose
}

public class GameManager : MonoBehaviour
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }

    // Kick the game off with the first state
    void Start() => ChangeState(GameState.Starting);

    public void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState)
        {
            case GameState.Starting:
                HandleStarting();
                break;
            case GameState.PlayerTurn:

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
        // Do some start setup, could be environment, cinematics etc


        ChangeState(GameState.PlayerTurn);
    }

    private void HandlePlayerTurn()
    {


        ChangeState(GameState.OpponentTurn);
    }
}