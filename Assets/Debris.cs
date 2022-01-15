using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    public Vector3 Inertia = new Vector2(0, 0);
    public float Torgue = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate() 
    {
        transform.position += Inertia;
        transform.Rotate(0, Torgue, 0);
    }
}
