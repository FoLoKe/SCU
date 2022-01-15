using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingOverlay : MonoBehaviour
{
    public BlueprintComponent Blueprint;
    public SpriteRenderer Renderer;

    public Texture2D _texture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Damping")) 
        {
            updateOverlay(BlueprintComponent.Module.Engine);
        }
    }

    public static Color32 s_transparent = new Color32();
    public static Color32 s_blocked = new Color32(255, 0, 0, 128);

    public bool[,] mask;

    public void updateOverlay(BlueprintComponent.Module module) 
    {
        mask = new bool[Blueprint.sizeX, Blueprint.sizeY];
        
        _texture = new Texture2D(Blueprint.sizeX, Blueprint.sizeY);
        _texture.filterMode = FilterMode.Point;
        var lockBorder = false;
        var lockInside = false;
        var lockCorners = false;

        switch (module)
        {
            case BlueprintComponent.Module.Cargo:
                lockBorder = true;
                break;
            case BlueprintComponent.Module.Engine:
                lockInside = true;
                lockCorners = true;
                break;
        }

        for (int x = 0; x < Blueprint.sizeX; x++)
        {
            for (int y = 0; y < Blueprint.sizeY; y++) 
            {
                BlueprintComponent.Cell cell = Blueprint.Cells[x, y];
                if (cell != null)
                {
                    if (lockBorder && cell.state != 15 
                        || lockInside && cell.state == 15
                        || lockCorners && (cell.state == 9 || cell.state == 3 || cell.state == 12 || cell.state == 6)) 
                    {
                        _texture.SetPixel(x, y, s_blocked);
                        continue;
                    } 
                        
                    _texture.SetPixel(x, y, s_transparent);
                    
                } 
                else 
                {
                    _texture.SetPixel(x, y, s_transparent);
                }
            }
        }

        _texture.Apply();

        Renderer.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
    }
}
