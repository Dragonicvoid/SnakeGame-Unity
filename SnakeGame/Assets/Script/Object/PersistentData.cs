using System.Collections.Generic;
using UnityEngine;

public class PersistentData : MonoBehaviour
{
  public static PersistentData _instance;
  public static PersistentData Instance
  {
    get
    {
      if (_instance)
      {
        _instance = FindAnyObjectByType(typeof(PersistentData)) as PersistentData;

        if (!_instance)
        {
          var obj = new GameObject("PersistentData");
          DontDestroyOnLoad(obj);
          _instance = obj.AddComponent<PersistentData>();
        }
      }
      return _instance;
    }
  }

  public int SelectedMap = 0;

  private bool isLoading;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
}
