using Assets.Scripts;
using Assets.Scripts.Models.Equipment;
using Assets.Scripts.Models.Slot;
using Assets.UnitsCharacteristics;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

public interface IEquipmentService
{
    void SetEquipemtPanelActive(bool state);

    void ChangeCellBackGroundColor(CharacterEquipmentSlotType slotType, bool isSetActive);
}

public class EquipmentService : IEquipmentService
{
    private const string EQUIPMENT_ITEMS_PANEL_NAME = "EquipmentItemsPanel";
    private const string INVENTORY_GRID_NAME = "InventoryGrid";
    private const string CELL_IMAGE_CHILDREN_NAME = "Image";
    private const string CHARECTER_PANEL_NAME = "CharacterPanel";
    private const string CHARECTER_PAG_PANEL_NAME = "BagPanel";
    private const string BACKGROUND_IMAGE_NAME = "BackgroundImage";
    private const string CHARECTER_CHOOSE_PANEL_NAME = "CharecterChoosePanel";
    private const string CHARACTER_NAME_PANEL_NAME = "CharacterNamePanel";
    private const string RUNE_PANEL_NAME = "RunePanel";

    private static readonly Color _cellActiveBackGroundColor = new(161/255f, 92/255f, 42/255f, 128/255f);
    private static readonly Color _cellUnActiveBackgroundColor = new(161/255f, 92/255f, 42/255f, 255/255f);

    private readonly IUnitManager _unitManager;

    private readonly IUiService _uiService;

    private readonly IGameGlobalStateManager _gameGlobalStateManager;

    private readonly GameObject _equipmentPanel;

    private readonly GameObject _equipmentSlotPrefab;

    // переделать под кнопку персонажа
    private readonly GameObject _actionButtonPrefab;

    private readonly GameObject _inventoryGrid;

    private readonly GameObject _charecterPanel;

    private readonly GameObject _charecterBagPanel;

    private readonly GameObject _charecterChoosePanel;

    private readonly TextMeshProUGUI _charecterNamePanelText;

    private readonly List<GameObject> _sharedCells = new();

    private readonly List<GameObject> _characterCells = new();

    private readonly List<GameObject> _charectersSpawnedButtons = new();

    private Unit? _selectedUnit = null;

    public static EquipmentService Instance {  get; private set; }

    public EquipmentService(
        [Inject(Id = Constants.EquipemntPanel)] GameObject equipemntPanel,
        [Inject(Id = Constants.EquipemntSlotPrefab)] GameObject equipmentSlotPrefab,
        [Inject(Id = Constants.ActionButtonPrefab)] GameObject actionButtonPrefab,
        [Inject] IUiService uiService,
        [Inject] IUnitManager unitManager,
        [Inject] IGameGlobalStateManager gameGlobalStateManager
    )
    {
        _equipmentPanel = equipemntPanel;
        _equipmentSlotPrefab = equipmentSlotPrefab;
        _actionButtonPrefab = actionButtonPrefab;

        var equipmentPanelTransform = _equipmentPanel.transform;
        
        _inventoryGrid = equipmentPanelTransform.Find(EQUIPMENT_ITEMS_PANEL_NAME).Find(INVENTORY_GRID_NAME).gameObject;
        _charecterPanel = equipmentPanelTransform.Find(CHARECTER_PANEL_NAME).gameObject;
        _charecterBagPanel = _charecterPanel.transform.Find(CHARECTER_PAG_PANEL_NAME).gameObject;
        _charecterChoosePanel = equipmentPanelTransform.Find(CHARECTER_CHOOSE_PANEL_NAME).gameObject;
        _charecterNamePanelText = _charecterPanel.transform.Find(CHARACTER_NAME_PANEL_NAME).gameObject.GetComponentInChildren<TextMeshProUGUI>();

        _unitManager = unitManager;
        _uiService = uiService;
        _gameGlobalStateManager = gameGlobalStateManager;

        Instance = this;
    }

    public void SetEquipemtPanelActive(bool state)
    {
        if (state)
        {
            GenerateCells();
            var defaultUserUnit = _unitManager.Units
                .OrderBy(x => x.Name)
                .FirstOrDefault(x => x.Characterictics.Side == SideType.UserSide)
                ?? throw new System.Exception("Нет unit-ов для пользователя(");
            _selectedUnit = defaultUserUnit;
            _charecterNamePanelText.text = _selectedUnit.Name;
            DrawCharecterButtons();
            DrawSharedEquipment();
            DrawUnitEquipment();
        }
        else
        {
            HideCells();
            HideCharecterButtons();

            if (_gameGlobalStateManager.SelectedUnit != null)
            {
                _uiService.RefreshUnitUi(_gameGlobalStateManager.SelectedUnit);
            }

            _selectedUnit = null;
        }

        _equipmentPanel.SetActive(state);
    }

