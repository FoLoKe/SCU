using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class MouseShadow : MonoBehaviour
{
    public Material Material;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        var pos = Pointer.current.position;
        var mousePos = new Vector2(pos.x.ReadValue(), pos.y.ReadValue());

        var cam = Camera.main;
        var point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));

        Material.SetVector("_Collision", point);
    }
}
