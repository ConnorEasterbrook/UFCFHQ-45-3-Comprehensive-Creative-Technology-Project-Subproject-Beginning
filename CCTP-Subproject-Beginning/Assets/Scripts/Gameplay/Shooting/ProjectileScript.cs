using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private GameObject planetGameObject;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent <Rigidbody>();
        
        if (GameObject.FindGameObjectWithTag ("Planet") != null)
        {
            planetGameObject = GameObject.FindGameObjectWithTag ("Planet");
        }

        player = GameObject.FindGameObjectWithTag ("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // Set the gravity amount
        float gravity = 1.25f;

        // If there is a planet object then use spherical shooting
        if (planetGameObject != null)
        {
            // Get projectile bearings
            Vector3 gravityUp = (transform.position - planetGameObject.transform.position).normalized;
            Vector3 localUp = transform.up;

            // Apply a gravitational force
            _rigidbody.AddForce (-gravityUp * gravity);

            // Rotate projectile to keep gravity consistent when travelling around the planet
            Quaternion targetRotation = Quaternion.FromToRotation (localUp, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, 50.0f * Time.deltaTime);
        }
        else
        {
            // Basic gravity if no planet
            _rigidbody.AddForce (-transform.up * _rigidbody.mass * gravity);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag != "Player")
        {
            if (other.tag == "Target")
            {
                other.gameObject.SetActive (false);
            }

            Destroy (gameObject);
        }
    }
}
