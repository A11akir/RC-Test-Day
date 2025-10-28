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
        private UIDocument uiDocument;
        private VisualElement buttonInstance;
        private VisualTreeAsset buttonAsset;
        
        private VisualElement _buildingSelectWindow;

        private VisualElement _buildingWindow;
        private VisualElement _buildingMenu;
        private Button _openBuildingWindowButton;
        private Button _upgardeBuildingMenuButton;
        private InputSystem _inputSystem;

        private bool _isSelectBuildingWindowOpen;
        private bool _isBuildingWindowMode;
        private bool _isBuildMenu;
        private GameObject _tempBuilding;
        private BuildMenuSystem _tempBuildMenu;
        private Camera _mainCamera;

        private Vector2 _lastGridPosition;
        private BuildingData _currentBuildingData;
        public BuildingData CurrentBuildingData => _currentBuildingData;
        
        private Label _currencyText;
        private Label _costUpgrade;
        private Label _nameBuilding;
        private Label _levelBuilding;
        
        private int _currencyAmount = 0;
        private int _currencyAmountMax = 100;

        private float _mineInterval = 1f;
        private Coroutine _mineCoroutine;
        
        private readonly List<BuildingType> _activeBuildings = new();
        
        
        private void OnEnable()
        {
            _checkerboardBackground.gameObject.SetActive(false);
            
            if (_checkerboardBackground == null)
                return;

            if (_checkerboardBackground.GridMap == null)
                _checkerboardBackground.ForceInitGridMap();

            _gridMap = _checkerboardBackground.GridMap;
            _mainCamera = Camera.main;

            _inputSystem = new InputSystem();
            _inputSystem.Enable();

            var root = GetComponentInParent<UIDocument>().rootVisualElement;

            _currencyText = root.Q<Label>("CurrencyText");
            _costUpgrade = root.Q<Label>("CostUpgradeText");
            _nameBuilding = root.Q<Label>("NameBuildingText");
            _levelBuilding = root.Q<Label>("LevelText");
            UpdateCurrencyText();
            
            _openBuildingWindowButton = root.Q<Button>("OpenBuildingWindowButton");
            _upgardeBuildingMenuButton = root.Q<Button>("UpgradeButton");
            _buildingSelectWindow = root.Q<VisualElement>("BuildingSelectWindow");
            _buildingWindow = root.Q<VisualElement>("BuildingWindow");
            _buildingMenu = root.Q<VisualElement>("BuildMenu");
            uiDocument = GetComponentInParent<UIDocument>();
            buttonAsset = Resources.Load<VisualTreeAsset>("Button");
            
            _openBuildingWindowButton.RegisterCallback<ClickEvent>(OnOpenBuildingWindow);
            
            _upgardeBuildingMenuButton.RegisterCallback<ClickEvent>(_ =>
            {
                UpgardeBuild(_currentBuildingData.buildingType);
            });
            

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

        private void UpgardeBuild(BuildingType buildingType)
        {
            CheckActiveBuilding(buildingType);
            _currentBuildingData.Level += 1;
            UpdateMenuText();
        }

        private void OnDisable()
        {
            _inputSystem.CoreGameplay.MousePosition.performed -= OnMouseMove;
            _inputSystem.CoreGameplay.ResetSelected.performed -= ctx => CloseBuildingWindow();
            _inputSystem.CoreGameplay.SetBuilding.performed -= ctx => PlaceBuilding();
        }

        private void OnOpenBuildingWindow(ClickEvent evt)
        {
            _isSelectBuildingWindowOpen = !_isSelectBuildingWindowOpen;

            if (_isSelectBuildingWindowOpen)
                OpenBuildingWindow();
            else
                CloseBuildingWindow();
        }

        private void CloseBuildingWindow()
        {
            _isSelectBuildingWindowOpen = !_isSelectBuildingWindowOpen;
            if (_isBuildingWindowMode)
            {
                _buildingSelectWindow.RemoveFromClassList("building-window-full-hide");
                _buildingWindow.RemoveFromClassList("head-window-down");
                Destroy(_tempBuilding);
                _checkerboardBackground.gameObject.SetActive(false);
            }
            else if (_isBuildMenu)
            {
                CloseBuildMenu();
                FullShowWindows();
                Debug.Log(_isBuildMenu);
                _isSelectBuildingWindowOpen = true;
            }
            else
            {
                Debug.Log(1);
                _buildingSelectWindow.RemoveFromClassList("building-window-show");
            }
        }

        private void OpenBuildingWindow()
        {
            _buildingSelectWindow.AddToClassList("building-window-show");
        }

        public void OpenBuildMenu()
        {
            _isBuildingWindowMode = false;
            _isSelectBuildingWindowOpen = true;
            _isBuildMenu = true;
            _buildingMenu.RemoveFromClassList("head-window-down");
            UpdateMenuText();
        }
        private void CloseBuildMenu()
        {
            _buildingMenu.AddToClassList("head-window-down");
            _isBuildMenu = false;
        }

        private void SpawnBuilding(int index)
        {
            if (_tempBuilding != null)
                Destroy(_tempBuilding);

            _isBuildingWindowMode = true;
            _currentBuildingData = buildingDataList[index];

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            worldPos.z = 0f;

            FullHideWindows();
            _checkerboardBackground.gameObject.SetActive(true);
            _tempBuilding = Instantiate(_currentBuildingData.prefab, worldPos, Quaternion.identity);
            
            _tempBuildMenu = _tempBuilding.GetComponent<BuildMenuSystem>();
            if (_tempBuildMenu != null)
            {
                _tempBuildMenu.Init(this, uiDocument, buttonAsset);
                _tempBuildMenu.UpdateButtonSize();
            }
            
            BuildingMode();
        }


        public void FullHideWindows()
        {
            _buildingSelectWindow.AddToClassList("building-window-full-hide");
            _buildingWindow.AddToClassList("head-window-down");
        }
        public void FullShowWindows()
        {
            _buildingSelectWindow.RemoveFromClassList("building-window-full-hide");
            _buildingSelectWindow.RemoveFromClassList("building-window-show");
            _buildingWindow.RemoveFromClassList("head-window-down");
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

            bool canBuild = CanPlaceBuilding(startX, startY, _currentBuildingData.sizeWorld);

            _tempBuildMenu.UpdateSortOrder();
            _tempBuildMenu.UpdateButtonPosition();
            Color targetColor = canBuild
                ? new Color(0f, 1f, 0f, 0.5f)
                : new Color(1f, 0f, 0f, 0.5f);

            foreach (var renderer in _tempBuilding.GetComponentsInChildren<SpriteRenderer>())
                renderer.color = targetColor;
            _tempBuildMenu.UpdateButtonSize();
        }

        private void PlaceBuilding()
        {
            if (_tempBuilding == null || _currentBuildingData == null)
                return;

            int startX = Mathf.FloorToInt(_tempBuilding.transform.position.x);
            int startY = Mathf.FloorToInt(_tempBuilding.transform.position.y);

            if (CanPlaceBuilding(startX, startY, _currentBuildingData.sizeWorld))
            {
                foreach (var renderer in _tempBuilding.GetComponentsInChildren<SpriteRenderer>())
                    renderer.color = Color.white;

                SetBuildingOccupied(startX, startY, _currentBuildingData.sizeWorld, true);

                _activeBuildings.Add(_currentBuildingData.buildingType);
                CheckActiveBuilding(_currentBuildingData.buildingType);
                FullShowWindows();
                _isSelectBuildingWindowOpen = false;
                _tempBuildMenu.UpdateButtonSize();
                _isBuildingWindowMode = false;
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
        private void UpdateMenuText()
        {
            _nameBuilding.text = _currentBuildingData.buildingType.ToString();
            _costUpgrade.text = $"Cost :{_currentBuildingData.CostUpgrade.ToString()}";
            _levelBuilding.text = $"Level :{_currentBuildingData.Level.ToString()}";
        }
    }
}
