using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[CustomEditor(typeof(ObjectRotate))]
public class ObjectRotationEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ObjectRotate obj = (ObjectRotate)target;

        GUILayout.BeginHorizontal();
        obj.initialAngle = EditorGUILayout.FloatField("Initial Angle", obj.initialAngle);
        if(GUILayout.Button("Set"))
        {
            obj.initialAngle = obj.transform.rotation.eulerAngles.y;
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Event Angles");

        if(obj.eventAngles == null)
        {
            obj.eventAngles = new List<float>();
        }


        for(int i = 0; i < obj.eventAngles.Count; i++)
        {
            GUILayout.BeginHorizontal();
            obj.eventAngles[i] = EditorGUILayout.FloatField($"Angle {i}", obj.eventAngles[i]);
            if (GUILayout.Button("View"))
            {
                obj.RotateTo(obj.eventAngles[i]);
            }
            if (GUILayout.Button("Set"))
            {
                obj.eventAngles[i] = obj.transform.rotation.eulerAngles.y;
            }
            if (GUILayout.Button("X"))
            {
                obj.eventAngles.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }

        if(GUILayout.Button("Add New Event Angle"))
        {
            obj.eventAngles.Add(obj.transform.rotation.eulerAngles.y);
        }

    }
    private void OnSceneGUI()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            
        }
    }

    
}
