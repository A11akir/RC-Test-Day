using Content._2D.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildMenuSystem : MonoBehaviour
{
    private InputSystem _inputSystem;
    
    private VisualElement _buildingSelectWindow;
    
    private void OnEnable()
    {
        _inputSystem = new InputSystem();
        _inputSystem.Enable();
        
        _inputSystem.CoreGameplay.SetBuilding.performed += ctx => BuildingMenu();
        
        /*_buildingMenuWindow.SetActive(false);*/
    }

    private void BuildingMenu()
    {
        /*_buildingMenuWindow.SetActive(true);*/
    }
}
