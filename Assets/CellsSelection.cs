using Assets.BlueprintUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsSelection : MonoBehaviour
{
    static Texture2D _whiteTexture;

    public BlueprintComponent bp;
    public BlueprintOverlay bpo;

    public static Texture2D WhiteTexture
    {
        get
        {
            if( _whiteTexture == null )
            {
                _whiteTexture = new Texture2D( 1, 1 );
                _whiteTexture.SetPixel( 0, 0, Color.white );
                _whiteTexture.Apply();
            }
 
            return _whiteTexture;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake() {

    }

    public static void DrawScreenRect( Rect rect, Color color )
    {
        GUI.color = color;
        GUI.DrawTexture( rect, WhiteTexture );
        GUI.color = Color.white;
    }
 
    public static void DrawScreenRectBorder( Rect rect, float thickness, Color color )
    {
        // Top
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
        // Left
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
        // Right
        DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
        // Bottom
        DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    private static Vector3 CameraToWorld(Vector3 mousePos) 
    {
        Camera cam = Camera.main;
        Vector3 point = new Vector3();

        point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));

        return point;
    }

    bool isSelecting = false;
    bool isAltSelecting = false;
    Vector3 mousePosition1;
 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        } 
        else if (!isSelecting && Input.GetMouseButtonDown(1))
        {
            isAltSelecting = true;
            mousePosition1 = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            if (isSelecting) 
            {
                bp.SelectRange(CameraToWorld(mousePosition1), CameraToWorld(Input.mousePosition), BlueprintComponent.Module.Cargo);

                bpo.UpdateOverlay();
            }

            isSelecting = false;
        } 
        else if (!isSelecting && Input.GetMouseButtonUp(1))
        {
            if (isAltSelecting)
            {
                bp.SelectRange(CameraToWorld(mousePosition1), CameraToWorld(Input.mousePosition), BlueprintComponent.Module.None);

                bpo.UpdateOverlay();
            }

            isAltSelecting = false;
        }
    }
 
    void OnGUI()
    {
        if( isSelecting || isAltSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect( mousePosition1, Input.mousePosition );
            DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
        }
    }

}
