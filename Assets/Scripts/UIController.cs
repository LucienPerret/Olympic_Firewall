using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private Button nextWaveButton;

    private void Awake()
    {
        if (nextWaveButton == null)
        {
            nextWaveButton = GameObject.Find("NextWave")?.GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        Spawner.OnWaveStateChanged += UpdateNextWaveButtonState;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        Spawner.OnWaveStateChanged -= UpdateNextWaveButtonState;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
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
            nextWaveButton.interactable = !isWaveActive;
        }
    }

    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
    }

    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
    }
}
