using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    PlayerController editScript;
    bool showNoConflictMenus;

    // Conditional variables
    bool sphericalWallWalk = false;

    public override void OnInspectorGUI()
    {
        editScript = target as PlayerController;

        base.OnInspectorGUI();

        EditorGUILayout.Space();
        GUILayout.Label ("Extra Movement Options", EditorStyles.boldLabel);

        BoolCheck();
        SphericalMovement();
        WallWalking();
    }

    // Function to check for an enabled header boolean. If so, hide the others that may conflict
    private void BoolCheck()
    {
        if (sphericalWallWalk)
        {
            DisableSNCM();
        }
        else
        {
            showNoConflictMenus = EditorGUILayout.Toggle (new GUIContent ("Show No Conflict Menus", "Show the header booleans that don't conflict with ones already enabled."), showNoConflictMenus);
        }

        if (showNoConflictMenus)
        {
            ShowNoConflictMenus();
            ConditionalBooleans();
        }
        else
        {
            if (!editScript.wallWalk)
            {
                // BOOL SPHERICALMOVEMENT (header)
                editScript.sphericalMovement = EditorGUILayout.Toggle (new GUIContent ("Spherical Movement", "Enable Planet-Based Movement?"), editScript.sphericalMovement);
            }
            else
            {
                // BOOL SPHERICALMOVEMENT (header)
                GUI.enabled = false; // Make variables inaccessible until true
                editScript.sphericalMovement = EditorGUILayout.Toggle (new GUIContent ("Spherical Movement", "Enable Planet-Based Movement?"), editScript.sphericalMovement);
                GUI.enabled = true; // Make variables accessible again
            }

            if (!editScript.sphericalMovement)
            {
                // BOOL WALLWALK (header)
                editScript.wallWalk = EditorGUILayout.Toggle (new GUIContent ("Wall Walking", "Enable Wall-Based Movement?"), editScript.wallWalk);
            }
            else
            {
                // BOOL WALLWALK (header)
                GUI.enabled = false; // Make variables inaccessible until true
                editScript.wallWalk = EditorGUILayout.Toggle (new GUIContent ("Wall Walking", "Enable Wall-Based Movement?"), editScript.wallWalk);
                GUI.enabled = true; // Make variables accessible again
            }
        }

        UnconditionalBooleans();
    }

    private void DisableSNCM()
    {
        GUI.enabled = false; // Make variables inaccessible until true
        showNoConflictMenus = EditorGUILayout.Toggle (new GUIContent ("Show No Conflict Menus", "Show the header booleans that don't conflict with ones already enabled."), showNoConflictMenus);
        GUI.enabled = true; // Make variables accessible again
    }

    private void ShowNoConflictMenus()
    {
        // BOOL SPHERICALMOVEMENT (header)
        editScript.sphericalMovement = EditorGUILayout.Toggle (new GUIContent ("Spherical Movement", "Enable Planet-Based Movement?"), editScript.sphericalMovement);

        // BOOL WALLWALK (header)
        editScript.wallWalk = EditorGUILayout.Toggle (new GUIContent ("Wall Walking", "Enable Wall-Based Movement?"), editScript.wallWalk);
    }

    private void ConditionalBooleans()
    {
        if (editScript.wallWalk || editScript.sphericalMovement)
        {
            sphericalWallWalk = true;
        }
        else
        {
            sphericalWallWalk = false;
        }
    }

    private void SphericalMovement()
    {
        if (editScript.sphericalMovement)
        {
            EditorGUILayout.Space();
            GUILayout.Label ("Spherical Movement", EditorStyles.boldLabel);

            // GAMEOBJECT PLANETGAMEOBJECT
            editScript.planetGameObject = EditorGUILayout.ObjectField (new GUIContent ("Planet GameObject", "Object for planet-based gravity."), editScript.planetGameObject, typeof (GameObject),  true) as GameObject;
        }
    }

    private void WallWalking()
    {
        if (editScript.wallWalk)
        {
            EditorGUILayout.Space();
            GUILayout.Label ("Wall Walking", EditorStyles.boldLabel);

            // FLOAT GRAVITYROTATIONSPEED
            editScript.gravityRotationSpeed = EditorGUILayout.FloatField (new GUIContent ("Gravity Rotation Speed", "Set the rotation speed of the player to the wall."), editScript.gravityRotationSpeed);

            // FLOAT WALLDETECTION
            editScript.wallWalkDetection = EditorGUILayout.FloatField (new GUIContent ("Wall Walk Detection Range", "Set range of the collision detection raycasts."), editScript.wallWalkDetection);
        }
    }

    private void UnconditionalBooleans()
    {
        EditorGUILayout.Space();
        GUILayout.Label ("Extra Visual Options", EditorStyles.boldLabel);

        // BOOL INSIDESPHERE (header)
        editScript.insideSphere = EditorGUILayout.Toggle (new GUIContent ("Inside Sphere", "Enable inside sphere visual effect?"), editScript.insideSphere);
    }
}
