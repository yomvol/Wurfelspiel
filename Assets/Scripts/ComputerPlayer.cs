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

    public override void SelectAndReroll()
    {
        Debug.Log("Here");
        if (_dicesToReroll.Count > 0)
        {
            Debug.Log("There");
            GameManager.Instance.diceZoomInCamera.Priority = 11;
            for (int i = 0; i < _dicesToReroll.Count; i++)
            {
                _dicesToReroll[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
                _dicesToReroll[i].ThrowDice();
            }
           
            // Evaluate hand again
        }

        StartCoroutine(GameManager.Instance.ChangeState(GameState.Win, 3f));
    }

    protected override void EvaluateHand()
    {
        Array.Sort(hand.mask);

        // Are there 3 or more dice of the same value?
        var groups = hand.mask.GroupBy(v => v);
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
                hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FiveOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                return;
            }
            else if (numberOfDicesOfTheSameValue == 4)
            {
                hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FourOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                foreach (var group in groups)
                {
                    if (group.Count() == 1)
                    {
                        int index = Array.IndexOf(hand.mask, group.Key);
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
                            hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.FullHouse, (DiceFace)dominantGroup.Key, group.Key);
                            return;
                        }
                    }
                }
                else // we have enough info to conclude that the hand is Three of a kind
                {
                    hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                    (HandCombination.ThreeOfKind, (DiceFace)dominantGroup.Key, DiceFace.One);
                    foreach (var diceFace in hand.mask)
                    {
                        if (diceFace != (DiceFace)dominantGroup.Key)
                        {
                            int index = Array.IndexOf(hand.mask, diceFace);
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

                if (hand.mask.SequenceEqual(fiveHighStraight))
                {
                    hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.FiveHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else if (hand.mask.SequenceEqual(sixHighStraight))
                {
                    hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.SixHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else // high card
                {
                    hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
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
                            int index = Array.IndexOf(hand.mask, group.Key);
                            _dicesToReroll.Add(_rolls[index]);
                        }
                    }
                    kickers.Sort();
                    hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.TwoPairs, (DiceFace)kickers[1], (DiceFace)kickers[0]);
                    return;
                }
                else if (groups.Count() == 4) // one pair
                {
                    foreach (var group in groups)
                    {
                        if (group.Count() == 2)
                        {
                            hand.handPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.Pair, group.Key, DiceFace.One);
                            foreach (var diceFace in hand.mask)
                            {
                                if (diceFace == group.Key)
                                    continue;
                                int index = Array.IndexOf(hand.mask, diceFace);
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