    private void GenerateCells()
    {
        for(var i = 0; i < Constants.MAX_EQUIPMENT_ITEMS_CELL_COUNT; i++)
        {
            var cell = Object.Instantiate(
                original: _equipmentSlotPrefab,
                parent: _inventoryGrid.transform
            );

            var slotInfo = cell.AddComponent<SlotInfo>();
            slotInfo.CellType = CellType.SharedStorage;
            slotInfo.Order = i;
            slotInfo.EquipmentSetType = null;

            _sharedCells.Add(cell);
        }

        GenerateCharecterCells();
    }

    private void GenerateCharecterCells()
    {
        foreach ((var type, var count) in EquipemntHelper.CHARECTER_EQUIPMENT_SLOTS)
        {
            for (var i = 0; i < count; i++)
            {
                var isBagCell = type == CharacterEquipmentSlotType.Bag;
                var charecterCell = Object.Instantiate(
                    original: _equipmentSlotPrefab,
                    parent: isBagCell ? _charecterBagPanel.transform : _charecterPanel.transform
                );

                var slotInfo = charecterCell.AddComponent<SlotInfo>();
                slotInfo.EquipmentSetType = type;
                slotInfo.Order = i;
                slotInfo.CellType = CellType.Unit;

                var rect = charecterCell.GetComponent<RectTransform>();
                if (!isBagCell)
                {
                    var (xCordinate, yCordinate) = EquipemntHelper.CHARECTER_EQUIPMENT_CORDINATES[(type, i)];
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(xCordinate, yCordinate);

                    rect.sizeDelta = new Vector2(65, 65);
                }

                var equipmentImage = charecterCell.transform.Find(CELL_IMAGE_CHILDREN_NAME).gameObject;

                equipmentImage.SetActive(false);

                _characterCells.Add(charecterCell);
            }
        }
    }

    private void HideCells()
    {
        if (_sharedCells.Count() == 0 && _characterCells.Count() == 0)
        {
            return;
        }

        foreach(var cell in _sharedCells)
        {
            Object.Destroy(cell);
        }

        _sharedCells.Clear();

        foreach(var cell in _characterCells)
        {
            Object.Destroy(cell);
        }

        _characterCells.Clear();
    }

    private void DrawSharedEquipment()
    {
        var index = 0;

        foreach (var cell in _sharedCells)
        {
            ClearRunePanel(cell);

            var equipmentImage = cell.transform.Find(CELL_IMAGE_CHILDREN_NAME).gameObject;

            var cellImage = equipmentImage.GetComponent<Image>();
            var equipment = _unitManager.SharedEquipment
                .Where(x => x.Order == index)
                .Select(x => x.Equipment)
                .FirstOrDefault();

            if(equipment != null)
            {
                cellImage.sprite = Resources.Load<Sprite>(IconHelper.GetIconFullPath(equipment));
                equipmentImage.SetActive(true);
                if(equipment.SlotType != CharacterEquipmentSlotType.Rune)
                {
                    var item = equipment as BaseEquipment;
                    DrawEquipmentRunesIternal(cell, item.Runes);
                }
            }
            else
            {
                equipmentImage.SetActive(false);
            }

            index++;
        }
    }

    private void DrawUnitEquipment()
    {
        if(_selectedUnit == null)
        {
            Debug.LogError("Can not draw equipment");
            return;
        }

        foreach (var cell in _characterCells)
        {
            var equipmentImage = cell.transform.Find(CELL_IMAGE_CHILDREN_NAME).gameObject;

            var cellImage = equipmentImage.GetComponent<Image>();

            cellImage.sprite = null;
            equipmentImage.SetActive(false);
            ClearRunePanel(cell);
        }

        foreach (var equipmentSlot in _selectedUnit.EqupmentSlots)
        {
            if (!equipmentSlot.IsEquipped)
            {
                continue; // Пропускаем пустые слоты, но продолжаем цикл
            }

            var cell = _characterCells.Select(x => new { SlotGo = x, SlotInfo = x.GetComponent<SlotInfo>() })
                .Where(x => x.SlotInfo.Order == equipmentSlot.Order)
                .Where(x => x.SlotInfo.EquipmentSetType.Value == equipmentSlot.Type)
                .Select(x => x.SlotGo)
                .FirstOrDefault()
                ?? throw new System.Exception("Нет unit-ов для пользователя(");

            var equipmentImage = cell.transform.Find(CELL_IMAGE_CHILDREN_NAME).gameObject;

            var cellImage = equipmentImage.GetComponent<Image>();

            cellImage.sprite = Resources.Load<Sprite>(IconHelper.GetIconFullPath(equipmentSlot.Equipment));

            var item = equipmentSlot.Equipment as BaseEquipment;
            // Проверяем, что предмет поддерживает руны перед отрисовкой
            if (item != null && item.Runes != null)
            {
                DrawEquipmentRunesIternal(cell, item.Runes);
            }
            equipmentImage.SetActive(true);
        }
    }

