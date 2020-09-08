using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    #region SerializedProperty

    #region Look Settings
    private SerializedProperty playerCamera;
    private SerializedProperty verticalRotationRange;
    #endregion

    #region Movement Settings

    private SerializedProperty playerCanMove;
    private SerializedProperty walkSpeed;
    private SerializedProperty playerCanJump;
    private SerializedProperty canHoldJump;
    private SerializedProperty jumpPower;

    #endregion

    #region Headbob Settings

    private SerializedProperty useHeadBob;
    private SerializedProperty head;
    private SerializedProperty snapHeadjointToCapsul;
    private SerializedProperty headbobFrequency;
    private SerializedProperty headbobSwayAngle;
    private SerializedProperty headbobHeight;
    private SerializedProperty headbobSideMovement;
    private SerializedProperty jumpLandIntensity;
    
    #endregion


    #endregion


    public override void OnInspectorGUI()
    {
        playerCamera = serializedObject.FindProperty("playerCamera");
        verticalRotationRange = serializedObject.FindProperty("verticalRotationRange");
        playerCanMove = serializedObject.FindProperty("playerCanMove");
        walkSpeed = serializedObject.FindProperty("walkSpeed");
        playerCanJump = serializedObject.FindProperty("playerCanJump");
        canHoldJump = serializedObject.FindProperty("canHoldJump");
        jumpPower = serializedObject.FindProperty("jumpPower");
        useHeadBob = serializedObject.FindProperty("useHeadBob");
        head = serializedObject.FindProperty("head");
        snapHeadjointToCapsul = serializedObject.FindProperty("snapHeadjointToCapsul");
        headbobFrequency = serializedObject.FindProperty("headbobFrequency");
        headbobSwayAngle = serializedObject.FindProperty("headbobSwayAngle");
        headbobHeight = serializedObject.FindProperty("headbobHeight");
        headbobSideMovement = serializedObject.FindProperty("headbobSideMovement");
        jumpLandIntensity = serializedObject.FindProperty("jumpLandIntensity");

        serializedObject.Update();
        
        ShowLabel("---- Basic Settings ----");
        EditorGUILayout.PropertyField(playerCamera, new GUIContent("First Person Camera"));

        ShowLabel("---- Look Settings ----");
        EditorGUILayout.PropertyField(verticalRotationRange, new GUIContent("Vertical Range"));

        ShowLabel("---- Move Settings ----");
        EditorGUILayout.PropertyField(playerCanMove, new GUIContent("Player Can Move"));
        EditorGUILayout.PropertyField(walkSpeed, new GUIContent("Walk Speed"));

        ShowLabel("---- Jump Settings ----");
        EditorGUILayout.PropertyField(playerCanJump, new GUIContent("Player Can Jump"));
        EditorGUILayout.PropertyField(canHoldJump, new GUIContent("Can Hold Jump"));
        EditorGUILayout.PropertyField(jumpPower, new GUIContent("Jump Power"));

        ShowLabel("---- Headbob Settings ----");
        EditorGUILayout.PropertyField(useHeadBob, new GUIContent("Headbob Enable"));
        EditorGUILayout.PropertyField(head, new GUIContent("Head"));
        EditorGUILayout.PropertyField(snapHeadjointToCapsul, new GUIContent("Snap Headjoint To Capsule"));
        EditorGUILayout.PropertyField(headbobFrequency, new GUIContent("Frequency"));
        EditorGUILayout.PropertyField(headbobSwayAngle, new GUIContent("Sway Angle"));
        EditorGUILayout.PropertyField(headbobHeight, new GUIContent("Height"));
        EditorGUILayout.PropertyField(headbobSideMovement, new GUIContent("Side Movement"));
        EditorGUILayout.PropertyField(jumpLandIntensity, new GUIContent("Land Intensity"));

        GUILayout.Space(10);
        serializedObject.ApplyModifiedProperties();
    }

    private void ShowLabel(string str)
    {
        GUILayout.Space(10);
        GUILayout.Label(str);
        GUILayout.Space(10);
    }
}
