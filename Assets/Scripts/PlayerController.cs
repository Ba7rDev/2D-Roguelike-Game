using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public int Strength = 1;
    public int Defense = 0;
    public float FoodMultiplier = 1.0f;
    public int Speed = 10;

    public int CurrentEnergy { get; private set; }

    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;

    private Animator m_Animator;

    public Vector2Int Cell => m_CellPosition;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Init()
    {
        m_IsGameOver = false;
        Strength = 1;
        Defense = 0;
        FoodMultiplier = 1.0f;
        Speed = 10;
        CurrentEnergy = Speed;
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell);
    }

    public void Attack()
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Attack");
        }
    }

    public void TakeDamage()
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Hurt");
        }
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

      
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CurrentEnergy += Speed;
            GameManager.Instance.TurnManager.Tick();
            return;
        }

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }

        if (hasMoved)
        {
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);
            if (cellData != null && cellData.Passable)
            {
                
                if (CurrentEnergy < 10)
                {
                    CurrentEnergy += Speed;
                }

                CurrentEnergy -= 10;

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget);
                }
                else if (cellData.ContainedObject.PlayerWantsToEnter())
                {
                    MoveTo(newCellTarget);
                    cellData.ContainedObject.PlayerEntered();
                }
                else
                {
                    Attack();
                }

                GameManager.Instance.TurnManager.Tick();
            }
        }
    }

    private void MoveTo(Vector2Int newCellTarget)
    {
        m_CellPosition = newCellTarget;
        transform.position = m_Board.CellToWorld(m_CellPosition);
    }
}