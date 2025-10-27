using System.Collections;
using System.Collections.Generic;
using Domain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Content._2D.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private BuildingData[] buildingDataList;
        [SerializeField] private CheckerboardBackground _checkerboardBackground;

        private GridMap _gridMap;

        private VisualElement _buildingSelectWindow;
        private VisualElement _buildingWindow;
        private Button _openBuildingWindowButton;
        private InputSystem _inputSystem;

        private bool _isBuildingWindowOpen;
        private GameObject _tempBuilding;
        private Camera _mainCamera;

        private Vector2 _lastGridPosition;
        private BuildingData _currentBuildingData;
        
        private Label _currencyText;

        private int _currencyAmount = 0;
        private int _currencyAmountMax = 100;

        private float _mineInterval = 1f; // Интервал добычи в секундах
        private Coroutine _mineCoroutine;
        
        private readonly List<BuildingType> _activeBuildings = new();
        
        
        private void OnEnable()
        {
            _checkerboardBackground.gameObject.SetActive(false);
            _gridMap = _checkerboardBackground.GridMap;
            _mainCamera = Camera.main;

            _inputSystem = new InputSystem();
            _inputSystem.Enable();

            var root = GetComponentInParent<UIDocument>().rootVisualElement;

            _currencyText = root.Q<Label>("CurrencyText");
            UpdateCurrencyText();
            
            _openBuildingWindowButton = root.Q<Button>("OpenBuildingWindowButton");
            _buildingSelectWindow = root.Q<VisualElement>("BuildingSelectWindow");
            _buildingWindow = root.Q<VisualElement>("BuildingWindow");

            _openBuildingWindowButton.RegisterCallback<ClickEvent>(OnOpenBuildingWindow);

            _inputSystem.CoreGameplay.ResetSelected.performed += ctx => CloseBuildingWindow();
            _inputSystem.CoreGameplay.MousePosition.performed += OnMouseMove;
            _inputSystem.CoreGameplay.SetBuilding.performed += ctx => PlaceBuilding();

            for (int i = 0; i < buildingDataList.Length; i++)
            {
                var button = root.Q<Button>($"SetBuildingButton{i + 1}");

                if (button != null)
                {
                    int index = i;
                    button.RegisterCallback<ClickEvent>(_ => { SpawnBuilding(index); });
                }
            }
        }

        private void OnDisable()
        {
            _inputSystem.CoreGameplay.MousePosition.performed -= OnMouseMove;
            _inputSystem.CoreGameplay.ResetSelected.performed -= ctx => CloseBuildingWindow();
            _inputSystem.CoreGameplay.SetBuilding.performed -= ctx => PlaceBuilding();
        }

        private void OnOpenBuildingWindow(ClickEvent evt)
        {
            _isBuildingWindowOpen = !_isBuildingWindowOpen;

            if (_isBuildingWindowOpen)
                OpenBuildingWindow();
            else
                CloseBuildingWindow();
        }

        private void CloseBuildingWindow()
        {
            _buildingSelectWindow.RemoveFromClassList("building-window-show");
            Destroy(_tempBuilding);
            _checkerboardBackground.gameObject.SetActive(false);
        }

        private void OpenBuildingWindow()
        {
            _buildingSelectWindow.AddToClassList("building-window-show");
        }

        private void SpawnBuilding(int index)
        {
            if (_tempBuilding != null)
                Destroy(_tempBuilding);

            _currentBuildingData = buildingDataList[index];

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            worldPos.z = 0f;

            _buildingSelectWindow.AddToClassList("building-window-full-hide");
            _buildingWindow.AddToClassList("head-window-down");
            _checkerboardBackground.gameObject.SetActive(true);
            _tempBuilding = Instantiate(_currentBuildingData.prefab, worldPos, Quaternion.identity);
        }

        private void OnMouseMove(InputAction.CallbackContext ctx)
        {
            if (_tempBuilding == null)
                return;

            Vector2 mouseScreenPos = ctx.ReadValue<Vector2>();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            worldPos.z = 0f;

            Vector2 gridPos = new Vector2(Mathf.Floor(worldPos.x), Mathf.Floor(worldPos.y));

            if (gridPos != _lastGridPosition)
            {
                _tempBuilding.transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
                _lastGridPosition = gridPos;
            }

            BuildingMode();
        }

        private void BuildingMode()
        {
            if (_tempBuilding == null || _currentBuildingData == null) return;

            int startX = Mathf.FloorToInt(_tempBuilding.transform.position.x);
            int startY = Mathf.FloorToInt(_tempBuilding.transform.position.y);

            bool canBuild = CanPlaceBuilding(startX, startY, _currentBuildingData.size);

            Color targetColor = canBuild
                ? new Color(0f, 1f, 0f, 0.5f)
                : new Color(1f, 0f, 0f, 0.5f);

            foreach (var renderer in _tempBuilding.GetComponentsInChildren<SpriteRenderer>())
                renderer.color = targetColor;
        }

        private void PlaceBuilding()
        {
            if (_tempBuilding == null || _currentBuildingData == null)
                return;

            int startX = Mathf.FloorToInt(_tempBuilding.transform.position.x);
            int startY = Mathf.FloorToInt(_tempBuilding.transform.position.y);

            if (CanPlaceBuilding(startX, startY, _currentBuildingData.size))
            {
                foreach (var renderer in _tempBuilding.GetComponentsInChildren<SpriteRenderer>())
                    renderer.color = Color.white;

                SetBuildingOccupied(startX, startY, _currentBuildingData.size, true);

                _activeBuildings.Add(_currentBuildingData.buildingType);
                CheckActiveBuilding(_currentBuildingData.buildingType);
                
                _isBuildingWindowOpen = false;
                _buildingSelectWindow.RemoveFromClassList("building-window-full-hide");     
                _buildingWindow.RemoveFromClassList("head-window-down");
                _buildingWindow.AddToClassList("head-window-up");
                
                _checkerboardBackground.gameObject.SetActive(false);
                _tempBuilding = null;
            }
        }
        
        private bool CanPlaceBuilding(int centerX, int centerY, Vector2Int size)
        {
            int halfWidth = Mathf.FloorToInt(size.x / 2f);
            int halfHeight = Mathf.FloorToInt(size.y / 2f);

            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    int checkX = centerX + x;
                    int checkY = centerY + y;

                    if (_gridMap.IsOccupiedWorld(checkX, checkY))
                        return false;
                }
            }

            return true;
        }
        
        private void SetBuildingOccupied(int centerX, int centerY, Vector2Int size, bool value)
        {
            int halfWidth = Mathf.FloorToInt(size.x / 2f);
            int halfHeight = Mathf.FloorToInt(size.y / 2f);

            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    int setX = centerX + x;
                    int setY = centerY + y;

                    _gridMap.SetOccupiedWorld(setX, setY, value);
                }
            }
        }

        private void CheckActiveBuilding(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Storage:
                    _currencyAmountMax += 50;
                    _currencyAmountMax *= 2;
                    UpdateCurrencyText();
                    break;

                case BuildingType.Mine:
                    if (_mineCoroutine == null)
                        _mineCoroutine = StartCoroutine(MineRoutine());
                    break;

                case BuildingType.Monastery:
                    _mineInterval /= 2f;
                    _currencyAmountMax += 50;
                    UpdateCurrencyText();
                    break;
            }
        }

        private IEnumerator MineRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_mineInterval);
                if (_currencyAmount < _currencyAmountMax)
                {
                    _currencyAmount = Mathf.Min(_currencyAmount + 5, _currencyAmountMax);
                    UpdateCurrencyText();
                }
            }
        }

        private void UpdateCurrencyText()
        {
            _currencyText.text = $"{_currencyAmount}/{_currencyAmountMax}";
        }
    }
}
