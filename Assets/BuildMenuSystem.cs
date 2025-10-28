using Content._2D.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BuildMenuSystem : MonoBehaviour
{
    private VisualElement _buildingSelectWindow;

    private UIController _uiController;

    private BuildingData _buildingData;

    private VisualElement buttonInstance;
    private UIDocument uiDocument;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Init(UIController uiController, UIDocument doc, VisualTreeAsset buttonUxml)
    {
        _uiController = uiController;
        _buildingData = uiController.CurrentBuildingData;
        
        uiDocument = doc;
        buttonInstance = buttonUxml.CloneTree();
        uiDocument.rootVisualElement.Add(buttonInstance);

        var button = buttonInstance.Q<Button>();

        button.RegisterCallback<ClickEvent>(evt => { BuildingMenu(); });

        UpdateSortOrder();
    }

    private void BuildingMenu()
    {
        _uiController.FullHideWindows();
        _uiController.OpenBuildMenu();
    }

    public void UpdateSortOrder()
    {
        _spriteRenderer.sortingOrder = -(int)transform.position.y;
    }

    public void UpdateButtonSize()
    {
        if (buttonInstance == null) return;

        float worldWidth = _buildingData.sizeSprite.x;
        float worldHeight = _buildingData.sizeSprite.y;

        Vector3 bottomLeft = Camera.main.WorldToScreenPoint(Vector3.zero);
        Vector3 topRight = Camera.main.WorldToScreenPoint(new Vector3(worldWidth, worldHeight, 0));

        float widthPx = topRight.x - bottomLeft.x;
        float heightPx = topRight.y - bottomLeft.y;

        buttonInstance.style.width = widthPx;
        buttonInstance.style.height = heightPx;
    }

    public void UpdateButtonPosition()
    {
        if (buttonInstance == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        buttonInstance.style.position = new StyleEnum<Position>(Position.Absolute);
        buttonInstance.style.left = screenPos.x;
        buttonInstance.style.top = Screen.height - screenPos.y - (_buildingData.sizeSprite.y * 100);
    }
}