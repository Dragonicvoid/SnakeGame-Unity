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

  public bool IsButtonLock = false;

  public bool isPaused = true;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

  Coroutine unlockDelayCour;

  public void LockButton(float time = 0.032f)
  {
    IsButtonLock = true;

    // if has time value positive the next button click by time
    // if has time value 0 or negative lock button forever until it is manually unlocked
    if (time > 0)
    {
      if (unlockDelayCour != null)
      {
        StopCoroutine(unlockDelayCour);
      }
      unlockDelayCour = StartCoroutine(unlockButtonByDelay(time));
    }
  }

  IEnumerator<object> unlockButtonByDelay(float time)
  {
    yield return null;
    yield return GetWaitSecond(time);
    UnlockButton();
  }

  public void UnlockButton()
  {
    if (unlockDelayCour != null)
    {
      StopCoroutine(unlockDelayCour);
    }
    IsButtonLock = false;
  }
}
