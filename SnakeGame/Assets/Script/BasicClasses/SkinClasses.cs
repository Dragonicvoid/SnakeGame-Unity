#nullable enable
using System;

[Serializable]
public class SkinList
{
    public SkinDetail[]? skins { set; get; }
}

[Serializable]
public class SkinDetail
{
    public int? id;
    public string? name;
    public string? shader_name;
    public string? texture_name;
}