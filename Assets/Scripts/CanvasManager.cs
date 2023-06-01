using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("Hand info")]
    public TextMeshProUGUI OpponentHandCombinationName;
    public TextMeshProUGUI PlayerHandCombinationName;
    public Image[] OpponentDiceIcons;
    public Image[] PlayerDiceIcons;
    public Sprite[] WhiteDiceSprites;
    public Sprite[] RedDiceSprites;
    public TextMeshProUGUI PlayerEndTurnPrompt;

    [SerializeField] TextMeshProUGUI _playerThrowPrompt;
    [SerializeField] Sprite[] _mouseIcons;
    [SerializeField] Image _mouseImage;
    [SerializeField] Texture2D[] _cursors;

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
        Cursor.visible = false;
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

    public void OnThrowStarted(InputAction.CallbackContext ctx)
    {
        _playerThrowPrompt.text = "Cancel throw";
        _mouseImage.sprite = _mouseIcons[1];
    }

    public void OnThrowAborted(InputAction.CallbackContext ctx)
    {
        _playerThrowPrompt.text = "Throw all (hold)";
        _mouseImage.sprite = _mouseIcons[0];
    }

    public void OnHoverIn()
    {
        Cursor.SetCursor(_cursors[1], Vector2.zero, CursorMode.Auto);
    }

    public void OnHoverOut()
    {
        Cursor.SetCursor(_cursors[0], Vector2.zero, CursorMode.Auto);
    }
}
