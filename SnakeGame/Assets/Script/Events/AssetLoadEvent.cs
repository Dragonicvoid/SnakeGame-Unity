using System;
using UnityEngine;

public class AssetLoadEvent : MonoBehaviour
{
    public static AssetLoadEvent Instance;

    private void Awake()
    {
        Instance = this;
    }

    public event Action<AssetConfig> onDownloadAssetSuccess;
    public void DownloadAssetSuccessEnter(AssetConfig conf)
    {
        if (onDownloadAssetSuccess != null)
            onDownloadAssetSuccess(conf);
    }

    public event Action<AssetConfig> onDownloadAssetFailed;
    public void DownloadAssetFailedEnter(AssetConfig conf)
    {
        if (onDownloadAssetFailed != null)
            onDownloadAssetFailed(conf);
    }

    public event Action onDownloadAssetCompleted;
    public void DownloadAssetCompletedEnter()
    {
        if (onDownloadAssetCompleted != null)
            onDownloadAssetCompleted();
    }
}
