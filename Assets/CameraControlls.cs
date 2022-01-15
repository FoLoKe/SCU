using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlls : MonoBehaviour
{
    private Vector3 _initialPos;
    private bool _isDragging;

    public float MinZoom = 0.25f;
    public float MaxZoom = 3f;
    public float MaxOffset = 10; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isDragging = true;
            _initialPos = Input.mousePosition;
        } 

        if (Input.GetMouseButtonUp(1)) 
        {
            _isDragging = false;
        }
    }

    private void FixedUpdate() {
        var position = transform.localPosition;

        if (_isDragging)
        {
            var dragPos = Input.mousePosition;

            var offset = _initialPos - dragPos;
            _initialPos = dragPos;

            var cam = Camera.main;
            //var worldOffset = cam.ScreenToWorldPoint(new Vector3(offset.x, offset.y, -cam.transform.position.z));
            position.x = Mathf.Clamp(position.x + offset.x / 1000f * -position.z, -MaxOffset, MaxOffset);
            position.y = Mathf.Clamp(position.y + offset.y / 1000f * -position.z, -MaxOffset, MaxOffset);
        }

        var zoom = Input.mouseScrollDelta.y * 0.5f;
        position.z = Mathf.Clamp(position.z + zoom * 0.25f, -MaxZoom, -MinZoom);

        transform.localPosition = position;
    }
}
