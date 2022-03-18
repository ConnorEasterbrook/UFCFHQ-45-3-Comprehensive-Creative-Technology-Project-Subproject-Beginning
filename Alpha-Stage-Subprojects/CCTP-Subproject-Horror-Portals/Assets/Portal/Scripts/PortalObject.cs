using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PortalObject : MonoBehaviour
{
    public Vector3 previousOffsetFromPortal { get; set; }

    // Called by the portal to commence portal object teleportation. It is virtual so that it can be referenced in FirstPersonController
    public virtual void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) 
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}
