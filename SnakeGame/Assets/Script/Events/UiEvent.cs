using System;
using UnityEngine;

public class UiEvent : MonoBehaviour
{
    public static UiEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<int> onSkinSelected;
    public void SkinSelected(int skinId)
    {
        if (onSkinSelected != null)
            onSkinSelected(skinId);
    }

    public event Action onPrevSkinDoneRender;
    public void PrevSkinDoneRender()
    {
        if (onPrevSkinDoneRender != null)
            onPrevSkinDoneRender();
    }
}
