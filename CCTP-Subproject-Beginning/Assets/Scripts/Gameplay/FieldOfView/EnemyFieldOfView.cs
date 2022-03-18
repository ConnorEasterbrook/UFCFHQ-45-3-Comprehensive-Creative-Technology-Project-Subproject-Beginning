using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFieldOfView : MonoBehaviour
{
    // Detection variables
    [Range (0, 20)] public float detectionRadius = 10;
    [Range (0, 360)] public float viewAngle = 110;
    [Range (0.1f, 2.0f)] public float raysPerDegree = 1;
    public LayerMask detectionMask;
    public LayerMask obstructionMask;
    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();
    private GameObject viewRenderChild;
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    // Raycasting variables
    private bool rayCastHit;
    private Vector3 rayCastHitPoint;
    private float rayCastHitPointDistance;
    private float rayCastHitPointAngle;

    private void Awake() 
    {
        viewRenderChild = transform.GetChild(0).gameObject;
    }

    private void Start() 
    {
        // Get child object's mesh filter for drawing view detection
        viewMeshFilter = viewRenderChild.GetComponent<MeshFilter>();
        
        // Establish viewMesh component
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void Update() 
    {
        FindTargetsInView(); // Call the detection function
        UpdateRayCasts(); // Update the ray casts
    }

    private void FindTargetsInView()
    {
        // Clear visibleTargets list on each call to ensure no detection release lag
        visibleTargets.Clear();

        // Create the detection area and add any detected object (on the detectionMask layer) to be added to an array
        Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, detectionRadius, detectionMask);

        // If there is a detected object then perform code for each detected object
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            // Set current object transform as the target transform
            Transform targetTransform = targetsInViewRadius[i].transform;

            // Get the direction to the target transform
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

            // If the target transform is within the detection radius
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                // Get the distance to the target transform
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                // If there are no obstruction between the character and the target transform then allow sight
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    // Add target transform to the list of visible targets
                    visibleTargets.Add (targetTransform);
                }
            }
        }
    }

    private void UpdateRayCasts()
    {
        // Get the amount of rays required per degrees within the view angle
        int rayCount = Mathf.RoundToInt (viewAngle * raysPerDegree);

        // Get the amount of degrees between each ray
        float rayAngleSize = viewAngle / rayCount;

        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= rayCount; i++)
        {
            // Get the location of view angle point 1 and then go through view to view angle point 2
            float currentAngle = transform.eulerAngles.y - viewAngle / 2 + rayAngleSize * i;
            
            // Using our current angle, update raycast information
            Vector3 newViewHitPoint = GetRayCastHitPoint (currentAngle);

            // Add the raycast iteration hit point to the list
            viewPoints.Add (newViewHitPoint);
        }

        DrawView (viewPoints);
    }

    private Vector3 GetRayCastHitPoint (float globalAngle)
    {
        // Get the view direction from our angle
        Vector3 direction = DirectionFromAngle (true, globalAngle);

        RaycastHit hit;

        // If a raycast from our character towards the direction hits an obstruction then return hit point
        if (Physics.Raycast (transform.position, direction, out hit, detectionRadius, obstructionMask))
        {
            return hit.point;
        }
        // Else return point on radius
        else
        {
            Vector3 returnVector = transform.position + direction * detectionRadius;
            return returnVector;
        }
    }

    private void DrawView(List<Vector3> viewPoints)
    {
        // Get amount of vertices. +1 because of transform.position (position of character)
        int vertexCount = viewPoints.Count + 1;

        // Create Vector3 variable to store each vertices' position
        Vector3[] vertices = new Vector3 [vertexCount];

        // Calculation for the amount of triangles that will be created by connecting 3 vertices
        int[] triangles = new int [(vertexCount - 2) * 3];

        // Set the first vertices to be transform.position (to the character)
        vertices [0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            // i + 1 so we don't overwrite the first vertices
            vertices [i + 1] = transform.InverseTransformPoint (viewPoints [i]);

            // Stop from going out of array
            if (i < vertexCount - 2)
            {
                // Set triangle vertex positions
                triangles [i * 3] = 0; // Set first vertex of triangle to be transform.position (to the character)
                triangles [i * 3 + 1] = i + 1; // Set second vertex
                triangles [i * 3 + 2] = i + 2; // Set third vertex
            }
        }

        // Set viewMesh to match calculated view triangles
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // private void SetRayCastInfo (float globalAngle)
    // {
    //     Vector3 direction = DirectionFromAngle (true, globalAngle);

    //     RaycastHit hit;

    //     if (Physics.Raycast (transform.position, direction, out hit, detectionRadius, obstructionMask))
    //     {
    //         rayCastHit = true;
    //         rayCastHitPoint = hit.point;
    //         rayCastHitPointDistance = hit.distance;
    //         rayCastHitPointAngle = globalAngle;
    //     }
    //     else
    //     {
    //         rayCastHit = false;
    //         rayCastHitPoint = transform.position + direction * detectionRadius;
    //         rayCastHitPointDistance = detectionRadius;
    //         rayCastHitPointAngle = globalAngle;
    //     }
    // }

    // Calculate view angle points. Gets called twice with different values to create the FOV
    public Vector3 DirectionFromAngle(bool globalAngle, float angleInDegrees)
    {
        if (!globalAngle)
        {
            // Get the view angle point and add the character's Y rotation to correct view direction
            angleInDegrees += transform.eulerAngles.y;
        }

        // Return trigonometry vector for the location of the view angle point 
        return new Vector3 (Mathf.Sin (angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos (angleInDegrees * Mathf.Deg2Rad));
    }
}
