using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string WinTitle = "You Won!";
    private const string GameOverTitle = "Game Over";

    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private Button nextWaveButton;

    private GameObject _endStatePopup;
    private Image _endStateOverlayImage;
    private Image _endStatePanelImage;
    private Image _endStateButtonImage;
    private Text _endStateTitleText;
    private Text _endStateButtonText;
    private bool _isEndStateShown;
    private Spawner _spawner;

    private void Awake()
    {
        if (nextWaveButton == null)
        {
            nextWaveButton = GameObject.Find("NextWave")?.GetComponent<Button>();
        }

        _spawner = FindObjectOfType<Spawner>();
        ConfigureNextWaveTooltip();
        EnsureEndStatePopup();
    }

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        Spawner.OnWaveStateChanged += UpdateNextWaveButtonState;
        Spawner.OnAllWavesCompleted += ShowWinPopup;
        GameManager.OnGameOver += ShowGameOverPopup;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        Spawner.OnWaveStateChanged -= UpdateNextWaveButtonState;
        Spawner.OnAllWavesCompleted -= ShowWinPopup;
        GameManager.OnGameOver -= ShowGameOverPopup;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
    }

    private void OnDestroy()
    {
        if (_isEndStateShown)
        {
            Time.timeScale = 1f;
        }
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }

    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"Lives: {currentLives}";
    }

    private void UpdateResourcesText(int currentResources)
    {
        resourcesText.text = $"Resources: {currentResources}";
    }

    private void UpdateNextWaveButtonState(bool isWaveActive)
    {
        if (nextWaveButton != null)
        {
            nextWaveButton.interactable = !isWaveActive && !_isEndStateShown;
        }
    }

    private void ShowWinPopup()
    {
        ShowEndStatePopup(WinTitle, false);
    }

    private void ShowGameOverPopup()
    {
        ShowEndStatePopup(GameOverTitle, true);
    }

    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
    }

    public void HideTowerPanel()
    {
        SoundManager.Instance?.PlayButtonClick();
        towerPanel.SetActive(false);
    }

    private void LoadMainMenu()
    {
        SoundManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void ShowEndStatePopup(string title, bool isGameOver)
    {
        if (_isEndStateShown)
        {
            return;
        }

        _isEndStateShown = true;
        UpdateNextWaveButtonState(false);

        if (towerPanel != null)
        {
            towerPanel.SetActive(false);
        }

        if (_endStatePopup == null)
        {
            EnsureEndStatePopup();
        }

        if (_endStateTitleText != null)
        {
            _endStateTitleText.text = title;
        }

        ApplyEndStateTheme(isGameOver);

        if (_endStatePopup != null)
        {
            _endStatePopup.SetActive(true);
            _endStatePopup.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    private void EnsureEndStatePopup()
    {
        if (_endStatePopup != null)
        {
            _endStatePopup.SetActive(false);
            return;
        }

        Transform popupParent = transform;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            popupParent = canvas.transform;
        }

        GameObject overlay = new GameObject("EndStatePopup", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(popupParent, false);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        _endStateOverlayImage = overlay.GetComponent<Image>();
        _endStateOverlayImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(460f, 240f);
        panelRect.anchoredPosition = Vector2.zero;

        _endStatePanelImage = panel.GetComponent<Image>();
        _endStatePanelImage.color = new Color(0.94f, 0.96f, 1f, 1f);

        GameObject title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(panel.transform, false);

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.58f);
        titleRect.anchorMax = new Vector2(0.9f, 0.88f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        _endStateTitleText = title.GetComponent<Text>();
        _endStateTitleText.text = WinTitle;
        _endStateTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _endStateTitleText.fontSize = 34;
        _endStateTitleText.alignment = TextAnchor.MiddleCenter;
        _endStateTitleText.color = new Color(0.1f, 0.15f, 0.23f, 1f);

        GameObject buttonObject = new GameObject("MainMenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.24f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.24f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(210f, 56f);

        _endStateButtonImage = buttonObject.GetComponent<Image>();
        _endStateButtonImage.color = new Color(0.18f, 0.45f, 0.85f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(LoadMainMenu);

        GameObject buttonLabel = new GameObject("Label", typeof(RectTransform), typeof(Text));
        buttonLabel.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = buttonLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        _endStateButtonText = buttonLabel.GetComponent<Text>();
        _endStateButtonText.text = "Main Menu";
        _endStateButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _endStateButtonText.fontSize = 24;
        _endStateButtonText.alignment = TextAnchor.MiddleCenter;
        _endStateButtonText.color = Color.white;

        overlay.SetActive(false);
        _endStatePopup = overlay;
    }

    private void ApplyEndStateTheme(bool isGameOver)
    {
        if (_endStateOverlayImage == null || _endStatePanelImage == null || _endStateButtonImage == null || _endStateTitleText == null || _endStateButtonText == null)
        {
            return;
        }

        if (isGameOver)
        {
            _endStateOverlayImage.color = new Color(0.16f, 0f, 0f, 0.82f);
            _endStatePanelImage.color = new Color(0.32f, 0.1f, 0.1f, 1f);
            _endStateButtonImage.color = new Color(0.66f, 0.16f, 0.16f, 1f);
            _endStateTitleText.color = new Color(1f, 0.85f, 0.85f, 1f);
            _endStateButtonText.color = new Color(1f, 0.95f, 0.95f, 1f);
            return;
        }

        _endStateOverlayImage.color = new Color(0f, 0.16f, 0f, 0.82f);
        _endStatePanelImage.color = new Color(0.1f, 0.32f, 0.1f, 1f);
        _endStateButtonImage.color = new Color(0.18f, 0.58f, 0.18f, 1f);
        _endStateTitleText.color = new Color(0.88f, 1f, 0.88f, 1f);
        _endStateButtonText.color = new Color(0.95f, 1f, 0.95f, 1f);
    }

    private void ConfigureNextWaveTooltip()
    {
        if (nextWaveButton == null)
        {
            return;
        }

        ButtonHoverTooltip tooltip = nextWaveButton.GetComponent<ButtonHoverTooltip>();
        if (tooltip == null)
        {
            tooltip = nextWaveButton.gameObject.AddComponent<ButtonHoverTooltip>();
        }

        tooltip.Configure(BuildNextWaveTooltipText);
    }

    private string BuildNextWaveTooltipText()
    {
        if (_spawner == null)
        {
            _spawner = FindObjectOfType<Spawner>();
        }

        if (_spawner == null || !_spawner.TryGetNextWavePreview(out Spawner.WavePreview preview))
        {
            return "No more waves";
        }

        return $"Enemy: {preview.EnemyType}\nCount: {preview.EnemyCount}\nHealth: {FormatFloat(preview.EnemyHealth)}";
    }

    private static string FormatFloat(float value)
    {
        return Mathf.Approximately(value % 1f, 0f) ? value.ToString("0") : value.ToString("0.##");
    }
}
