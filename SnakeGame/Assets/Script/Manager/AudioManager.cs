using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
  struct BgmData
  {
    public string Key;
    public float Vol;
  }
  public static AudioManager _instance;

  public static AudioManager Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new AudioManager();
      }
      return _instance;
    }
  }

  List<AudioSource> unusedSources = new List<AudioSource>();

  Dictionary<string, AudioSource> runningAudio = new Dictionary<string, AudioSource>();

  float masterVol = 1f;

  BgmData? currBGM;

  BgmData? transitionBGM;

  Coroutine? bgmTransitionCour;

  void Awake()
  {
    DontDestroyOnLoad(gameObject);
    _instance = this;
  }

  void OnEnable()
  {
    foreach (AudioSource item in unusedSources)
    {
      item.volume = masterVol;
    }
  }

  AudioSource getUnusedSource()
  {
    AudioSource source = Util.Pop(unusedSources);

    if (!source)
    {
      source = gameObject.AddComponent<AudioSource>();
      source.volume = masterVol;
    }

    return source;
  }

  void returnSource(AudioSource source)
  {
    unusedSources.Add(source);
  }

  public void PlaySFX(string key)
  {
    AudioClip? clip;
    AssetManager.Instance.assetsAudio.TryGetValue(key, out clip);

    if (!clip) return;

    string timeStr = Time.time.ToString();
    string audioStr = "SFX_" + key + "_" + timeStr;

    AudioSource source = getUnusedSource();

    bool success = runningAudio.TryAdd(audioStr, source);
    if (!success)
    {
      returnSource(source);
      return;
    }

    source.volume = masterVol;
    source.clip = clip;
    source.loop = false;
    source.PlayOneShot(clip);

    float length = clip.length;
    StartCoroutine(waitAndRemoveAudio(length, audioStr));
  }

  public void PlayBGM(string key, float vol, bool withTransition = true)
  {
    AudioClip? clip;
    AssetManager.Instance.assetsAudio.TryGetValue(key, out clip);

    if (!clip) return;

    string audioStr = "BGM_" + key + "_" + vol;

    if (currBGM != null && audioStr == currBGM.Value.Key)
    {
      return;
    }

    if (withTransition && currBGM != null)
    {
      updateCurrBgm();
      transitionBGM = new BgmData
      {
        Key = audioStr,
        Vol = vol,
      };

      AudioSource currSource;
      runningAudio.TryGetValue(currBGM.Value.Key, out currSource);

      AudioSource transSource = getUnusedSource();
      bool success = runningAudio.TryAdd(audioStr, transSource);
      if (!success)
      {
        returnSource(transSource);
        return;
      }

      BaseTween<List<BgmData>> tweenData = new BaseTween<List<BgmData>>(
        1f,
        new List<BgmData> { currBGM.Value, transitionBGM.Value },
        (dist, bgmList) =>
        {
          currSource.volume = bgmList[0].Vol * masterVol * (1.0f - dist);
          transSource.volume = bgmList[1].Vol * masterVol * dist;

          transSource.clip = clip;
          transSource.loop = true;
          transSource.Play();
        },
        (dist, bgmList) =>
        {
          currSource.volume = bgmList[0].Vol * masterVol * (1.0f - dist);
          transSource.volume = bgmList[1].Vol * masterVol * dist;
        },
        (dist, bgmList) =>
        {
          currSource.volume = bgmList[0].Vol * masterVol * (1.0f - dist);
          transSource.volume = bgmList[1].Vol * masterVol * dist;
          updateCurrBgm();
        }
      );
      IEnumerator<object> tween = Tween.Create(tweenData);
      bgmTransitionCour = StartCoroutine(tween);
    }
    else
    {
      AudioSource source = getUnusedSource();
      bool success = runningAudio.TryAdd(audioStr, source);
      if (!success)
      {
        returnSource(source);
        return;
      }

      currBGM = new BgmData
      {
        Key = audioStr,
        Vol = vol,
      };

      source.volume = vol * masterVol;
      source.clip = clip;
      source.loop = true;
      source.Play();
    }
  }

  void updateCurrBgm()
  {
    if (bgmTransitionCour == null || transitionBGM == null) return;

    StopCoroutine(bgmTransitionCour);
    AudioSource transitionSource;
    AudioSource currSource;

    runningAudio.TryGetValue(currBGM.Value.Key, out currSource);
    runningAudio.TryGetValue(transitionBGM.Value.Key, out transitionSource);

    if (currSource)
    {
      currSource.Stop();
      runningAudio.Remove(currBGM.Value.Key);
      returnSource(currSource);
    }

    if (transitionSource)
    {
      transitionSource.volume = transitionBGM.Value.Vol * masterVol;
    }

    currBGM = transitionBGM;
    transitionBGM = null;
  }

  IEnumerator<object> waitAndRemoveAudio(float length, string key)
  {
    yield return PersistentData.Instance.GetWaitSecond(length);

    AudioSource? source;
    bool success = runningAudio.TryGetValue(key, out source);

    if (!source || !success) yield break;

    runningAudio.Remove(key);
    source.Stop();
    returnSource(source);
  }

  public void SetVolume(float vol)
  {
    masterVol = Mathf.Clamp(vol, 0, 1);

    foreach (KeyValuePair<string, AudioSource> item in runningAudio)
    {
      if (item.Key.Substring(0, 3) == "BGM")
      {
        BgmData? data = currBGM.Value.Key == item.Key ? currBGM : transitionBGM;

        if (data == null)
        {
          item.Value.volume = masterVol;
        }
        else
        {
          item.Value.volume = masterVol * data.Value.Vol;
        }
      }
      else
      {
        item.Value.volume = masterVol;
      }
    }

    foreach (AudioSource item in unusedSources)
    {
      item.volume = masterVol;
    }
  }

  public void SetCurrBgmVol(float vol)
  {
    if (currBGM == null) return;

    BgmData curr = currBGM.Value;

    curr.Vol = vol;

    AudioSource source;
    runningAudio.TryGetValue(curr.Key, out source);

    if (!source) return;

    source.volume = vol * masterVol;
  }

  void OnDisable()
  {
    StopAllCoroutines();
    bgmTransitionCour = null;
    foreach (AudioSource item in runningAudio.Values)
    {
      item.Stop();
      unusedSources.Add(item);
    }
    currBGM = default;
    transitionBGM = default;
  }
}
