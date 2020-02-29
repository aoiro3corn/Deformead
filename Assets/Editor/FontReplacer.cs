using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class MyFont : ScriptableObject {
    public Font font;
    public int size;
}

public class FontReplacer : EditorWindow {
    static SerializedProperty sp;
    static SerializedProperty font_size;

    [MenuItem("Tools/Custom/Replace All Fonts")]
    public static void Open() {
        EditorWindow a = EditorWindow.GetWindow(typeof(FontReplacer), true, "Font Replacer");
        var obj = ScriptableObject.CreateInstance<MyFont>();
        var serializedObject = new UnityEditor.SerializedObject(obj);

        sp = serializedObject.FindProperty("font");
        font_size = serializedObject.FindProperty("size");

        Debug.Log("font " + sp.propertyType);
        Debug.Log("font_size " + font_size.propertyType);
    }

    void OnGUI() {
        EditorGUILayout.PropertyField(sp);
        EditorGUILayout.PropertyField(font_size);
        if (GUILayout.Button("Replace All Fonts")) {
            Debug.Log("you are trying to replace all fonts to new one");

            var textComponents = Resources.FindObjectsOfTypeAll(typeof(Text)) as Text[];
            foreach (var component in textComponents) {
                component.font = sp.objectReferenceValue as Font;
                component.fontSize = font_size.intValue;
            }
            // ※追記 : シーンに変更があることをUnity側に通知しないと、シーンを切り替えたときに変更が破棄されてしまうので、↓が必要
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}