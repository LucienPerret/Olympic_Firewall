using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public TargetType targetType;
    public float range;
    public float damage;
    public float damageInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float projectileSize;
    public int cost;
    
}

public enum TargetType{
    Single,
    Multi, //implement later
    Area
}