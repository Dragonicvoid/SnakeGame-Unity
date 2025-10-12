#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
  public override void OnInspectorGUI()
  {
    SaveManager myScript = (SaveManager)target;

    if (GUILayout.Button("Delete Save"))
    {
      myScript.DeleteSave();
    }

    if (GUI.changed)
    {
      EditorUtility.SetDirty(myScript);
    }
  }
}
#endif