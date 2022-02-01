using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private float _thrust;
    private float _rotation;
    private Controller2D _controller;

    public PlayerInput playerInput;

    private InputAction move;
    private InputAction damping;

    // Start is called before the first frame update
    void Awake()
    {
        _controller = GetComponent<Controller2D>();

        move = playerInput.actions["Move"];
        damping = playerInput.actions["Damping"];

        damping.performed += _ => OnToggleDamping(); 
    }

    private void OnToggleDamping()
    {
        _controller.ToggleDamping();
    }

    // Update is called once per frame
    void Update()
    {
        _thrust = move.ReadValue<Vector2>().y;
        _rotation = move.ReadValue<Vector2>().x;
    }

    private void FixedUpdate() 
    {
        _controller.Thrust(_thrust);
        _controller.Rotate(_rotation);
    }
}
