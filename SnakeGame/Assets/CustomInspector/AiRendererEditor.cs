using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AiRenderer))]
public class AiRendererEditor : Editor
{
  public override void OnInspectorGUI()
  {
    AiRenderer myScript = (AiRenderer)target;

    myScript.LineWidth = EditorGUILayout.FloatField("Dir Line Width", myScript.LineWidth);
    myScript.UpdateTime = EditorGUILayout.FloatField("Update Interval", myScript.UpdateTime);

    myScript.MoveDirColor = EditorGUILayout.ColorField("Dir Line Color", myScript.MoveDirColor);
    myScript.VeloColor = EditorGUILayout.ColorField("Dir Velo Color", myScript.VeloColor);
    myScript.InputColor = EditorGUILayout.ColorField("Dir Input Color", myScript.InputColor);
    myScript.OpenListColor = EditorGUILayout.ColorField("Open Path Color", myScript.OpenListColor);
    myScript.CloseListColor = EditorGUILayout.ColorField("Close Path Color", myScript.CloseListColor);
    myScript.WallColor = EditorGUILayout.ColorField("Wall Color", myScript.WallColor);
    myScript.PathColor = EditorGUILayout.ColorField("Path Color", myScript.PathColor);
    myScript.OccupyColor = EditorGUILayout.ColorField("Occupy Color", myScript.OccupyColor);

    for (int i = 0; i < myScript.Mask.Count; i++)
    {
      myScript.Mask[i] = EditorGUILayout.Toggle($"Draw {getMaskNameByIdx(i)}", myScript.Mask[i]);
    }

    if (GUI.changed)
    {
      EditorUtility.SetDirty(myScript);
    }
  }

  string getMaskNameByIdx(int idx)
  {
    switch (idx)
    {
      case 0:
        return "Map";
      case 1:
        return "Path";
      case 2:
        return "Direction";
      default:
        return "Unknown";
    }
  }
}
