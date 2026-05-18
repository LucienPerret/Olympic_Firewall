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

    private Button _previousButton;
    private Button _nextButton;
    private Button _startButton;
    private Image _previewImage;
    private TMP_Text _selectedLevelText;
    private int _currentIndex;

    private void Awake()
    {
        _previousButton = FindRequired<Button>("Previous Button");
        _nextButton = FindRequired<Button>("Next Button");
        _startButton = FindRequired<Button>("Start Button");
        _previewImage = FindRequired<Image>("PreviewImage");
        _selectedLevelText = FindRequired<TMP_Text>("SelectedLevelText");

        if (_previousButton == null || _nextButton == null || _startButton == null || _previewImage == null || _selectedLevelText == null)
        {
            enabled = false;
            return;
        }

        _previousButton.onClick.RemoveAllListeners();
        _nextButton.onClick.RemoveAllListeners();
        _startButton.onClick.RemoveAllListeners();

        _previousButton.onClick.AddListener(ShowPreviousLevel);
        _nextButton.onClick.AddListener(ShowNextLevel);
        _startButton.onClick.AddListener(StartSelectedLevel);

        UpdateSelection();
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
        _selectedLevelText.text = selectedLevel.displayName;
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

    private T FindRequired<T>(string objectName) where T : Component
    {
        Transform child = transform.Find(objectName);
        if (child == null)
        {
            Debug.LogError($"LevelSelectMenu could not find scene object '{objectName}'.");
            return null;
        }

        T component = child.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Scene object '{objectName}' is missing component {typeof(T).Name}.");
        }

        return component;
    }
}
