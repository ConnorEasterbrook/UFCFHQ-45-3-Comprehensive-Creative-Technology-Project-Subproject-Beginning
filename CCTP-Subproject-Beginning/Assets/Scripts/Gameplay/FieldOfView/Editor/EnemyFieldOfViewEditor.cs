using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyFieldOfView))]
public class EnemyFieldOfViewEditor : Editor
{
    EnemyFieldOfView editScript;

    Vector3 viewAnglePoint_1;
    Vector3 viewAnglePoint_2;

    private void OnSceneGUI()
    {
        editScript = target as EnemyFieldOfView;

        Handles.color = Color.white; // Set colour
        Handles.DrawWireArc(editScript.transform.position, Vector3.up, Vector3.forward, 360, editScript.detectionRadius); // Using colour, create a circle that matches the view radius

        // Get the view angle points from editScript
        viewAnglePoint_1 = editScript.DirectionFromAngle (false, -editScript.viewAngle / 2);
        viewAnglePoint_2 = editScript.DirectionFromAngle (false, editScript.viewAngle / 2);

        Handles.color = Color.red; // Set colour
        // Using colour, draw a line from the character to the radius edge
        Handles.DrawLine(editScript.transform.position, editScript.transform.position + viewAnglePoint_1 * editScript.detectionRadius); 
        Handles.DrawLine(editScript.transform.position, editScript.transform.position + viewAnglePoint_2 * editScript.detectionRadius);

        // If the character can see a target
        foreach (Transform visibleTarget in editScript.visibleTargets)
        {
            Handles.color = Color.green; // Set colour
            Handles.DrawLine(editScript.transform.position, visibleTarget.position); // Using colour, draw a line to the target from the character
        }
    }
}
