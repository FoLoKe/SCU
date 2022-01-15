using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float _thrust;
    private float _rotation;
    private Controller2D _controller;

    // Start is called before the first frame update
    void Awake()
    {
        _controller = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _thrust = Input.GetAxisRaw("Vertical");
        _rotation = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Damping")) 
        {
            _controller.ToggleDamping();
        }
    }

    private void FixedUpdate() 
    {
        _controller.Thrust(_thrust);
        _controller.Rotate(_rotation);
    }
}
