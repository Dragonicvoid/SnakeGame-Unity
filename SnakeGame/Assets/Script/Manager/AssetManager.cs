using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public Dictionary<string, Texture2D> assetsTexture = new Dictionary<string, Texture2D>();

    public Dictionary<string, string> assetsText = new Dictionary<string, string>();

    public static AssetManager Instance;

    private void Awake()
    {
        Instance = this;
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
