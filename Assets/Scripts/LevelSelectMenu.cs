using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    private struct LevelOption
    {
        public string sceneName;
        public string displayName;
        public string previewResourcePath;

        public LevelOption(string sceneName, string displayName, string previewResourcePath)
        {
            this.sceneName = sceneName;
            this.displayName = displayName;
            this.previewResourcePath = previewResourcePath;
        }
    }

    private readonly LevelOption[] _levels =
    {
        new LevelOption("Level1", "Level 1", "LevelPreviews/Level1Preview"),
        new LevelOption("Level2", "Level 2", "LevelPreviews/Level2Preview"),
        new LevelOption("Level3", "Level 3", "LevelPreviews/Level3Preview")
    };

    private TMP_FontAsset _fontAsset;
    private Image _previewImage;
    private TMP_Text _levelNameText;
    private int _currentIndex;

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("LevelSelectMenu requires a Canvas on the same GameObject.");
            enabled = false;
            return;
        }

        _fontAsset = GetReferenceFont();
        PrepareExistingCanvas(canvas.transform);
        BuildSelectionUi(canvas.transform);
        UpdateSelection();
    }

    private TMP_FontAsset GetReferenceFont()
    {
        TMP_Text existingText = GetComponentInChildren<TMP_Text>(true);
        if (existingText != null && existingText.font != null)
        {
            return existingText.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private void PrepareExistingCanvas(Transform canvasTransform)
    {
        TMP_Text titleText = null;
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        Transform titleTransform = canvasTransform.Find("GameTitle");
        if (titleTransform != null)
        {
            titleText = titleTransform.GetComponent<TMP_Text>();
        }

        if (titleText != null)
        {
            titleText.text = "Choose\nLevel";
            titleText.fontSize = 46;
            titleText.alignment = TextAlignmentOptions.Center;
        }
    }

    private void BuildSelectionUi(Transform canvasTransform)
    {
        RectTransform root = CreateRect("LevelSelectRoot", canvasTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.SetAsLastSibling();

        RectTransform previewFrame = CreateRect("PreviewFrame", root, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(700f, 420f), Vector2.zero);
        Image frameImage = previewFrame.gameObject.AddComponent<Image>();
        frameImage.color = new Color(0.09f, 0.15f, 0.2f, 0.9f);
        Outline frameOutline = previewFrame.gameObject.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.89f, 0.78f, 0.56f, 0.9f);
        frameOutline.effectDistance = new Vector2(8f, -8f);

        _levelNameText = CreateText("LevelName", previewFrame, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 60f), new Vector2(0f, -36f), 34, TextAlignmentOptions.Center);

        RectTransform previewHolder = CreateRect("PreviewHolder", previewFrame, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(620f, 280f), Vector2.zero);
        Image holderImage = previewHolder.gameObject.AddComponent<Image>();
        holderImage.color = new Color(0.03f, 0.05f, 0.07f, 0.95f);

        RectTransform previewRect = CreateRect("PreviewImage", previewHolder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        previewRect.offsetMin = new Vector2(12f, 12f);
        previewRect.offsetMax = new Vector2(-12f, -12f);
        _previewImage = previewRect.gameObject.AddComponent<Image>();
        _previewImage.preserveAspect = true;

        Button leftButton = CreateButton("PreviousLevelButton", root, new Vector2(0.5f, 0.6f), new Vector2(100f, 100f), new Vector2(-420f, 0f), "<", 46);
        leftButton.onClick.AddListener(ShowPreviousLevel);

        Button rightButton = CreateButton("NextLevelButton", root, new Vector2(0.5f, 0.6f), new Vector2(100f, 100f), new Vector2(420f, 0f), ">", 46);
        rightButton.onClick.AddListener(ShowNextLevel);

        Button startButton = CreateButton("StartLevelButton", root, new Vector2(0.5f, 0.27f), new Vector2(240f, 84f), Vector2.zero, "Start", 34);
        startButton.onClick.AddListener(StartSelectedLevel);
    }

    private void ShowPreviousLevel()
    {
        _currentIndex = (_currentIndex - 1 + _levels.Length) % _levels.Length;
        UpdateSelection();
    }

    private void ShowNextLevel()
    {
        _currentIndex = (_currentIndex + 1) % _levels.Length;
        UpdateSelection();
    }

    private void StartSelectedLevel()
    {
        SceneManager.LoadScene(_levels[_currentIndex].sceneName);
    }

    private void UpdateSelection()
    {
        LevelOption selectedLevel = _levels[_currentIndex];
        _levelNameText.text = selectedLevel.displayName;
        _previewImage.sprite = LoadPreviewSprite(selectedLevel.previewResourcePath);
    }

    private Sprite LoadPreviewSprite(string resourcePath)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0];
        }

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning($"Missing preview sprite at Resources/{resourcePath}");
        }

        return sprite;
    }

    private Button CreateButton(string objectName, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, string label, int fontSize)
    {
        RectTransform rect = CreateRect(objectName, parent, anchor, anchor, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.74f, 0.43f, 0.95f);

        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.92f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.82f, 0.88f, 0.98f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        button.colors = colors;
        button.targetGraphic = image;

        TMP_Text labelText = CreateText("Label", rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, fontSize, TextAlignmentOptions.Center);
        labelText.text = label;
        labelText.color = new Color(0.12f, 0.12f, 0.15f, 1f);

        return button;
    }

    private TMP_Text CreateText(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, int fontSize, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(objectName, parent, anchorMin, anchorMax, size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = _fontAsset;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = false;
        return text;
    }

    private RectTransform CreateRect(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }
}
