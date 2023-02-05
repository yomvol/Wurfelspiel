using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hand
{
    // This mask acts as a digital clone of actual dice objects in game
    public DiceFace[] Mask;
    public GameObject[] Dices;
    // This structure lets us make judgements which hand is stronger. Second and third paramaters are senior and junior kickers.
    // Let`s say that both players have full house. The first one has 222**55, the second one has 222**66, the second one wins.
    public Tuple<HandCombination, DiceFace, DiceFace> HandPower;

    public Hand()
    {
        Mask = new DiceFace[BasePlayer.NUMBER_OF_DICES];
        Dices = new GameObject[BasePlayer.NUMBER_OF_DICES];
        HandPower = new Tuple<HandCombination, DiceFace, DiceFace>(HandCombination.HighCard, DiceFace.One, DiceFace.One);
    }
}

public abstract class BasePlayer : MonoBehaviour
{
    public const int NUMBER_OF_DICES = 5;
    public GameObject DicePrefab;
    public Hand Hand { get; protected set; }

    protected DiceRoll[] _rolls;
    protected List<DiceRoll> _dicesToReroll;
    protected int _resultsReceivedCounter;

    protected virtual void Initialize()
    {
        Hand = new Hand();
        _rolls = new DiceRoll[NUMBER_OF_DICES];
        _dicesToReroll = new List<DiceRoll>();
        _resultsReceivedCounter = 0;

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            Hand.Mask[i] = DiceFace.Two;
            Hand.Dices[i] = Instantiate(DicePrefab, transform.position + new Vector3(0, 0, i * 0.5f), Quaternion.identity, transform);
            _rolls[i] = Hand.Dices[i].GetComponent<DiceRoll>();
        }
    }

    protected abstract void ResultReadyAllDicesEventHandler(object sender, EventArgs e);

    protected void ResultReadyRerollDicesEventHandler(object sender, EventArgs e)
    {
        _resultsReceivedCounter++;

        // count = 0 case is handled externally
        if (_resultsReceivedCounter >= _dicesToReroll.Count)
        {
            for (int i = 0; i < _dicesToReroll.Count; i++)
            {
                int index = Array.IndexOf(_rolls, _dicesToReroll[i]);
                Hand.Mask[index] = _dicesToReroll[i].RollResult;
                _dicesToReroll[i].ResultReadyEvent -= ResultReadyRerollDicesEventHandler;
            }

            _resultsReceivedCounter = 0;
            EvaluateHand();
            if (GameManager.Instance.State == GameState.Reroll)
            {
                UpdateUI((DiceFace[])Hand.Mask.Clone());
            }
            Debug.Log($"{gameObject.name} got {Hand.HandPower.Item1} of {Hand.HandPower.Item2}");
        }
    }

    public virtual void ThrowDices()
    {
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            // TODO Do a tween?
            Hand.Dices[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
            _rolls[i].ResultReadyEvent += ResultReadyAllDicesEventHandler;
            _rolls[i].ThrowDice();
        }
    }

    public abstract void KeepDicesNearby();

    public abstract void SelectAndReroll();

    protected abstract void UpdateUI(DiceFace[] arr);

    protected virtual void EvaluateHand()
    {
        _dicesToReroll.Clear();
        Array.Sort(Hand.Mask);

        // Are there 3 or more dice of the same value?
        var groups = Hand.Mask.GroupBy(v => v);
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

                if (Hand.Mask.SequenceEqual(fiveHighStraight))
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.FiveHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else if (Hand.Mask.SequenceEqual(sixHighStraight))
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.SixHighStraight, DiceFace.One, DiceFace.One);
                    return;
                }
                else // high card
                {
                    Hand.HandPower = new Tuple<HandCombination, DiceFace, DiceFace>
                        (HandCombination.HighCard, DiceFace.One, DiceFace.One);
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
                            return;
                        }
                    }
                }
            }
        }
    }
}
