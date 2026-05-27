using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private Button nextWaveButton;

    private GameObject _winPopup;
    private bool _hasWon;

    private void Awake()
    {
        if (nextWaveButton == null)
        {
            nextWaveButton = GameObject.Find("NextWave")?.GetComponent<Button>();
        }

        EnsureWinPopup();
    }

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        Spawner.OnWaveStateChanged += UpdateNextWaveButtonState;
        Spawner.OnAllWavesCompleted += ShowWinPopup;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        Spawner.OnWaveStateChanged -= UpdateNextWaveButtonState;
        Spawner.OnAllWavesCompleted -= ShowWinPopup;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
    }

    private void OnDestroy()
    {
        if (_hasWon)
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
            nextWaveButton.interactable = !isWaveActive && !_hasWon;
        }
    }

    private void ShowWinPopup()
    {
        _hasWon = true;
        UpdateNextWaveButtonState(false);

        if (_winPopup == null)
        {
            EnsureWinPopup();
        }

        if (_winPopup != null)
        {
            _winPopup.SetActive(true);
            _winPopup.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
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

    private void EnsureWinPopup()
    {
        if (_winPopup != null)
        {
            _winPopup.SetActive(false);
            return;
        }

        Transform popupParent = transform;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            popupParent = canvas.transform;
        }

        GameObject overlay = new GameObject("WinPopup", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(popupParent, false);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(460f, 240f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.94f, 0.96f, 1f, 1f);

        GameObject title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(panel.transform, false);

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.58f);
        titleRect.anchorMax = new Vector2(0.9f, 0.88f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Text titleText = title.GetComponent<Text>();
        titleText.text = "You Won!";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 34;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.1f, 0.15f, 0.23f, 1f);

        GameObject buttonObject = new GameObject("MainMenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.24f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.24f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(210f, 56f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.45f, 0.85f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(LoadMainMenu);

        GameObject buttonLabel = new GameObject("Label", typeof(RectTransform), typeof(Text));
        buttonLabel.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = buttonLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text buttonText = buttonLabel.GetComponent<Text>();
        buttonText.text = "Main Menu";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;

        overlay.SetActive(false);
        _winPopup = overlay;
    }
}
