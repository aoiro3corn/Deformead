using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(ExploreButton), true)]
public class ExploreButtonEditor : ButtonEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        this.serializedObject.Update();
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("objectCheck"), true);
        this.serializedObject.ApplyModifiedProperties();
    }
}

[CanEditMultipleObjects, CustomEditor(typeof(ChoiceButton), true)]
public class ChoiceButtonEditor : ButtonEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        this.serializedObject.Update();
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_txtChoice"), true);
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_imgChoice"), true);
        this.serializedObject.ApplyModifiedProperties();
    }
}