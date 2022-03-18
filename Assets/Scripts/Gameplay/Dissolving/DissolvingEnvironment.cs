using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingEnvironment : MonoBehaviour
{
    public GameObject parentObject; // The parent object of environment objects. Used to get the environment
    public GameObject lightParent; // The parent object of all light objects
    private List <GameObject> lightObjects = new List<GameObject>(); // Keep track of all gameobjects that bring light to the map
    private List <GameObject> nearbyEnvironment = new List<GameObject>(); // List that contains all environment objects. Used to simply track those closest to the player
    private Vector3 initialScale; // Required for distance size change calculation. Registers the inital localScale
    private Vector3 wallScale; // Get initial localscale for wall objects
    private float scaleMultiplier = 2.5f; // Used in the distance size change calculation.

    // Start is called before the first frame update
    void Start()
    {
        // For each environment object within the parent object
        foreach (Transform children in parentObject.transform)
        {
            if (children.tag == "Floor")
            {
                initialScale = children.transform.localScale; // Take a note of the set localScale of the environment tiles
            }
            else if (children.tag == "Wall")
            {
                wallScale = children.transform.localScale;
            }
            
            nearbyEnvironment.Add (children.gameObject); // Add environment objects to the list
        }

        // For each light object within the light parent
        foreach (Transform child in lightParent.transform)
        {
            lightObjects.Add (child.gameObject); // Add to list of light objects
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Run through every environment object in list
        for (int i = 0; i < nearbyEnvironment.Count; i++)
        {
            // Establish the transform of the current environment object in the list
            Transform currentEnvironmentObject = nearbyEnvironment [i].transform;

            // Calculate the distance of Player from environment objects
            float distanceFromEnvironment = Vector3.Distance (transform.position, currentEnvironmentObject.position);
            
            // Decide what happens based on the distance from the environment object
            if (distanceFromEnvironment < 4.0f)
            {
                currentEnvironmentObject.gameObject.SetActive(true); // Activate object when required

                if (currentEnvironmentObject.tag == "Floor")
                {
                    currentEnvironmentObject.localScale = initialScale; // Set default localScale
                    currentEnvironmentObject.localPosition = new Vector3 (currentEnvironmentObject.localPosition.x, 0.0f, currentEnvironmentObject.localPosition.z); // Set default localPosition
                }
                else if (currentEnvironmentObject.tag == "Wall")
                {
                    currentEnvironmentObject.localScale = wallScale;
                }
            }
            else if (distanceFromEnvironment <= 12.5f)
            { 
                currentEnvironmentObject.gameObject.SetActive(true); // Activate object when required

                if (currentEnvironmentObject.tag == "Floor")
                {
                    currentEnvironmentObject.localScale = initialScale / distanceFromEnvironment * scaleMultiplier; // Set scale to vary based on distance. Below sets the height based on distance
                    currentEnvironmentObject.localPosition = new Vector3 (currentEnvironmentObject.localPosition.x, distanceFromEnvironment - 4.75f, currentEnvironmentObject.localPosition.z);
                }
                else if (currentEnvironmentObject.tag == "Wall")
                {
                    currentEnvironmentObject.localScale = wallScale / distanceFromEnvironment * scaleMultiplier;
                }
            }
            else if (distanceFromEnvironment > 12.5f)
            {
                currentEnvironmentObject.gameObject.SetActive(false); // If out of range then deactivate the object to save memory
            }

            // Run through every light object in list
            for (int o = 0; o < lightObjects.Count; o++)
            {
                // Establish the transform of the current light object in the list
                Transform currentLightObject = lightObjects [o].transform;

                // Calculate the distance of light object from environment objects
                float distance = Vector3.Distance (currentLightObject.position, currentEnvironmentObject.position);

                if (distance < 4.0f)
                {
                    currentEnvironmentObject.gameObject.SetActive(true); // Activate object when required within light

                    // Hard code scale and position to stop local changes
                    currentEnvironmentObject.localScale = new Vector3 (2.0f, 1.0f, 2.0f); // Set default localScale
                    currentEnvironmentObject.localPosition = new Vector3 (currentEnvironmentObject.localPosition.x, 0.0f, currentEnvironmentObject.localPosition.z); // Set default localPosition

                    // Stop lit environment objects from being tracked by player movement
                    nearbyEnvironment.Remove (currentEnvironmentObject.gameObject); 
                    currentEnvironmentObject.transform.parent = null;
                }
            }
        }
    }

    // On exiting test runs or end of game
    private void OnDisable()
    {
        nearbyEnvironment.Clear();
    }

    // In editor, view more information
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere (transform.position, 4.0f);
    }
}
