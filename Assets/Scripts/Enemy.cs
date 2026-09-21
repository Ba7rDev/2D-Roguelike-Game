using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    public int BaseDamage = 3;
    public int ExtraDamagePerLevel = 1;
    public int Defense = 0;
    public int Speed = 10;

    public int CurrentEnergy { get; private set; }

    private int m_CurrentHealth;
    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
        CurrentEnergy = 0;
    }

    public override bool PlayerWantsToEnter()
    {
        int playerStrength = GameManager.Instance.PlayerController.Strength;
        int damageDealt = Mathf.Max(1, playerStrength - Defense);

        m_CurrentHealth -= damageDealt;
        if (m_CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
        return false;
    }

    private bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    private void TurnHappened()
    {
        CurrentEnergy += Speed;

        while (CurrentEnergy >= 10)
        {
            CurrentEnergy -= 10;
            ExecuteEnemyBehavior();
        }
    }

    private void ExecuteEnemyBehavior()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;
        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        {
            if (m_Animator != null)
            {
                m_Animator.SetTrigger("Attack");
            }

            GameManager.Instance.PlayerController.TakeDamage();

            int currentLevel = GameManager.Instance.CurrentLevel;
            int rawDamage = BaseDamage + ((currentLevel - 1) * ExtraDamagePerLevel);

            int playerDefense = GameManager.Instance.PlayerController.Defense;
            int finalDamage = Mathf.Max(1, rawDamage - playerDefense);

            GameManager.Instance.ChangeFood(-finalDamage);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    private bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        return MoveTo(m_Cell + Vector2Int.left);
    }

    private bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        return MoveTo(m_Cell + Vector2Int.down);
    }
}