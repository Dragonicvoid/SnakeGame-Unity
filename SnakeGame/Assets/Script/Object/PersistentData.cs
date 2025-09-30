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
    RenderTex = RenderTexture.active;
    Application.targetFrameRate = 120;
  }

  public RenderTexture RenderTex;

  public int SelectedMap = 0;

  public DIFFICULTY Difficulty = DIFFICULTY.MEDIUM;

  private bool isLoading;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
}
