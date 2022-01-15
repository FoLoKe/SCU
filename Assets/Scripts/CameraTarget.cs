using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField]
    public Rigidbody2D Rigidbody2D;

    private void FixedUpdate() 
    {
        transform.position = Rigidbody2D.worldCenterOfMass;
    }
}
