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

    //private Blueprint _module;

    // TOOLS
    private Button brushButton;
    private Button eraiserButton;
    private Button pickerButton;

    private enum Tool { Brush, Eraser, Picker, None }; //Editor tools types
    private Tool tool = Tool.None; //Tool to use on left click
    private Tool selectedTool = Tool.Brush; //Tool selected on the left panel

    // PALETTE
    private ColorPicker colorPicker;
    private VisualElement palette;

    private PaintButton selectedPaint; //Paint selected for the brush

    // SAVE DIALOG
    private VisualElement saveConfirm;
    private Label saveLabel;
    private TextField saveNameField;

    private bool saveExistsChecked;

    // INPUT
    private InputAction brushShortcut;
    private InputAction pickerShortcut;
    private InputAction eraserShortcut;

    private InputAction pickerQuickShortcut;
    private InputAction eraserQuickShortcut;

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

        pickerQuickShortcut = PlayerInput.actions["QuickPicker"];

        eraserQuickShortcut = PlayerInput.actions["QuickEraser"];

        clickInput = PlayerInput.actions["Click"];

        StartCoroutine(LoadFromAddressable("ships"));
    }

    private void OnEnable() 
    {
        var root = UIDoc.rootVisualElement;

        // CONTROLS 
        var saveBlueprint = root.contentContainer.Q<Button>("SaveBP");
        saveBlueprint.clicked += () => OnSaveBlueprint();

        var loadBlueprint = root.contentContainer.Q<Button>("LoadBP");
        loadBlueprint.clicked += () => OnLoadBlueprint();

        var newBlueprint = root.contentContainer.Q<Button>("NewBP");
        newBlueprint.clicked += () => OnNewBlueprint();

        var closeEditor = root.contentContainer.Q<Button>("Close");
        closeEditor.clicked += () => OnCloseEditor();

        // SAVE
        saveConfirm = root.Q<VisualElement>("SaveConfirmLayer");

        var saveConfirmButton = saveConfirm.Q<Button>("ConfirmSave");
        saveConfirmButton.clicked += () => OnSaveConfirm();

        var saveCancelButton = saveConfirm.Q<Button>("CancelSave");
        saveCancelButton.clicked += () => OnSaveCancel();

        saveLabel = root.Q<Label>("SaveConfirmLabel");
        saveNameField = root.Q<TextField>("SaveName");

        // TOOLS
        brushButton = root.Q<Button>("Brush");
        brushButton.clicked += () => OnToolChange(Tool.Brush);

        eraiserButton = root.Q<Button>("Eraser");
        eraiserButton.clicked += () => OnToolChange(Tool.Eraser);

        pickerButton = root.Q<Button>("Picker");
        pickerButton.clicked += () => OnToolChange(Tool.Picker);

        // COLOR PICKER
        colorPicker = root.Q<ColorPicker>();
        colorPicker.colorChangeEvent.AddListener(OnColorChanged);

        var closePicker = colorPicker.Q<Button>("ClosePicker");
        closePicker.clicked += OnClosePicker;

        var deletePaint = colorPicker.Q<Button>("DeleteColor");
        deletePaint.clicked += OnDeleteColor;

        // PAINT
        palette = root.Q<VisualElement>("Palette");

        var newPainButton = palette.Q<Button>("New");
        newPainButton.clicked += OnCreateNewPaint;

        var paintButton = CreateNewPaint();
        selectedPaint = paintButton;
        SelectButton(paintButton);
    }

    // UI EVENT HANDLERS
    private void OnToolChange(Tool newTool)
    {
        ChangeTool(newTool);
    }

    private void OnDeleteColor()
    {
        DeletePaint();
        ClosePicker();
    }

    private void OnClosePicker()
    {
        ClosePicker();
    }

    private void OnCreateNewPaint()
    {
        CreateNewPaint();
    }

    private void OnColorChanged()
    {
        ChangePaintColor(colorPicker.Color);
    }

    private void OnCloseEditor()
    {
        throw new System.NotImplementedException();
    }

    private void OnNewBlueprint()
    {
        throw new System.NotImplementedException();
    }

    private void OnLoadBlueprint()
    {
        throw new System.NotImplementedException();
    }

    private void OnSaveBlueprint()
    {
        OpenSaveConfirmDialog();
    }

    private void OnSaveCancel()
    {
        CloseSaveConfirm();
    }

    private void OnSaveConfirm()
    {
        if (!IsSaveExits(saveNameField.text) || saveExistsChecked && BlueprintComp.bp.Name == saveNameField.text)
        {
            SaveBlueprint();
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

    private PaintButton CreateNewPaint()
    {
        PaintButton paintButton = new();

        palette.hierarchy.Insert(palette.childCount - 1, paintButton);

        // I can't find += combination with ClickEvent :/
        paintButton.RegisterCallback<ClickEvent>(OnPaintChange);
        DeselectButton(paintButton);

        PrepareDeletePaintButton();

        return paintButton;
    }

    private void ChangeTool(Tool newTool)
    {
        selectedTool = newTool;
    }

    private void UpdateToolSelection()
    {
        DeselectButton(eraiserButton);
        DeselectButton(brushButton);
        DeselectButton(pickerButton);

        switch (tool)
        {
            case Tool.Eraser:
                SelectButton(eraiserButton);
                break;
            case Tool.Brush:
                SelectButton(brushButton);
                break;
            case Tool.Picker:
                SelectButton(pickerButton);
                break;
        }
    }

    private void SelectButton(Button button)
    {
        button.style.borderTopColor = focusColor;
        button.style.borderBottomColor = focusColor;
        button.style.borderLeftColor = focusColor;
        button.style.borderRightColor = focusColor;
    }

    private void DeselectButton(Button button)
    {
        button.style.borderTopColor = unfocusColor;
        button.style.borderBottomColor = unfocusColor;
        button.style.borderLeftColor = unfocusColor;
        button.style.borderRightColor = unfocusColor;
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
        DeselectButton(selectedPaint);
        selectedPaint = newSelection;
        SelectButton(selectedPaint);

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

    private IEnumerator LoadFromAddressable(string key)
    {
        loadHandle = Addressables.LoadAssetsAsync<TextAsset>(
            keys,
            addressable => {
                Debug.Log(addressable.text);
            }, Addressables.MergeMode.Union, 
            false);

        yield return loadHandle;
    }

    private void SaveBlueprint()
    {
        BlueprintComp.bp.Name = saveNameField.text;
        if (BlueprintComp.TryGetSaveData(out var json))
        {
            File.WriteAllText(Application.persistentDataPath + "/" + BlueprintComp.bp.Name + ".json", json);
            Debug.Log("Saved to: " + Application.persistentDataPath + "/" + BlueprintComp.bp.Name + ".json");
        }
    }

    private void CloseSaveConfirm()
    {
        saveConfirm.visible = false;
    }

    private void OpenSaveConfirmDialog()
    {
        saveExistsChecked = false;
        saveLabel.text = "Save changes to this blueprint?";
        saveLabel.style.color = Color.white;
        saveNameField.SetValueWithoutNotify(BlueprintComp.bp.Name);
        saveConfirm.visible = true;
    }

    private void MarkExistanceForSaveDiaolg()
    {
        BlueprintComp.bp.Name = saveNameField.text;
        saveLabel.text = BlueprintComp.bp.Name + " Exists! Overwrite?";
        saveLabel.style.color = Color.red;
        saveExistsChecked = true;
    }

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

        if (newTool != tool)
        {
            tool = newTool;
            UpdateToolSelection();
        }

        if (clickInput.WasPerformedThisFrame() && !EventSystem.current.IsPointerOverGameObject())
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
                        break;
                }
            }
            else // INSTALLING MODULE
            {

            }
        }
    }

    private bool IsSaveExits(string saveName)
    {
        return File.Exists(Application.persistentDataPath + "/" + saveName + ".json");
    }
}
