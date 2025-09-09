using System;
using System.Collections.Generic;

public class SkinList
{
    public List<SkinDetail> Skins { set; get; }
}

[Serializable]
public class SkinDetail
{
    public int? id;
    public string? name;
    public string? effect_name;
    public string? sprite_frame;
    public string? defines;

    public string? effect_code;
}