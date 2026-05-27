using System;
using UnityEngine;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;
    public static event Action<GameObject> OnTowerbuttonClick;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button == null || towerPrefab == null)
        {
            return;
        }

        ButtonHoverTooltip tooltip = GetComponent<ButtonHoverTooltip>();
        if (tooltip == null)
        {
            tooltip = gameObject.AddComponent<ButtonHoverTooltip>();
        }

        tooltip.Configure(BuildTooltipText);
    }

    public void OnClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        OnTowerbuttonClick?.Invoke(towerPrefab);
        Debug.Log("ButtonPressed");
    }

    private string BuildTooltipText()
    {
        Tower tower = towerPrefab.GetComponent<Tower>();
        if (tower == null || tower.Data == null)
        {
            return string.Empty;
        }

        TowerData data = tower.Data;
        return $"Cost: {data.cost}\nDamage: {FormatNumber(data.damage)}\nDamageIntervall: {FormatNumber(data.damageInterval)}\nRange: {FormatNumber(data.range)}";
    }

    private static string FormatNumber(float value)
    {
        return Mathf.Approximately(value % 1f, 0f) ? value.ToString("0") : value.ToString("0.##");
    }
}
