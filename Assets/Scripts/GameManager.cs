using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }

    public int CurrentLevel => m_CurrentLevel;

    private int m_FoodAmount = 100;
    private int m_CurrentLevel = 1;

  
    private Label m_FoodLabel;
    private Label m_SpeedLabel;
    private Label m_EnergyLabel;

    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

       
        var root = UIDoc.rootVisualElement;
        m_FoodLabel = root.Q<Label>("FoodLabel");
        m_SpeedLabel = root.Q<Label>("SpeedLabel");
        m_EnergyLabel = root.Q<Label>("EnergyLabel");

        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

        StartNewGame();
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 100;

        BoardManager.Clean();
        BoardManager.Init(m_CurrentLevel);

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        UpdateStatsUI();
    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        BoardManager.Clean();
        BoardManager.Init(m_CurrentLevel);
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        UpdateStatsUI();
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
        UpdateStatsUI();
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " levels\n\nPress Enter to Restart";
        }

        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (m_FoodLabel != null)
            m_FoodLabel.text = "Food: " + m_FoodAmount;

        if (PlayerController != null)
        {
            if (m_SpeedLabel != null)
                m_SpeedLabel.text = "Speed: " + PlayerController.Speed;

            if (m_EnergyLabel != null)
                m_EnergyLabel.text = "Energy: " + PlayerController.CurrentEnergy;
        }
    }
}