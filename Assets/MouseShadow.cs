using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var mousePos = Input.mousePosition;
        var cam = Camera.main;
        var point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));

        Material.SetVector("_Collision", point);
    }
}
