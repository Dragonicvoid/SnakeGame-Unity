using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AssetLoader : MonoBehaviour
{
  [SerializeField]
  Slider downloadSlider = null;

  uint totalAssets = 0;
  uint downloadAsset = 0;

  IEnumerator<object> downloadEnumerator = null;

  private void Awake()
  {
    downloadEnumerator = startAssetsDownload();
  }

  private void Start()
  {
    StartCoroutine(downloadEnumerator);
  }

  List<AssetConfig> getAssetsConf()
  {
    // Texture 
    List<AssetConfig> assets = new List<AssetConfig>() {
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/image/upload/v1759574990/Snake/Skins/slime_normal_isqsoc.jpg",
          key = ASSET_KEY.SKIN_WITHER_NORMAL,
          type = ASSET_TYPE.IMAGE,
          opts = new DownloadOpts(),
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/image/upload/v1759575035/Snake/Skins/bubble_kblnw4.png",
          key = ASSET_KEY.SKIN_BUBBLE_MAIN,
          type = ASSET_TYPE.IMAGE,
          opts = new DownloadOpts {
            retries = 4,
          },
      },

      // Music 
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760447743/Snake/audio/happy_adventure_hor8hb.mp3",
          key = ASSET_KEY.BGM_MAIN_MENU,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.MPEG
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760447720/Snake/audio/8bit_title_screen_fdl6w3.mp3",
          key = ASSET_KEY.BGM_GAMEPLAY,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.MPEG
          },
      },

      // SFX
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448039/Snake/audio/eat_2_pu93uq.wav",
          key = ASSET_KEY.SFX_EAT,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448347/Snake/audio/move_1_lzv84y.wav",
          key = ASSET_KEY.SFX_START_PLAY,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448360/Snake/audio/move_2_dvcxc8.wav",
          key = ASSET_KEY.SFX_BACK_TO_MENU,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760449061/Snake/audio/button_click_5_esh6jq.wav",
          key = ASSET_KEY.SFX_BUTTON_CLICK,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448285/Snake/audio/button_click_3_btuyka.wav",
          key = ASSET_KEY.SFX_CLICK_SKIN,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448317/Snake/audio/button_click_4_w5avaa.wav",
          key = ASSET_KEY.SFX_CLICK_TAB,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760447871/Snake/audio/lose_4_w5atfe.wav",
          key = ASSET_KEY.SFX_LOSE,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448023/Snake/audio/eat_3_fke7ui.wav",
          key = ASSET_KEY.SFX_WIN,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
      new AssetConfig {
          url = "https://res.cloudinary.com/dyfgknhce/video/upload/v1760448001/Snake/audio/fire_1_udfndd.wav",
          key = ASSET_KEY.SFX_FIRE,
          type = ASSET_TYPE.AUDIO,
          opts = new DownloadOpts {
            retries = 4,
            audioType = AudioType.WAV
          },
      },
    };

    totalAssets = (uint)assets.Count;
    return assets;
  }

  IEnumerator<object> startAssetsDownload()
  {
    yield return null;

    List<AssetConfig> confs = this.getAssetsConf();
    updateSlider();

    AssetLoadEvent.Instance.onDownloadAssetCompleted -= onDownloadCompleted;
    AssetLoadEvent.Instance.onDownloadAssetFailed -= onAssetFailed;
    AssetLoadEvent.Instance.onDownloadAssetSuccess -= onAssetSuccess;

    AssetLoadEvent.Instance.onDownloadAssetCompleted += onDownloadCompleted;
    AssetLoadEvent.Instance.onDownloadAssetFailed += onAssetFailed;
    AssetLoadEvent.Instance.onDownloadAssetSuccess += onAssetSuccess;

    confs.ForEach((asset) =>
    {
      switch (asset.type)
      {
        case ASSET_TYPE.IMAGE:
          StartCoroutine(downloadImage(asset));
          break;
        case ASSET_TYPE.TEXT:
          StartCoroutine(downloadText(asset));
          break;
        case ASSET_TYPE.AUDIO:
          StartCoroutine(downloadAudio(asset));
          break;
        default:
          break;
      }
    });
  }

  void onAssetSuccess(AssetConfig _)
  {
    downloadAsset++;
    updateSlider();
  }

  void onAssetFailed(AssetConfig conf)
  {
    Debug.Log("Missing Assets: " + conf.key);
  }

  void onDownloadCompleted()
  {
    SceneManager.LoadScene(1, LoadSceneMode.Single);
  }

  void updateSlider()
  {
    if (downloadSlider == null) return;

    float value;
    if (totalAssets != 0)
    {
      value = (float)downloadAsset / (float)totalAssets;
    }
    else
    {
      value = 1;
    }

    downloadSlider.value = value;

    if (value >= 1)
    {
      AssetLoadEvent.Instance.DownloadAssetCompletedEnter();
    }
  }

  IEnumerator<object> downloadImage(AssetConfig conf, uint tries = 0)
  {
    UnityWebRequest request = UnityWebRequestTexture.GetTexture(conf.url);
    yield return request.SendWebRequest();
    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    {
      if (conf.opts.retries < tries)
      {
        downloadImage(conf, tries++);
      }
      else
      {
        AssetLoadEvent.Instance.DownloadAssetFailedEnter(conf);
      }
    }
    else
    {
      Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
      if (!tex) yield break;
      AssetManager.Instance.AddTexture(conf.key, ((DownloadHandlerTexture)request.downloadHandler).texture);
      AssetLoadEvent.Instance.DownloadAssetSuccessEnter(conf);
    }
  }

  IEnumerator<object> downloadText(AssetConfig conf, uint tries = 0)
  {
    UnityWebRequest request = UnityWebRequestTexture.GetTexture(conf.url);
    yield return request.SendWebRequest();
    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    {
      if (conf.opts.retries < tries)
      {
        downloadText(conf, tries++);
      }
      else
      {
        AssetLoadEvent.Instance.DownloadAssetFailedEnter(conf);
      }
    }
    else
    {
      AssetManager.Instance.AddTextAsset(conf.key, request.downloadHandler.text);
      AssetLoadEvent.Instance.DownloadAssetSuccessEnter(conf);
    }
  }

  IEnumerator<object> downloadAudio(AssetConfig conf, uint tries = 0)
  {
    UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(conf.url, conf.opts.audioType ?? AudioType.UNKNOWN);
    yield return request.SendWebRequest();
    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    {
      if (conf.opts.retries < tries)
      {
        downloadText(conf, tries++);
      }
      else
      {
        AssetLoadEvent.Instance.DownloadAssetFailedEnter(conf);
      }
    }
    else
    {
      AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
      AssetManager.Instance.AddAudio(conf.key, clip);
      AssetLoadEvent.Instance.DownloadAssetSuccessEnter(conf);
    }
  }

  private void OnDestroy()
  {
    AssetLoadEvent.Instance.onDownloadAssetCompleted -= onDownloadCompleted;
    AssetLoadEvent.Instance.onDownloadAssetFailed -= onAssetFailed;
    AssetLoadEvent.Instance.onDownloadAssetSuccess -= onAssetSuccess;

    if (downloadEnumerator != null) StopCoroutine(downloadEnumerator);
    downloadEnumerator = null;
  }
}
