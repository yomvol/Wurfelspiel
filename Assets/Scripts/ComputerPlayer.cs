using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ComputerPlayer : BasePlayer
{
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
                Hand.Mask[i] = _rolls[i].RollResult;
                _rolls[i].ResultReadyEvent -= ResultReadyAllDicesEventHandler;
            }
            _resultsReceivedCounter = 0;
            EvaluateHand();
            if (GameManager.Instance.State == GameState.OpponentTurn)
            {
                UpdateUI((DiceFace[])Hand.Mask.Clone());
            }

            GameManager.Instance.DiceZoomInCamera.Priority = 11;
            StartCoroutine(GameManager.Instance.ChangeState(GameState.Reroll, false, 4f));
        }
    }

    protected override void UpdateUI(DiceFace[] arr)
    {
        Array.Sort(arr);
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            switch (arr[i])
            {
                case DiceFace.One:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[0];
                    break;
                case DiceFace.Two:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[1];
                    break;
                case DiceFace.Three:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[2];
                    break;
                case DiceFace.Four:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[3];
                    break;
                case DiceFace.Five:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[4];
                    break;
                case DiceFace.Six:
                    CanvasManager.Instance.OpponentDiceIcons[i].sprite = CanvasManager.Instance.RedDiceSprites[5];
                    break;
            }
        }
        CanvasManager.Instance.OpponentHandCombinationName.text = Hand.HandPower.Item1.ToString();
    }

    public override void SelectAndReroll()
    {
        if (_dicesToReroll.Count > 0)
        {
            for (int i = 0; i < _dicesToReroll.Count; i++)
            {
                _dicesToReroll[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
                _dicesToReroll[i].ResultReadyEvent += ResultReadyRerollDicesEventHandler;
                _dicesToReroll[i].ThrowDice();
            }
        }
        else
        {
            // There are no dices to reroll, so hand and UI are unchanged
            //EvaluateHand();
            //UpdateUI((DiceFace[])Hand.Mask.Clone());
            Debug.Log($"{gameObject.name} got {Hand.HandPower.Item1} of {Hand.HandPower.Item2}");
        }

        StartCoroutine(GameManager.Instance.ChangeState(GameState.HandsComparing, 6f));
    }

    public override void KeepDicesNearby()
    {
        // Are hard coded values a temporary solution?

        Hand.Dices[0].transform.position = new Vector3(-2.056f, 4.176f, 1.736f);
        Hand.Dices[1].transform.position = new Vector3(-1.806f, 4.176f, 1.601f);
        Hand.Dices[2].transform.position = new Vector3(-1.929f, 4.176f, 1.372f);
        Hand.Dices[3].transform.position = new Vector3(-2.179f, 4.176f, 1.264f);
        Hand.Dices[4].transform.position = new Vector3(-2.235f, 4.176f, 1.509f);
    }

    protected override void EvaluateHand()
    {
        _dicesToReroll.Clear();
        DiceFace[] maskClone = (DiceFace[])Hand.Mask.Clone();
        Array.Sort(maskClone);

        // Are there 3 or more dice of the same value?
        var groups = maskClone.GroupBy(v => v);
        DictionaryEntry dominantGroup = new DictionaryEntry();
        dominantGroup.Value = 0;
        foreach (var group in groups)
        {
            if (group.Count() >= 3)
            {
                dominantGroup.Key = group.Key;
                dominantGroup.Value = group.Count();
            }
        }

        int numberOfDicesOfTheSameValue = (int)dominantGroup.Value;
        if (numberOfDicesOfTheSameValue >= 3)
        {
            if (numberOfDicesOfTheSameValue == 5)
            {
                Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FiveOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                return;
            }
            else if (numberOfDicesOfTheSameValue == 4)
            {
                Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FourOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                foreach (var group in groups)
                {
                    if (group.Count() == 1)
                    {
                        int index = Array.IndexOf(Hand.Mask, group.Key);
                        _dicesToReroll.Add(_rolls[index]);
                        break;
                    }
                }
                return;
            }
            else // 3 same dices. Is there a full house or no?
            {
                if (groups.Count() == 2) // in case of full house there must be only 2 groups
                {
                    foreach (var group in groups)
                    {
                        if (group.Key != (DiceFace)dominantGroup.Key)
                        {
                            Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FullHouse, (DiceFace)dominantGroup.Key, group.Key);
                            return;
                        }
                    }
                }
                else // we have enough info to conclude that the hand is Three of a kind
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.ThreeOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                    foreach (var diceFace in Hand.Mask)
                    {
                        if (diceFace != (DiceFace)dominantGroup.Key)
                        {
                            int index = Array.IndexOf(Hand.Mask, diceFace);
                            _dicesToReroll.Add(_rolls[index]);
                        }
                    }
                    return;
                }
            }
        }
        else
        {
            // Is there at least one pair?
            if (groups.Count() == 5) // No pairs
            {
                DiceFace[] fiveHighStraight = { DiceFace.One, DiceFace.Two, DiceFace.Three, DiceFace.Four, DiceFace.Five };
                DiceFace[] sixHighStraight = { DiceFace.Two, DiceFace.Three, DiceFace.Four, DiceFace.Five, DiceFace.Six };

                if (maskClone.SequenceEqual(fiveHighStraight))
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.FiveHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else if (maskClone.SequenceEqual(sixHighStraight))
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.SixHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else // high card
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.HighCard, DiceFace.One, DiceFace.One);
                    _dicesToReroll.AddRange(_rolls);
                    return;
                }
            }
            else // at least one
            {
                if (groups.Count() == 3) // two pairs
                {
                    ArrayList kickers = new ArrayList();
                    foreach (var group in groups)
                    {
                        if (group.Count() == 2)
                        {
                            kickers.Add(group.Key);
                        }
                        else if (group.Count() == 1)
                        {
                            int index = Array.IndexOf(Hand.Mask, group.Key);
                            _dicesToReroll.Add(_rolls[index]);
                        }
                    }
                    kickers.Sort();
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.TwoPairs, (DiceFace)kickers[1], (DiceFace)kickers[0]);
                    return;
                }
                else if (groups.Count() == 4) // one pair
                {
                    foreach (var group in groups)
                    {
                        if (group.Count() == 2)
                        {
                            Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.Pair, group.Key, DiceFace.One);
                            foreach (var diceFace in Hand.Mask)
                            {
                                if (diceFace == group.Key)
                                    continue;
                                int index = Array.IndexOf(Hand.Mask, diceFace);
                                _dicesToReroll.Add(_rolls[index]);
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}
