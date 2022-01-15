using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;

public class ShipEditor : MonoBehaviour
{
    public BlueprintComponent Blueprint;
    public UnityEngine.UIElements.UIDocument EditorUIDoc;

    private BlueprintComponent _module;

    private bool _eraiser;

    private ColorPicker colorPicker;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable() {
        //var rootVisualElement = EditorUIDoc.rootVisualElement;
       //colorPicker = rootVisualElement.Q<ColorPicker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            var cam = Camera.main;
            var point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));
            var bpPoint = Blueprint.WorldPointToBlueprint(point);

            // PIXEL CHANGING
            if (_module == null)
            {
                if (Input.GetKey(KeyCode.LeftAlt)) // PICK COLOR
                {
                    if (Blueprint.TryGetCell(bpPoint.x, bpPoint.y, out var cell))
                    {
                        //TODO:
                        //colorPicker.color = cell.Color;
                    }
                }
                else // ADD OR REMOVE
                {
                    if (_eraiser)
                    {
                        Blueprint.RemoveCell(bpPoint.x, bpPoint.y);
                    }
                    else
                    {
                        //TODO:
                        //Blueprint.SetCell(bpPoint.x, bpPoint.y, colorPicker.color);
                    }
                }
            }
            else // INSTALLING MODULE
            {

            }
        }
    }

    
}
