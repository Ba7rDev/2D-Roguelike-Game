using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

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

  
    private VisualElement m_MainMenuPanel;
    private VisualElement m_PauseMenuPanel;
    private VisualElement m_GameOverPanel;


    private Label m_FoodLabel;
    private Label m_SpeedLabel;
    private Label m_EnergyLabel;
    private Label m_GameOverMessage;

  
    public bool IsInMainMenu { get; private set; } = true;
    public bool IsPaused { get; private set; } = false;

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

     
        m_MainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        m_PauseMenuPanel = root.Q<VisualElement>("PauseMenuPanel");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");

      
        m_FoodLabel = root.Q<Label>("FoodLabel");
        m_SpeedLabel = root.Q<Label>("SpeedLabel");
        m_EnergyLabel = root.Q<Label>("EnergyLabel");
        m_GameOverMessage = m_GameOverPanel?.Q<Label>("GameOverMessage");

    
        if (m_MainMenuPanel != null)
        {
            m_MainMenuPanel.Q<Button>("StartGameButton")?.RegisterCallback<ClickEvent>(evt => StartNewGame());
            m_MainMenuPanel.Q<Button>("QuitButton")?.RegisterCallback<ClickEvent>(evt => QuitGame());
        }

       
        if (m_PauseMenuPanel != null)
        {
            m_PauseMenuPanel.Q<Button>("ResumeButton")?.RegisterCallback<ClickEvent>(evt => TogglePause());
            m_PauseMenuPanel.Q<Button>("MainMenuButton")?.RegisterCallback<ClickEvent>(evt => ShowMainMenu());
            m_PauseMenuPanel.Q<Button>("QuitButton")?.RegisterCallback<ClickEvent>(evt => QuitGame());
        }

        ShowMainMenu();
    }

    private void Update()
    {
        if (!IsInMainMenu && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void ShowMainMenu()
    {
        IsInMainMenu = true;
        IsPaused = false;
        Time.timeScale = 1f;

        BoardManager.Clean();

        if (m_MainMenuPanel != null) m_MainMenuPanel.style.visibility = Visibility.Visible;
        if (m_PauseMenuPanel != null) m_PauseMenuPanel.style.visibility = Visibility.Hidden;
        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
    }

    public void StartNewGame()
    {
        IsInMainMenu = false;
        IsPaused = false;
        Time.timeScale = 1f;

        if (m_MainMenuPanel != null) m_MainMenuPanel.style.visibility = Visibility.Hidden;
        if (m_PauseMenuPanel != null) m_PauseMenuPanel.style.visibility = Visibility.Hidden;
        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 100;

        BoardManager.Clean();
        BoardManager.Init(m_CurrentLevel);

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        UpdateStatsUI();
    }

    public void TogglePause()
    {
        if (IsInMainMenu) return;

        IsPaused = !IsPaused;

        if (m_PauseMenuPanel != null)
        {
            m_PauseMenuPanel.style.visibility = IsPaused ? Visibility.Visible : Visibility.Hidden;
        }

        Time.timeScale = IsPaused ? 0f : 1f;
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
            if (m_GameOverPanel != null)
            {
                m_GameOverPanel.style.visibility = Visibility.Visible;
                m_GameOverMessage.text = "Game Over!\n\nSurvived " + m_CurrentLevel + " levels\n\nPress Enter to Restart";
            }
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

    public void QuitGame()
    {
        Debug.Log("Quit button triggered.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}