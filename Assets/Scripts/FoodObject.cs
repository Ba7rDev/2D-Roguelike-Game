using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted = 10;

    public override void PlayerEntered()
    {
        Destroy(gameObject);

        float multiplier = GameManager.Instance.PlayerController.FoodMultiplier;
        int finalAmount = Mathf.RoundToInt(AmountGranted * multiplier);

        GameManager.Instance.ChangeFood(finalAmount);
    }
}