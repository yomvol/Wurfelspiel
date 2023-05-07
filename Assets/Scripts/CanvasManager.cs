using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Pause menu")]
    [SerializeField] private GameObject _pauseNote;

    public void UpdateRoundsInfo(int roundsWonByHuman, int roundsWonByComputer)
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
        //StartCoroutine(ShowAnnoucement("Round " + roundNumber));
    }

    public IEnumerator ShowAnnoucement(string msg, int nextRoundNum = -1)
    {
        yield return new WaitUntil(() => !_announcerNote.activeInHierarchy);
        _announcerNote.transform.DOScale(1f, 1f);
        _announcerNote.SetActive(true);
        _announcerText.text = msg;

        yield return new WaitForSeconds(4f);
        if (nextRoundNum != -1)
        {
            _announcerText.text = $"Round {nextRoundNum}";
            yield return new WaitForSeconds(2f);
        }
        
        _announcerNote.transform.DOScale(0f, 1f);
        yield return new WaitUntil(() => _announcerNote.transform.localScale.x < 0.1f);
        _announcerNote.SetActive(false);
    }

    public void ShowPause()
    {
        Time.timeScale = 0.0f;
        _pauseNote.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        _pauseNote.SetActive(false);
#if !DEBUG
                AudioManager.Instance.PlayEffect("UI_Move");
#endif
    }

    public void ReturnToMenu()
    {
        LevelManager.Instance.LoadScene("Menu");
#if !DEBUG
                AudioManager.Instance.PlayEffect("UI_Confirm");
#endif
    }

    public void ExitGame()
    {
    #if UNITY_STANDALONE
        Application.Quit();
    #endif
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
#if !DEBUG
                AudioManager.Instance.PlayEffect("UI_Confirm");
#endif
    }
}
