using UnityEngine;
using Game.UI;
using UnityEngine.UIElements;
using Assets.BlueprintUtils;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ShipEditor : MonoBehaviour
{
    public BlueprintComponent Blueprint;
    public UIDocument UIDoc;
    public PlayerInput PlayerInput;

    //private Blueprint _module;

    private ColorPicker colorPicker;
    private Button brushButton;
    private Button eraiserButton;
    private Button pickerButton;
    private VisualElement palette;

    private enum Tool {Brush, Eraser, Picker, None}; //Editor tools types

    private Tool tool = Tool.None; //Tool to use on left click
    private Tool selectedTool = Tool.Brush; //Tool selected on the left panel

    private PaintButton selectedPaint; //Paint selected for the brush

    private InputAction brushShortcut;
    private InputAction pickerShortcut;
    private InputAction eraserShortcut;

    private InputAction pickerQuickShortcut;
    private InputAction eraserQuickShortcut;

    private InputAction clickInput;

    private Color focusColor = Color.white;
    private Color unfocusColor = new(0, 0, 0, 0);

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
    }

    private void OnEnable() 
    {
        var root = UIDoc.rootVisualElement;
        
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

    // EVENT HANDLERS
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

    // UI Functions
    // leaving them separate from handlers for re-usability
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
            var bpPoint = Blueprint.WorldPointToBlueprint(point);

            // PIXEL CHANGING
            if (true) // TODO
            {
                switch (tool)
                {
                    case Tool.Eraser:
                        Blueprint.RemoveCell(bpPoint.x, bpPoint.y);
                        break;
                    case Tool.Picker:
                        if (Blueprint.TryGetCell(bpPoint.x, bpPoint.y, out var cell))
                        {
                            selectedPaint.Color = cell.Color;
                            if (colorPicker.visible)
                                colorPicker.Color = selectedPaint.Color;
                        }
                        break;
                    case Tool.Brush:
                        Blueprint.SetCell(bpPoint.x, bpPoint.y, selectedPaint.Color);
                        break;
                }
            }
            else // INSTALLING MODULE
            {

            }
        }
    }
}
