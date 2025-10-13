using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public Dictionary<string, Texture2D> assetsTexture = new Dictionary<string, Texture2D>();

    public Dictionary<string, string> assetsText = new Dictionary<string, string>();

    public static AssetManager _instance;

    public static AssetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AssetManager();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    public void AddTexture(string key, Texture2D tex)
    {
        assetsTexture.Add(key, tex);
    }

    public void AddTextAsset(string key, string text)
    {
        assetsText.Add(key, text);
    }

    public void Remove(string key)
    {
        assetsTexture.Remove(key);
    }
}