    public void ChangeCellBackGroundColor(CharacterEquipmentSlotType slotType, bool isSetActive)
    {
        var selectedUnitSlots = _selectedUnit.EqupmentSlots
            .Where(x => x.Equipment != null)
            .Select(x => new { x.Order, x.Equipment});

        var hilightCells = slotType != CharacterEquipmentSlotType.Rune 
            ? _characterCells.Where(x => x.GetComponent<SlotInfo>().EquipmentSetType == slotType).ToArray()
            : _characterCells.Where(
                x => RuneHelper.IsRunCanBeSet(x.GetComponent<SlotInfo>().EquipmentSetType.Value)
                && selectedUnitSlots.Any(su => 
                    su.Equipment.SlotType == x.GetComponent<SlotInfo>().EquipmentSetType.Value
                    && su.Order == x.GetComponent<SlotInfo>().Order
                )
            )
            .ToArray();

        var backgroundColor = isSetActive ? _cellActiveBackGroundColor : _cellUnActiveBackgroundColor;
        foreach (var cell in hilightCells)
        {
            var backGroundImageGO = cell.transform.Find(BACKGROUND_IMAGE_NAME);
            var backGroundImage = backGroundImageGO.GetComponent<Image>();
            backGroundImage.color = backgroundColor;
        }

        if(slotType == CharacterEquipmentSlotType.Rune)
        {
            foreach ((var equipment, var order) in _unitManager.SharedEquipment)
            {
                if (equipment is not BaseEquipment)
                {
                    continue;
                }

                var cell = _sharedCells.FirstOrDefault(x => x.GetComponent<SlotInfo>().Order == order) ?? throw new System.Exception();
                var backGroundImageGO = cell.transform.Find(BACKGROUND_IMAGE_NAME);
                var backGroundImage = backGroundImageGO.GetComponent<Image>();
                backGroundImage.color = backgroundColor;
            }
        }
    }

    public void OnCellDrop(GameObject fromSlotGO, GameObject toSlotGO)
    {
        var fromSlotInfo = fromSlotGO.GetComponent<SlotInfo>();
        var toSlotInfo = toSlotGO.GetComponent<SlotInfo>();

        switch (fromSlotInfo.CellType, toSlotInfo.CellType)
        {
            case (CellType.Unit, CellType.Unit):
            case (CellType.SharedStorage, CellType.SharedStorage):
                SwapOrders(
                    fromSlotInfo: fromSlotInfo,
                    toSlotInfo: toSlotInfo
                );
                break;
            case (CellType.SharedStorage, CellType.Unit):
                OnUnit(
                    fromSlotInfo: fromSlotInfo,
                    toSlotInfo: toSlotInfo
                );
                break;
            case (CellType.Unit, CellType.SharedStorage):
                FromUnit(
                    fromSlotInfo: fromSlotInfo,
                    toSlotInfo: toSlotInfo
                );
                break;
            default:
                Debug.LogWarning($"Нет логики для взаимодействия {fromSlotInfo.CellType} -> {toSlotInfo.CellType}");
                break;
        }

        DrawSharedEquipment();
        DrawUnitEquipment();
    }

