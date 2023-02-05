using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("Hand info")]
    public TextMeshProUGUI OpponentHandCombinationName;
    public TextMeshProUGUI PlayerHandCombinationName;
    public Image[] OpponentDiceIcons;
    public Image[] PlayerDiceIcons;
    public Sprite[] WhiteDiceSprites;
    public Sprite[] RedDiceSprites;

    [Header("Rounds info")]
    [SerializeField] private Image[] VictoryIndicators;


    public void UpdateRoundsInfo(int roundsWonByHuman, int roundsWonByComputer)
    {
        Color col;
        if (roundsWonByHuman > 0 && roundsWonByHuman < 3)
        {
            col = VictoryIndicators[roundsWonByHuman - 1].color;
            col.a = 1;
            VictoryIndicators[roundsWonByHuman - 1].color = col;
        }

        if (roundsWonByComputer > 0 && roundsWonByComputer < 3)
        {
            col = VictoryIndicators[roundsWonByComputer + 1].color;
            col.a = 1;
            VictoryIndicators[roundsWonByComputer + 1].color = col;
        }

        foreach (var playerIcon in PlayerDiceIcons)
        {
            playerIcon.sprite = WhiteDiceSprites[1];
        }
        foreach (var opponentIcon in OpponentDiceIcons)
        {
            opponentIcon.sprite = RedDiceSprites[1];
        }
        OpponentHandCombinationName.text = "Opponent combination";
        PlayerHandCombinationName.text = "Player combination";
    }
}
