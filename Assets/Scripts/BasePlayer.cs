using UnityEngine;

public class Hand
{
    // This mask acts as a digital clone of actual dice objects in game
    public DiceFace[] mask;
    public GameObject[] dices;
}

public abstract class BasePlayer : MonoBehaviour
{
    public const int NUMBER_OF_DICES = 5;
    public GameObject dicePrefab;

    private Hand _hand;
    public Hand hand
    {
        get { return _hand; }
        protected set { _hand = value; }
    }

    protected virtual void Initialize()
    {
        hand.mask = new DiceFace[NUMBER_OF_DICES];
        hand.dices = new GameObject[NUMBER_OF_DICES];
    }

    public abstract void ThrowDices();
}
