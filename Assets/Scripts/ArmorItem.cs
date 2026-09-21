using UnityEngine;

public class ArmorItem : CellObject
{
    public int DefenseBonus = 1;

    public override void PlayerEntered()
    {
        GameManager.Instance.PlayerController.Defense += DefenseBonus;
        Destroy(gameObject);
    }
}