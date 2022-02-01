using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlls : MonoBehaviour
{
    public PlayerInput PlayerInput;

    private InputAction rightClickInput;
    private InputAction zoomInput;

    private Vector2 _initialPos;
    private bool _isDragging;

    public float MinZoom = 0.25f;
    public float MaxZoom = 3f;
    public float MaxOffset = 10;

    private float zoom = 1;

    // Start is called before the first frame update
    void Start()
    {
        rightClickInput = PlayerInput.actions["RightClick"];
        zoomInput = PlayerInput.actions["zoom"];
    }

    // Update is called once per frame
    void Update()
    {
        if (rightClickInput.WasPressedThisFrame())
        {
            _isDragging = true;
            var pos = Pointer.current.position;
            _initialPos.Set(pos.x.ReadValue(), pos.y.ReadValue());
        }

        if (rightClickInput.WasReleasedThisFrame())
        {
            _isDragging = false;
        }

        
        
        zoom = zoomInput.ReadValue<float>() * 0.005f;
    }

    private void FixedUpdate() {
        Debug.Log(Mouse.current.scroll.ReadValue().y);
        var position = transform.localPosition;

        if (_isDragging)
        {
            var pos = Pointer.current.position;
            var dragPos = new Vector2(pos.x.ReadValue(), pos.y.ReadValue());

            var offset = _initialPos - dragPos;
            _initialPos = dragPos;

            var cam = Camera.main;
            //var worldOffset = cam.ScreenToWorldPoint(new Vector3(offset.x, offset.y, -cam.transform.position.z));
            position.x = Mathf.Clamp(position.x + offset.x / 1000f * -position.z, -MaxOffset, MaxOffset);
            position.y = Mathf.Clamp(position.y + offset.y / 1000f * -position.z, -MaxOffset, MaxOffset);
        }

        position.z = Mathf.Clamp(position.z + zoom * 0.25f, -MaxZoom, -MinZoom);

        transform.localPosition = position;
    }
}
