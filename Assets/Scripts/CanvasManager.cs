using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    [SerializeField] private Image[] _victoryIndicators;
    [SerializeField] private GameObject _announcerNote;
    [SerializeField] private TextMeshProUGUI _announcerText;

    public void UpdateRoundsInfo(int roundsWonByHuman, int roundsWonByComputer, int roundNumber)
    {
        Color col;
        if (roundsWonByHuman > 0 && roundsWonByHuman < 3)
        {
            col = _victoryIndicators[roundsWonByHuman - 1].color;
            col.a = 1;
            _victoryIndicators[roundsWonByHuman - 1].color = col;
        }

        if (roundsWonByComputer > 0 && roundsWonByComputer < 3)
        {
            col = _victoryIndicators[roundsWonByComputer + 1].color;
            col.a = 1;
            _victoryIndicators[roundsWonByComputer + 1].color = col;
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
        StartCoroutine(ShowAnnoucement("Round " + roundNumber));

    }

    public IEnumerator ShowAnnoucement(string msg)
    {
        // TODO Do a tween
        _announcerNote.SetActive(true);
        _announcerText.text = msg;

        yield return new WaitForSeconds(2f);
        _announcerNote.SetActive(false);
    }
}
