using UnityEngine;

public class LunchboxItem : CellObject
{
    public float MultiplierIncrease = 1.0f;

    public override void PlayerEntered()
    {
        GameManager.Instance.PlayerController.FoodMultiplier += MultiplierIncrease;
        Destroy(gameObject);
    }
}