using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hand
{
    // This mask acts as a digital clone of actual dice objects in game
    public DiceFace[] mask;
    public GameObject[] dices;
    // This structure lets us make judgements which hand is stronger. Second and third paramaters are senior and junior kickers.
    // Let`s say that both players have full house. The first one has 222**55, the second one has 222**66, the second one wins.
    public Tuple<HandCombination, DiceFace, DiceFace> handPower;

    public Hand()
    {
        mask = new DiceFace[BasePlayer.NUMBER_OF_DICES];
        dices = new GameObject[BasePlayer.NUMBER_OF_DICES];
        handPower = new Tuple<HandCombination, DiceFace, DiceFace>(HandCombination.HighCard, DiceFace.One, DiceFace.One);
    }
}

public abstract class BasePlayer : MonoBehaviour
{
    public const int NUMBER_OF_DICES = 5;
    public GameObject dicePrefab;

    protected DiceRoll[] _rolls;
    protected List<DiceRoll> _dicesToReroll;
    protected int _resultsReceivedCounter;

    public Hand hand { get; protected set; }

    protected virtual void Initialize()
    {
        hand = new Hand();
        _rolls = new DiceRoll[NUMBER_OF_DICES];
        _dicesToReroll = new List<DiceRoll>();
        _resultsReceivedCounter = 0;

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            hand.mask[i] = DiceFace.Two;
            hand.dices[i] = Instantiate(dicePrefab, transform.position + new Vector3(0, 0, i * 0.5f), Quaternion.identity, transform);
            _rolls[i] = hand.dices[i].GetComponent<DiceRoll>();
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
                hand.mask[index] = _dicesToReroll[i].rollResult;
                _dicesToReroll[i].resultReadyEvent -= ResultReadyRerollDicesEventHandler;
            }

            _resultsReceivedCounter = 0;
            EvaluateHand();
            Debug.Log($"{gameObject.name} got {hand.handPower.Item1} of {hand.handPower.Item2}");
        }
    }

    public virtual void ThrowDices()
    {
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            hand.dices[i].transform.position = transform.position + new Vector3(0, 0, i * 0.5f);
            _rolls[i].resultReadyEvent += ResultReadyAllDicesEventHandler;
            _rolls[i].ThrowDice();
        }
    }

    public void KeepDicesNearby(bool isCalledByHuman)
    {
        if (isCalledByHuman)
        {
            hand.dices[0].transform.position = new Vector3(1.873f, 4.176f, -1.383f);
            hand.dices[1].transform.position = new Vector3(2.034f, 4.176f, -1.084f);
            hand.dices[2].transform.position = new Vector3(1.916f, 4.176f, -0.813f);
            hand.dices[3].transform.position = new Vector3(2.286f, 4.176f, -0.964f);
            hand.dices[4].transform.position = new Vector3(2.161f, 4.176f, -1.456f);
        }
        else
        {
            hand.dices[0].transform.position = new Vector3(-2.056f, 4.176f, 1.736f);
            hand.dices[1].transform.position = new Vector3(-1.806f, 4.176f, 1.601f);
            hand.dices[2].transform.position = new Vector3(-1.929f, 4.176f, 1.372f);
            hand.dices[3].transform.position = new Vector3(-2.179f, 4.176f, 1.264f);
            hand.dices[4].transform.position = new Vector3(-2.235f, 4.176f, 1.509f);
        }
    }

    public abstract void SelectAndReroll();

    protected virtual void EvaluateHand()
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
                            return;
                        }
                    }
                }
            }
        }
    }
}
