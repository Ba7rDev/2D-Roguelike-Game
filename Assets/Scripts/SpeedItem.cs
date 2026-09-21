using UnityEngine;

public class SpeedItem : CellObject
{
    public int SpeedBonus = 5;

    public override void PlayerEntered()
    {
        GameManager.Instance.PlayerController.Speed += SpeedBonus;
        GameManager.Instance.UpdateStatsUI();
        Destroy(gameObject);
    }
}