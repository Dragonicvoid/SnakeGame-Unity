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
    List<AssetConfig> assets = new List<AssetConfig>() {
            new AssetConfig {
                url = "https://res.cloudinary.com/dyfgknhce/image/upload/v1759574990/Snake/Skins/slime_normal_isqsoc.jpg",
                key = "SKIN_WITHER_NORMAL",
                type = ASSET_TYPE.IMAGE,
                opts = new DownloadOpts(),
            },
            new AssetConfig {
                url = "https://res.cloudinary.com/dyfgknhce/image/upload/v1759575035/Snake/Skins/bubble_kblnw4.png",
                key = "SKIN_BUBBLE_MAIN",
                type = ASSET_TYPE.IMAGE,
                opts = new DownloadOpts {
                  retries = 4,
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
    if (request.isNetworkError || request.isHttpError)
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
    if (request.isNetworkError || request.isHttpError)
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

  private void OnDestroy()
  {
    AssetLoadEvent.Instance.onDownloadAssetCompleted -= onDownloadCompleted;
    AssetLoadEvent.Instance.onDownloadAssetFailed -= onAssetFailed;
    AssetLoadEvent.Instance.onDownloadAssetSuccess -= onAssetSuccess;

    if (downloadEnumerator != null) StopCoroutine(downloadEnumerator);
    downloadEnumerator = null;
  }
}
