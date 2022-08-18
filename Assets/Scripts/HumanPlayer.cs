using UnityEngine;

public class HumanPlayer : BasePlayer
{
    // these components handle logic interacting with a dice
    private DiceRoll[] rolls;

    protected override void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            hand.mask[i] = DiceFace.Two;
            hand.dices[i] = Instantiate(dicePrefab, transform.position + new Vector3(0, 0, i * 2), Quaternion.identity, transform);
            hand.dices[i].SetActive(false);
            rolls[i] = hand.dices[i].GetComponent<DiceRoll>();
        }
    }

    private void Start()
    {
        Initialize();
    }

    public override void ThrowDices()
    {
        for (int i = 0; i < NUMBER_OF_DICES; i++)
        {
            rolls[i].
        }
    }
}
