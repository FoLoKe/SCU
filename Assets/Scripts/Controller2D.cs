using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    private float _thrustForce = 30;
    private float _dampingForce = 10;

    private float _leftForce = 10;
    private float _rightForce = 5;

    private Vector2 _linearForce = new Vector2();
    private float _torque = 0;
    private bool _damping = true;

    private void Awake()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
	}

    private void FixedUpdate() 
    {
        if (_linearForce != Vector2.zero) 
        {
            _rigidbody2D.AddForce(_linearForce);
        } 
        else if (_damping && _rigidbody2D.velocity.magnitude != 0) 
        {
            var dampingDirection = -1f * _rigidbody2D.velocity.normalized;
            var up = Vector2.Dot(transform.up, _rigidbody2D.velocity);
            var right = Vector2.Dot(transform.right, _rigidbody2D.velocity);

            var dampingVector = Vector2.ClampMagnitude(new Vector2(right, up), 1.0f);

            dampingVector.y *= dampingVector.y > 0 ? _dampingForce : -_thrustForce;
            dampingVector.x *= dampingVector.x > 0 ? _rightForce : -_leftForce;

            if (_rigidbody2D.velocity.magnitude < dampingVector.magnitude / _rigidbody2D.mass * Time.fixedDeltaTime) {
                _rigidbody2D.velocity = Vector2.zero;
            } 
            else 
            {
                _rigidbody2D.AddForce(dampingVector.magnitude * dampingDirection);
            }
        }

        if (_torque != 0) 
        {
            _rigidbody2D.AddTorque(_torque);
        } 
        else if (_damping && _rigidbody2D.angularVelocity != 0) 
        {
            if (_rigidbody2D.angularVelocity > _leftForce) // This is not right to compare degrese per second with force, but works okay for now
            {
                _rigidbody2D.AddTorque(-_leftForce);
            } 
            else if (_rigidbody2D.angularVelocity < -_rightForce)
            {
                _rigidbody2D.AddTorque(_rightForce);
            } 
            else 
            {
                _rigidbody2D.angularVelocity = 0;
            }
        }
    }

    public void Thrust(float thrust) 
    {
        if (thrust > 0) 
        {
            _linearForce = _thrustForce * thrust * transform.up;
        } 
        else if (thrust < 0) 
        {
            _linearForce = _dampingForce * thrust * transform.up;
        } 
        else 
        {
            _linearForce = Vector2.zero;
        }
    }

    public void Rotate(float rotation) 
    {
        if (rotation > 0) {
            _torque = -_rightForce * rotation;
        } 
        else if (rotation < 0) 
        {
            _torque = -_leftForce * rotation;
        } 
        else 
        {
            _torque = 0;
        }
    }

    public void ToggleDamping() 
    {
        _damping = !_damping;
    }
}
