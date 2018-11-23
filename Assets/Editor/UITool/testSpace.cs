using UnityEngine;
using UnityEditor;
using System.Collections;

public class testSpace : EditorWindow
{
    [MenuItem("PrivateTools/testSpace")]
    private static void Init()
    {
        EditorWindow.GetWindow<testSpace>(false, "TestSpace", true).Show();
    }

    private void OnGUI()
    {
        GUILayout.TextField("title", "SearchTextField");
        GUILayout.Space(50);
        GUILayout.Box("box");
    }
}
