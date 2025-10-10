public enum ASSET_TYPE
{
    IMAGE,
    TEXT,
}

public struct AssetConfig
{
    public string url { get; set; }
    public string key { get; set; }
    public ASSET_TYPE type { get; set; }
    public DownloadOpts opts { get; set; }
}

public struct DownloadOpts
{
    public uint retries { get; set; }
}