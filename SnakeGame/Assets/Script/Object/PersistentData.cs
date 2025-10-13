using System.Collections.Generic;
using UnityEngine;

public class PersistentData : MonoBehaviour
{
  public static PersistentData _instance;

  public static PersistentData Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new PersistentData();
      }
      return _instance;
    }
  }

  void Awake()
  {
    DontDestroyOnLoad(gameObject);
    _instance = this;
  }

  void Start()
  {
    Difficulty = (DIFFICULTY)SaveManager.Instance.SaveData.LastDiffSelect;
    RenderTex = RenderTexture.active;
    Application.targetFrameRate = 120;
  }

  public WaitForSeconds GetWaitSecond(float time)
  {
    WaitForSeconds res;
    waitForSecond.TryGetValue(time, out res);

    if (res == null)
    {
      res = new WaitForSeconds(time);
      waitForSecond.Add(time, res);
    }

    return res;
  }

  public RenderTexture RenderTex;

  public int SelectedMap = 0;

  public DIFFICULTY Difficulty = DIFFICULTY.MEDIUM;

  public WaitForEndOfFrame WaitForFrameEnd = new WaitForEndOfFrame();

  Dictionary<float, WaitForSeconds> waitForSecond = new Dictionary<float, WaitForSeconds>();

  private bool isLoading;

  public bool isPaused = true;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
}