    private void SwapOrders(SlotInfo fromSlotInfo, SlotInfo toSlotInfo)
    {
        if (_selectedUnit == null)
        {
            Debug.LogError("Can not draw equipment");
            return;
        }

        if (fromSlotInfo.EquipmentSetType != null
            && toSlotInfo.EquipmentSetType != null
            && fromSlotInfo.CellType == CellType.Unit
        )
        {
            if (fromSlotInfo.EquipmentSetType != toSlotInfo.EquipmentSetType)
            {
                return;
            }

            var fromEquipmentSlot = _selectedUnit.EqupmentSlots
                .Where(x => x.Order == fromSlotInfo.Order)
                .Where(x => x.Type == fromSlotInfo.EquipmentSetType)
                .FirstOrDefault();

            if (fromEquipmentSlot == null || fromEquipmentSlot.Equipment == null)
            {
                Debug.LogError("Бля братан, ну что-то не так пошло (нет предмета из)");
                return;
            }

            _unitManager.SwapEquipmentUnit(
                unitName: _selectedUnit.Name,
                fromOrder: fromSlotInfo.Order,
                toOrder: toSlotInfo.Order,
                slotType: toSlotInfo.EquipmentSetType.Value
            );
        }
        else
        {
            var fromEquipment = _unitManager.SharedEquipment
               .Where(x => x.Order == fromSlotInfo.Order)
               .Select(x => x.Equipment)
               .FirstOrDefault();

            if (fromEquipment == null)
            {
                Debug.LogError("Бля братан, ну что-то не так пошло (нет предмета из)");
                return;
            }

            var toEquipment = _unitManager.SharedEquipment
                .Where(x => x.Order == toSlotInfo.Order)
                .Select(x => x.Equipment)
                .FirstOrDefault();
             
            if (fromEquipment.SlotType == CharacterEquipmentSlotType.Rune)
            {
                if(toEquipment == null)
                {
                    _unitManager.SetSharedEquipemt(
                        oldOrder: fromSlotInfo.Order,
                        newOrder: toSlotInfo.Order,
                        equipment: fromEquipment
                    );
                    return;
                }

                if (toEquipment.SlotType == CharacterEquipmentSlotType.Rune)
                {
                    _unitManager.SwapSharedEquipmentOrders(
                        leftOrder: fromSlotInfo.Order,
                        rightOrder: toSlotInfo.Order
                    );
                    return;
                }

                if (!RuneHelper.IsRunCanBeSet(toEquipment.SlotType))
                {
                    _unitManager.SwapSharedEquipmentOrders(
                        leftOrder: fromSlotInfo.Order,
                        rightOrder: toSlotInfo.Order
                    );
                    return;
                }

                var equipment = toEquipment as BaseEquipment;
                if (equipment.RuneMaxCount <= equipment.Runes.Count())
                {
                    return;
                }

                var rune = fromEquipment as Rune;

                // Убедимся, что список рун инициализирован
                if (equipment.Runes == null)
                {
                    equipment.Runes = new List<Rune>();
                }

                equipment.Runes.Add(rune);
                _unitManager.RemoveEquipmentFromSharedStorage(fromSlotInfo.Order);

                return;
            }

            if (toEquipment != null)
            {
                _unitManager.SwapSharedEquipmentOrders(
                    leftOrder: fromSlotInfo.Order,
                    rightOrder: toSlotInfo.Order
                );
                return;
            }

            _unitManager.SetSharedEquipemt(
                oldOrder: fromSlotInfo.Order,
                newOrder: toSlotInfo.Order,
                equipment: fromEquipment
            );
        }
    }

    private void OnUnit(SlotInfo fromSlotInfo, SlotInfo toSlotInfo)
    {
        var fromEquipment = _unitManager.SharedEquipment
            .Where(x => x.Order == fromSlotInfo.Order)
            .Select(x => x.Equipment)
            .FirstOrDefault();

        if ((fromEquipment.SlotType != toSlotInfo.EquipmentSetType) && fromEquipment.SlotType != CharacterEquipmentSlotType.Rune)
        {
            return;
        }

        if (fromEquipment == null)
        {
            Debug.LogError("Бля братан, ну что-то не так пошло (нет предмета из)");
            return;
        }

        var toEquipmentSlot = _selectedUnit.EqupmentSlots
            .Where(x => x.Order == toSlotInfo.Order)
            .Where(x => x.Type == toSlotInfo.EquipmentSetType)
            .FirstOrDefault();

        var isSlotEmpty = toEquipmentSlot == null || toEquipmentSlot.Equipment == null;

        if (fromEquipment.SlotType == CharacterEquipmentSlotType.Rune && isSlotEmpty)
        {
            return;
        }

        if(fromEquipment.SlotType == CharacterEquipmentSlotType.Rune && !isSlotEmpty)
        {
            var equipment = toEquipmentSlot.Equipment as BaseEquipment;

            if (equipment.RuneMaxCount <= equipment.Runes.Count())
            {
                return;
            }
            var rune = fromEquipment as Rune;

            // Убедимся, что список рун инициализирован
            if (equipment.Runes == null)
            {
                equipment.Runes = new List<Rune>();
            }

            equipment.Runes.Add(rune);

            // НЕ переприсваиваем Equipment - он уже содержит нужный объект
            // toEquipmentSlot.Equipment уже ссылается на equipment

            _unitManager.RemoveEquipmentFromSharedStorage(fromSlotInfo.Order);
            return;
        }

        if (isSlotEmpty)
        {
            var toSlotUnitInfo = _selectedUnit
                .EqupmentSlots
                .FirstOrDefault(x => x.Order == toSlotInfo.Order && x.Type == toSlotInfo.EquipmentSetType.Value);

            if (toSlotUnitInfo == null)
            {
                _selectedUnit.EqupmentSlots.Add(
                    new EquipmentSlot()
                    {
                        Type = toSlotInfo.EquipmentSetType.Value,
                        Order = toSlotInfo.Order,
                        Equipment = fromEquipment
                    }
                );
            }
            else
            {
                toSlotUnitInfo.Equipment = fromEquipment;
            }

            _unitManager.RemoveEquipmentFromSharedStorage(fromSlotInfo.Order);
        }
        else
        {
            var z = toEquipmentSlot.Equipment;
            toEquipmentSlot.Equipment = fromEquipment;
            _unitManager.SetSharedEquipemt(
                oldOrder: fromSlotInfo.Order,
                newOrder: fromSlotInfo.Order,
                equipment: z
            );
        }
    }

