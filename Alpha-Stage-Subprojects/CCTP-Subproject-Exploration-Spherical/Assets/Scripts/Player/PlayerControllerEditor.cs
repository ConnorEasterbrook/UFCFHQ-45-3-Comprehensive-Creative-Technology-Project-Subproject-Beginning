using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerController editScript = target as PlayerController;

        if (editScript.sphericalMovement)
        {
            editScript.planetGameObject = EditorGUILayout.ObjectField ("Planet GameObject", editScript.planetGameObject, typeof (GameObject),  true) as GameObject;
        }
    }
}
