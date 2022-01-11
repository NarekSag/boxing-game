using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(SMBEvent))]
public class Editor_SMBEvent : Editor
{
    #region Const
    private const string TOTAL_FRAMES = "totalFrames";
    private const string CURRENT_FRAME = "currentFrame";
    private const string NORMALIZED_TIME = "normalizedTime";
    private const string NORMALIZED_TIME_UNCAPPED = "normalizedTimeUncapped";
    private const string MOTION_TIME = "motionTime";
    private const string EVENTS_LIST = "eventsList";
    #endregion

    #region SerializedProperty
    private SerializedProperty totalFrames;
    private SerializedProperty currentFrame;
    private SerializedProperty normalizedTime;
    private SerializedProperty normalizedTimeUncapped;
    private SerializedProperty motionTime;
    private SerializedProperty events;
    #endregion

    private ReorderableList eventsList;

    private void OnEnable()
    {
        totalFrames = serializedObject.FindProperty(TOTAL_FRAMES);
        currentFrame = serializedObject.FindProperty(CURRENT_FRAME);
        normalizedTime = serializedObject.FindProperty(NORMALIZED_TIME);
        normalizedTimeUncapped = serializedObject.FindProperty(NORMALIZED_TIME_UNCAPPED);
        motionTime = serializedObject.FindProperty(MOTION_TIME);
        events = serializedObject.FindProperty(EVENTS_LIST);
        eventsList = new ReorderableList(serializedObject, events, true, true, true, true);

        eventsList.drawHeaderCallback = DrawHeaderCallback;
        eventsList.drawElementCallback = DrawElementCallback;
        eventsList.elementHeightCallback = ElementHeightCallback;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.IndentLevelScope(1))
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((SMBEvent)target),typeof(SMBEvent),false);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(totalFrames);
                EditorGUILayout.PropertyField(currentFrame);
                EditorGUILayout.PropertyField(normalizedTime);
                EditorGUILayout.PropertyField(normalizedTimeUncapped);
            }

            EditorGUILayout.PropertyField(motionTime);
        }
        eventsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, "Events");
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = eventsList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty eventName = element.FindPropertyRelative("eventName");
        SerializedProperty timing = element.FindPropertyRelative("timing");

        string elementTitle;
        int timingIndex = timing.enumValueIndex;
        elementTitle = string.IsNullOrEmpty(eventName.stringValue) ?
            $"Event *Name* ({timing.enumDisplayNames[timingIndex] })" :
            $"Event {eventName.stringValue} ({timing.enumDisplayNames[timingIndex] })";

        EditorGUI.PropertyField(rect, element, new GUIContent(elementTitle), true);
    }

    private float ElementHeightCallback(int index)
    {
        SerializedProperty element = eventsList.serializedProperty.GetArrayElementAtIndex(index);
        float propertyHeight = EditorGUI.GetPropertyHeight(element, true);
        return propertyHeight;
    }
}
