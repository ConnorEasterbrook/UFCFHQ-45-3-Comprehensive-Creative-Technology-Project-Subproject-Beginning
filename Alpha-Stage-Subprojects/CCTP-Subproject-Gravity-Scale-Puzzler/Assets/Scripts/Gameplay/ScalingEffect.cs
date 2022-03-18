using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingEffect : MonoBehaviour
{
    private Transform targetObject; // The transform for the target interactible object

    // INSPECTOR VARIABLES
    [Tooltip ("The layer of the interactible objects.")]
    public LayerMask interactibleLayer;
    [Tooltip ("What layers to ignore but still use for the scaling effect.")]
    public LayerMask ignoreLayer;

    [Tooltip ("The offset amount for targetObject to avoid clipping.")]
    [Range (0.5f, 5.0f)] public float offsetFactor;

    // CALCULATION VARIABLES
    private float initialDistance; // The initial distance between the transform object and the target object
    private float initialScale; // The initial scale of the target object
    private Vector3 intendedScale; // The scale we intend to set the targetObject to
 
    void Update()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            UpdateInput();
        }

        if (targetObject != null) UpdateScaling();
    }
 
    void UpdateInput()
    {
        if (targetObject == null)
        {
            // Raycast onto interactible layer
            RaycastHit grabbedObject;

            // If raycast hits then get information for scaling effect
            if (Physics.Raycast (transform.position, transform.forward, out grabbedObject, Mathf.Infinity, interactibleLayer))
            {
                // Set target object to grabbed object and disable its physics
                targetObject = grabbedObject.transform;
                targetObject.GetComponent<Rigidbody>().isKinematic = true;

                // Calculate the distance between the camera and the object
                initialDistance = Vector3.Distance (transform.position, targetObject.position);

                // Save initial local scale and set intended scale as placeholder
                initialScale = targetObject.localScale.x; // Local scale as object may have a parent
                intendedScale = targetObject.localScale;
            }
        }
        else
        {
            // Re-enable physics for the target (grabbed) object
            targetObject.GetComponent<Rigidbody>().isKinematic = false;

            // Reset target object
            targetObject = null;
        }
        
    }
 
    void UpdateScaling()
    {
        // Cast a ray forward from the camera position, ignore the layer that is used to acquire targets
        // so we don't hit the attached target with our ray
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ignoreLayer))
        {
            // Set the new position of the target by getting the hit point and moving it back a bit
            // depending on the scale and offset factor
            targetObject.position = hit.point - transform.forward * offsetFactor * intendedScale.x;
 
            // Calculate the current distance between the camera and the target object
            float currentDistance = Vector3.Distance(transform.position, targetObject.position);
 
            // Calculate the ratio between the current distance and the original distance
            float s = currentDistance / initialDistance;
 
            // Set the scale Vector3 variable to be the ratio of the distances
            intendedScale.x = intendedScale.y = intendedScale.z = s;
 
            // Set the scale for the target objectm, multiplied by the original scale
            targetObject.localScale = intendedScale * initialScale;
        }
    }
}
