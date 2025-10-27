using Content._2D.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    public GameObject prefab;
    public Vector2Int size;
    public BuildingType buildingType;
}
