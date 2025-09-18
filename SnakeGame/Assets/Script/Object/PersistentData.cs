using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentData : MonoBehaviour
{
  public static PersistentData Instance;

  void Awake()
  {
    DontDestroyOnLoad(gameObject);
    Instance = this;
  }

  void Start()
  {
    SceneManager.LoadScene(1, LoadSceneMode.Single);
  }

  public int SelectedMap = 0;

  private bool isLoading;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
}
