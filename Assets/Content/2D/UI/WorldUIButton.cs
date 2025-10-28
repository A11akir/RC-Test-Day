using UnityEngine;
using UnityEngine.UIElements;

public class WorldUIButton : MonoBehaviour
{
    private VisualElement buttonInstance;
    private UIDocument uiDocument;

    public void Init(UIDocument doc, VisualTreeAsset buttonUxml)
    {
        uiDocument = doc;
        buttonInstance = buttonUxml.CloneTree();
        uiDocument.rootVisualElement.Add(buttonInstance);
    }

    void Update()
    {
        if (buttonInstance == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        buttonInstance.style.left = screenPos.x;
        buttonInstance.style.top = Screen.height - screenPos.y;
    }
}