using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innerSphereMaterial : MonoBehaviour
{
    public PlayerController playerController;

    private Renderer innerSphereRenderer; 
    public Material innerSphereMat;
    public Material standardMat;

    // Start is called before the first frame update
    void Start()
    {
        innerSphereRenderer = GetComponent <Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.insideSphere)
        {
            innerSphereRenderer.material = innerSphereMat;
        }
        else
        {
            innerSphereRenderer.material = standardMat;
        }
    }
}
