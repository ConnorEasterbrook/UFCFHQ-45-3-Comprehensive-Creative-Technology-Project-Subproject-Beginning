using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerSphericalMask : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector4 playerPos = new Vector4(player.transform.position.x, player.transform.position.y, player.transform.position.z, 0);
        Shader.SetGlobalVector("_PlayerPos", playerPos);
    }
}
