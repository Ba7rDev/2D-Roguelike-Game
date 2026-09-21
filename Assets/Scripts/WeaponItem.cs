using UnityEngine;

public class WeaponItem : CellObject
{
    public int StrengthBonus = 1;

    public override void PlayerEntered()
    {
        GameManager.Instance.PlayerController.Strength += StrengthBonus;
        Destroy(gameObject);
    }
}