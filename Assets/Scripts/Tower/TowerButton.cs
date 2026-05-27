using System;
using UnityEngine;

public class TowerButton : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;
    public static event Action<GameObject> OnTowerbuttonClick;

    public void OnClick()
    {
        SoundManager.Instance?.PlayButtonClick();
        OnTowerbuttonClick?.Invoke(towerPrefab);
        Debug.Log("ButtonPressed");
    }
}
