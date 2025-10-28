using Content._2D.UI;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    public GameObject prefab;
    public Vector2Int sizeWorld;
    public Vector2Int sizeSprite;
    public BuildingType buildingType;
    public int CostUpgrade;
    public int Level;
}
