using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AngleToSphere : MonoBehaviour
{
    public GameObject planetGameObject;
    public bool lockY;
    public bool SphereMove;

    private float yDistance;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt (planetGameObject.transform);
        transform.Rotate (-90, 0, 0);

        if (SphereMove)
        {
            transform.position = (transform.position - planetGameObject.transform.position).normalized * yDistance + planetGameObject.transform.position;
        }

        if (!lockY)
        {
            yDistance = Vector3.Distance (transform.position, planetGameObject.transform.position);
        }
    }
}
