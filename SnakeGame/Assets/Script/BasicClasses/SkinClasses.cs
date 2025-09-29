using System;
using System.Collections.Generic;

[Serializable]
public class SkinList
{
    public List<SkinDetail> skins;
}

[Serializable]
public class SkinDetail
{
    public int id;
    public string name;
    public string shader_name;
    public string texture_name;
    public float main_size;
    public float second_size;
}