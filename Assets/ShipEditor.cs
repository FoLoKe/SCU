using UnityEngine;
using Game.UI;
using UnityEngine.UIElements;
using Assets.BlueprintUtils;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class ShipEditor : MonoBehaviour
{
    public BlueprintComponent BlueprintComp; // OBJECT TO EDIT
    public UIDocument UIDoc; // EDITOR UI DOC
    public PlayerInput PlayerInput; // INPUT CONTROLLER
    public CameraControlls cameraControlls;
    //private Blueprint _module;

    // TOOLS
    private Button brushButton;
    private Button eraiserButton;
    private Button pickerButton;
    private Button fillButton;
    private Button moveButton;

    private enum Tool { Brush, Eraser, Picker, Move, Fill, None }; //Editor tools types
    private Tool tool = Tool.None; //Tool to use on left click
    private Tool selectedTool = Tool.Brush; //Tool selected on the left panel

    // PALETTE
    private ColorPicker colorPicker;
    private VisualElement palette;

    private PaintButton selectedPaint; //Paint selected for the brush

    // SAVE DIALOG
    private VisualElement saveDialog;
    private Label saveLabel;
    private TextField saveNameField;

    private bool saveExistsChecked;

    // LOAD DIALOG
    private VisualElement loadDialog;
    private TabbedPane loadTabbedPane;
    private ScrollView builtInLoadPane;
    private ScrollView customLoadPane;

    private LoadEntry selectedLaodEntry;
    private Button confirmLoadButton;

    // INPUT
    private InputAction brushShortcut;
    private InputAction pickerShortcut;
    private InputAction eraserShortcut;
    private InputAction moveShortcut;
    private InputAction fillShortcut;

    private InputAction pickerQuickShortcut;
    private InputAction eraserQuickShortcut;
    private InputAction moveQuickShortcut;
    private InputAction fillQuickShortcut;

    private InputAction clickInput;

    // STYLE
    private Color focusColor = Color.white;
    private Color unfocusColor = new(0, 0, 0, 0);

    // TESTING
    AsyncOperationHandle<IList<TextAsset>> loadHandle;
    public List<string> keys = new List<string>() { "ships" };

    // Start is called before the first frame update
    void Start()
    {
        brushShortcut = PlayerInput.actions["Brush"];
        brushShortcut.performed += _ => OnToolChange(Tool.Brush);

        pickerShortcut = PlayerInput.actions["Picker"];
        pickerShortcut.performed += _ => OnToolChange(Tool.Picker);

        eraserShortcut = PlayerInput.actions["Eraser"];
        eraserShortcut.performed += _ => OnToolChange(Tool.Eraser);

        moveShortcut = PlayerInput.actions["Move"];
        moveShortcut.performed += _ => OnToolChange(Tool.Move);

        fillShortcut = PlayerInput.actions["Fill"];
        fillShortcut.performed += _ => OnToolChange(Tool.Fill);

        pickerQuickShortcut = PlayerInput.actions["QuickPicker"];

        eraserQuickShortcut = PlayerInput.actions["QuickEraser"];

        fillQuickShortcut = PlayerInput.actions["QuickFill"];

        moveQuickShortcut = PlayerInput.actions["QuickMove"];

        clickInput = PlayerInput.actions["Click"];
    }

    private void OnEnable() 
    {
        var root = UIDoc.rootVisualElement;

        // CONTROLS 
        var saveBlueprint = root.contentContainer.Q<Button>("SaveBP");
        saveBlueprint.clicked += () => OnSave();

        var loadBlueprint = root.contentContainer.Q<Button>("LoadBP");
        loadBlueprint.clicked += () => OnLoad();

        var newBlueprint = root.contentContainer.Q<Button>("NewBP");
        newBlueprint.clicked += () => OnNewBlueprint();

        var closeEditor = root.contentContainer.Q<Button>("Close");
        closeEditor.clicked += () => OnEditorClose();

        // SAVE
        saveDialog = root.Q<VisualElement>("SaveConfirmLayer");

        var saveConfirmButton = saveDialog.Q<Button>("ConfirmSave");
        saveConfirmButton.clicked += () => OnSaveConfirm();

        var saveCancelButton = saveDialog.Q<Button>("CancelSave");
        saveCancelButton.clicked += () => OnSaveCancel();

        saveLabel = root.Q<Label>("SaveConfirmLabel");
        saveNameField = root.Q<TextField>("SaveName");

        // LOAD
        loadDialog = root.Q<VisualElement>("LoadDialogLayer");
        loadTabbedPane = loadDialog.Q<TabbedPane>();

        builtInLoadPane = new ScrollView();
        builtInLoadPane.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        loadTabbedPane.AddTab(builtInLoadPane, "Default");

        customLoadPane = new ScrollView();
        customLoadPane.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        loadTabbedPane.AddTab(customLoadPane, "Custom");

        confirmLoadButton = loadDialog.Q<Button>("ConfirmLoad");
        confirmLoadButton.clicked += () => OnLoadConfirm(); 

        var cancelLoadButton = loadDialog.Q<Button>("CancelLoad");
        cancelLoadButton.clicked += () => OnLoadCancel();

        // TOOLS
        brushButton = root.Q<Button>("Brush");
        brushButton.clicked += () => OnToolChange(Tool.Brush);

        eraiserButton = root.Q<Button>("Eraser");
        eraiserButton.clicked += () => OnToolChange(Tool.Eraser);

        pickerButton = root.Q<Button>("Picker");
        pickerButton.clicked += () => OnToolChange(Tool.Picker);

        fillButton = root.Q<Button>("Fill");
        fillButton.clicked += () => OnToolChange(Tool.Fill);

        moveButton = root.Q<Button>("Move");
        moveButton.clicked += () => OnToolChange(Tool.Move);

        // COLOR PICKER
        colorPicker = root.Q<ColorPicker>();
        colorPicker.colorChangeEvent.AddListener(OnColorChanged);

        var closePicker = colorPicker.Q<Button>("ClosePicker");
        closePicker.clicked += OnPickerClose;

        var deletePaint = colorPicker.Q<Button>("DeleteColor");
        deletePaint.clicked += OnPaintDelete;

        // PAINT
        palette = root.Q<VisualElement>("Palette");

        var newPainButton = palette.Q<Button>("New");
        newPainButton.clicked += OnPaintCreate;

        var paintButton = CreatePaint();
        selectedPaint = paintButton;
        SelectElement(paintButton);
    }

    // UI EVENT HANDLERS
    private void OnToolChange(Tool newTool)
    {
        ChangeTool(newTool);
    }

    private void OnPaintDelete()
    {
        DeletePaint();
        ClosePicker();
    }

    private void OnPickerClose()
    {
        ClosePicker();
    }

    private void OnPaintCreate()
    {
        CreatePaint();
    }

    private void OnColorChanged()
    {
        ChangePaintColor(colorPicker.Color);
    }

    private void OnEditorClose()
    {
        throw new System.NotImplementedException();
    }

    private void OnNewBlueprint()
    {
        throw new System.NotImplementedException();
    }

    private void OnLoad()
    {
        OpenLoadDialog();
    }

    private void OnLoadEntrySelect(ClickEvent evt)
    {
        if (selectedLaodEntry != null)
        {
            DeselectElement(selectedLaodEntry);
        }

        selectedLaodEntry = evt.currentTarget as LoadEntry;
        SelectElement(selectedLaodEntry);
        confirmLoadButton.SetEnabled(true);
    }

    private void OnLoadCancel()
    {
        CloseLoadDialog();
    }

    private void OnLoadConfirm()
    {
        LoadBlueprint();
        CloseLoadDialog();
    }

    private void OnSave()
    {
        OpenSaveConfirmDialog();
    }

    private void OnSaveCancel()
    {
        CloseSaveConfirm();
    }

    private void OnSaveConfirm()
    {
        string folder = "ships";
        if (!IsSaveExits(saveNameField.text, folder) || saveExistsChecked && BlueprintComp.bp.Name == saveNameField.text)
        {
            SaveBlueprint(folder);
            CloseSaveConfirm();
        }
        else //mark as checked
        {
            MarkExistanceForSaveDiaolg();
        }
    }

    private void OnPaintChange(ClickEvent click)
    {
        var paintButton = (PaintButton)click.currentTarget;
        if (selectedPaint == null || selectedPaint != paintButton)
        {
            SelectPaint(paintButton);
        }
        else
        {
            OpenPicker();
        }
    }

    // EDITOR FUNCTIONS
    // leaving them separate from UI HANDLERS for re-usability
    private void ClosePicker()
    {
        if (colorPicker.visible)
        {
            colorPicker.visible = false;
            colorPicker.SetEnabled(false);
        }
    }

    private void DeletePaint()
    {
        if (palette.childCount > 2)
        {
            var index = palette.IndexOf(selectedPaint);
            palette.Remove(selectedPaint);

            SelectPaint((PaintButton)palette.ElementAt(index > 0 ? index - 1 : 0));
        }
    }

    private PaintButton CreatePaint()
    {
        PaintButton paintButton = new();

        palette.hierarchy.Insert(palette.childCount - 1, paintButton);

        // I can't find += combination with ClickEvent :/
        paintButton.RegisterCallback<ClickEvent>(OnPaintChange);
        DeselectElement(paintButton);

        PrepareDeletePaintButton();

        return paintButton;
    }

    private void ChangeTool(Tool newTool)
    {
        selectedTool = newTool;
    }

    private void UpdateToolSelection()
    {
        DeselectElement(eraiserButton);
        DeselectElement(brushButton);
        DeselectElement(pickerButton);
        DeselectElement(fillButton);
        DeselectElement(moveButton);

        switch (tool)
        {
            case Tool.Eraser:
                SelectElement(eraiserButton);
                break;
            case Tool.Brush:
                SelectElement(brushButton);
                break;
            case Tool.Picker:
                SelectElement(pickerButton);
                break;
            case Tool.Fill:
                SelectElement(fillButton);
                break;
            case Tool.Move:
                SelectElement(moveButton);
                break;
        }
    }

    private void SelectElement(VisualElement elem)
    {
        elem.style.borderTopColor = focusColor;
        elem.style.borderBottomColor = focusColor;
        elem.style.borderLeftColor = focusColor;
        elem.style.borderRightColor = focusColor;
    }

    private void DeselectElement(VisualElement elem)
    {
        elem.style.borderTopColor = unfocusColor;
        elem.style.borderBottomColor = unfocusColor;
        elem.style.borderLeftColor = unfocusColor;
        elem.style.borderRightColor = unfocusColor;
    }

    private void OpenPicker()
    {
        colorPicker.Color = selectedPaint.Color;
        colorPicker.visible = true;
        colorPicker.SetEnabled(true);

        PrepareDeletePaintButton();
    }

    private void ChangePaintColor(Color color)
    {
        selectedPaint.Color = color;
    }

    private void SelectPaint(PaintButton newSelection)
    {
        DeselectElement(selectedPaint);
        selectedPaint = newSelection;
        SelectElement(selectedPaint);

        if (colorPicker.visible)
        {
            colorPicker.Color = selectedPaint.Color;
        }
    }

    private void PrepareDeletePaintButton()
    {
        if (!colorPicker.visible)
            return;

        var deleteColorButton = colorPicker.Q<Button>("DeleteColor");
        var enabled = palette.childCount > 2;

        deleteColorButton.SetEnabled(enabled);

        // This is working weird
        //deleteColorButton.visible = enabled;
    }

    private void OpenLoadDialog()
    {
        confirmLoadButton.SetEnabled(false);
        loadDialog.visible = true;
        
        StartCoroutine(LoadFromAddressable("ships"));
        StartCoroutine(LoadFromCustom("ships"));
    }

    private void CloseLoadDialog()
    {
        selectedLaodEntry = null;
        builtInLoadPane.Clear();
        customLoadPane.Clear();
        loadDialog.visible = false;
        Addressables.Release(loadHandle);
    }

    private void LoadBlueprint()
    {
        if (BlueprintComp != null)
        {
            BlueprintComp.SetBlueprint(selectedLaodEntry.blueprint);
        }
    }

    // Function for async loading of addressable assets
    private IEnumerator LoadFromAddressable(string key)
    {
        
        loadHandle = Addressables.LoadAssetsAsync<TextAsset>(
            keys,
            addressable => {
                var loadEntry = CreateLoadEntry(addressable.text);
                builtInLoadPane.Add(loadEntry);
            }, Addressables.MergeMode.Union, 
            false);

        yield return loadHandle;
    }

    private IEnumerator LoadFromCustom(string folder)
    {
        string path = Application.persistentDataPath + "/" + folder;
        Debug.Log("started: " + path);

        if (Directory.Exists(path))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.json"))
            {
                var json = File.ReadAllText(file);
                var entry = CreateLoadEntry(json);
                customLoadPane.Add(entry);
                Debug.Log("Loaded: " + entry.blueprint.Name);
                yield return null;
            }
        }
    }

    public LoadEntry CreateLoadEntry(string json)
    {
        var blueprint = JsonUtility.FromJson<Blueprint>(json);
        var loadEntry = new LoadEntry(blueprint);

        loadEntry.RegisterCallback<ClickEvent>(OnLoadEntrySelect);

        return loadEntry;
    }

    private void SaveBlueprint(string folder)
    {
        BlueprintComp.bp.Name = saveNameField.text;
        if (BlueprintComp.TryGetSaveData(out var json))
        {
            string path = Application.persistentDataPath + "/" + folder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(path + "/" + BlueprintComp.bp.Name + ".json", json);
            Debug.Log("Saved to: " + path + "/" + BlueprintComp.bp.Name + ".json");
        }
    }

    private void CloseSaveConfirm()
    {
        saveDialog.visible = false;
    }

    private void OpenSaveConfirmDialog()
    {
        saveExistsChecked = false;
        saveLabel.text = "Save changes to this blueprint?";
        saveLabel.style.color = Color.white;
        saveNameField.SetValueWithoutNotify(BlueprintComp.bp.Name);
        saveDialog.visible = true;
    }

    private void MarkExistanceForSaveDiaolg()
    {
        BlueprintComp.bp.Name = saveNameField.text;
        saveLabel.text = BlueprintComp.bp.Name + " Exists! Overwrite?";
        saveLabel.style.color = Color.red;
        saveExistsChecked = true;
    }

    private bool pressed;
    // Update is called once per frame
    void Update()
    {
        Tool newTool = selectedTool;

        if (eraserQuickShortcut.inProgress)
        {
            newTool = Tool.Eraser;
        }
        else if (pickerQuickShortcut.inProgress)
        {
            newTool = Tool.Picker;
        }
        else if (fillQuickShortcut.inProgress)
        {
            newTool = Tool.Fill;
        }
        else if (moveQuickShortcut.inProgress)
        {
            newTool = Tool.Move;
        }

        if (newTool != tool)
        {
            tool = newTool;
            UpdateToolSelection();
        }

        if (!pressed && clickInput.WasPressedThisFrame() && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Pressed");
            pressed = true;

            if (tool == Tool.Move)
            {
                cameraControlls.DragStarted();
            }
        }

        if (pressed)
        {
            pressed = false; // DRAG DISABLE BY DEFAULT

            if (clickInput.IsPressed())
            {
                var mousePos = Pointer.current.position;
                var cam = Camera.main;
                var point = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue(), -cam.transform.position.z));
                var bpPoint = BlueprintComp.WorldPointToBlueprint(point);

                // PIXEL CHANGING
                if (true) // TODO
                {
                    switch (tool)
                    {
                        case Tool.Eraser:
                            BlueprintComp.RemoveCell(bpPoint.x, bpPoint.y);
                            pressed = true; // DRAG ENABLED
                            break;
                        case Tool.Picker:
                            if (BlueprintComp.TryGetCell(bpPoint.x, bpPoint.y, out var cell))
                            {
                                selectedPaint.Color = cell.Color;
                                if (colorPicker.visible)
                                    colorPicker.Color = selectedPaint.Color;
                            }
                            break;
                        case Tool.Brush:
                            BlueprintComp.SetCell(bpPoint.x, bpPoint.y, selectedPaint.Color);
                            pressed = true; // DRAG ENABLED
                            break;
                        case Tool.Fill:
                            BlueprintComp.FillCells(bpPoint.x, bpPoint.y, selectedPaint.Color);
                            break;
                    }
                }
                else // INSTALLING MODULE
                {

                }
            }
            else
            {
                
            }
        }

        if (clickInput.WasReleasedThisFrame())
        {
            Debug.Log("Released");
            cameraControlls.DragEnd();
        }
    }

    private bool IsSaveExits(string saveName, string folder)
    {
        string path = Application.persistentDataPath + "/" + folder;
        if (!Directory.Exists(path))
            return false;

        return File.Exists(path + "/" + saveName + ".json");
    }
}
