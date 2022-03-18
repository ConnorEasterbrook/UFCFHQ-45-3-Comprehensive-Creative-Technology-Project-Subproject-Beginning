using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowSpirit : MonoBehaviour
{
    public GameObject player;
    private float floatSpeed = 8.0f;
    private bool shooting = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, 
        new Vector3 (player.transform.position.x + 0.75f, player.transform.position.y + 1.0f, player.transform.position.z + 0.75f), Time.deltaTime * floatSpeed);
    }
}