    private void FromUnit(SlotInfo fromSlotInfo, SlotInfo toSlotInfo)
    {
        var fromEquipmentSlot = _selectedUnit.EqupmentSlots
            .Where(x => x.Order == fromSlotInfo.Order)
            .Where(x => x.Type == fromSlotInfo.EquipmentSetType)
            .FirstOrDefault();

        if (fromEquipmentSlot == null || fromEquipmentSlot.Equipment == null)
        {
            Debug.LogError("Бля братан, ну что-то не так пошло (нет предмета из)");
            return;
        }

        var toEquipment = _unitManager.SharedEquipment
            .Where(x => x.Order == toSlotInfo.Order)
            .Select(x => x.Equipment)
            .FirstOrDefault();

        _unitManager.SetSharedEquipemt(
            oldOrder: toEquipment == null ? null : fromSlotInfo.Order,
            newOrder: toSlotInfo.Order,
            equipment: fromEquipmentSlot.Equipment
        );

        fromEquipmentSlot.Equipment = toEquipment;
    }

    private void DrawCharecterButtons()
    {
        var userUnits = _unitManager.Units.Where(x => x.Characterictics.Side == SideType.UserSide);

        foreach (var unit in userUnits)
        {
            var buttonGO = Object.Instantiate(
                original: _actionButtonPrefab,
                parent: _charecterChoosePanel.transform
            );

            var button = buttonGO.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => OnCharacterButtonClick(unit.Name));
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = unit.Name;
            }
            else
            {
                Debug.Log($"Бля не нашел текст для {unit.Name}");
            }

            _charectersSpawnedButtons.Add(buttonGO);
        }
    }

    private void HideCharecterButtons()
    {
        foreach(var button in _charectersSpawnedButtons)
        {
            if (button != null)
            {
                Object.Destroy(button);
            }
        }

        _charectersSpawnedButtons.Clear();
    }

    private void OnCharacterButtonClick(string newUnitName)
    {
        if(_selectedUnit != null && _selectedUnit.Name != newUnitName)
        {
            _selectedUnit = _unitManager
                .Units
                .FirstOrDefault(x => x.Name == newUnitName && x.Characterictics.Side == SideType.UserSide)
                ?? throw new System.Exception("Нет unit-ов для пользователя(");

            _charecterNamePanelText.text = _selectedUnit.Name;

            DrawUnitEquipment();
        }
    }

    private void DrawEquipmentRunesIternal(GameObject cell, IEnumerable<Rune> runes)
    {
        var runePanelTransform = cell.transform.Find(RUNE_PANEL_NAME);

        ClearRunePanel(runePanelTransform);

        if (runes == null || !runes.Any())
        {
            runePanelTransform.gameObject.SetActive(false);
            return;
        }

        foreach (var rune in runes)
        {
            var runeGO = new GameObject($"Rune_{rune.Type}");

            runeGO.transform.SetParent(runePanelTransform, false);

            runeGO.AddComponent<RectTransform>();
            var runeImage = runeGO.AddComponent<Image>();

            runeImage.sprite = Resources.Load<Sprite>(IconHelper.GetIconFullPath(rune));

            runeImage.raycastTarget = false;
        }

        runePanelTransform.gameObject.SetActive(true);
    }

    private void ClearRunePanel(GameObject cell)
    {
        var runePanelTransform = cell.transform.Find(RUNE_PANEL_NAME);

        foreach (Transform child in runePanelTransform)
        {
            Object.Destroy(child.gameObject);
        }
    }
    private void ClearRunePanel(Transform runePanelTransform)
    {
        foreach (Transform child in runePanelTransform)
        {
            Object.Destroy(child.gameObject);
        }
    }
}
