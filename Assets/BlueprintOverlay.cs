using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintOverlay : MonoBehaviour
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
        
    }

    Dictionary<BlueprintComponent.Module, Color32> colors = new Dictionary<BlueprintComponent.Module, Color32> {
        {BlueprintComponent.Module.Cargo, new Color32(255, 255, 0, 128)},
        {BlueprintComponent.Module.None, new Color32(0, 0, 0, 0)}
    };

    public static Color32 s_transparent = new Color32();

    public void UpdateOverlay() {
        _texture = new Texture2D(Blueprint.sizeX, Blueprint.sizeY);
        _texture.filterMode = FilterMode.Point;

        for (int x = 0; x < Blueprint.sizeX; x++)
        {
            for (int y = 0; y < Blueprint.sizeY; y++) 
            {
                BlueprintComponent.Cell cell = Blueprint.Cells[x, y];
                if (cell != null 
                    && cell.module != BlueprintComponent.Module.None
                    && colors.TryGetValue(cell.module, out var color)) 
                {
                    _texture.SetPixel(x, y, color);
                } else {
                    colors.TryGetValue(BlueprintComponent.Module.None, out color);
                    _texture.SetPixel(x, y, color);
                }
            }
        }

        _texture.Apply();

        Renderer.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
    }
}
