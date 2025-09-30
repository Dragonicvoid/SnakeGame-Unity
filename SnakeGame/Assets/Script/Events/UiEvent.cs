using System;
using UnityEngine;

public class UiEvent : MonoBehaviour
{
    public static UiEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<int, bool> onSkinSelected;
    public void SkinSelected(int skinId, bool updateData)
    {
        if (onSkinSelected != null)
            onSkinSelected(skinId, updateData);
    }

    public event Action<int> onTabSkinSelect;
    public void TabSkinSelect(int tabId)
    {
        if (onTabSkinSelect != null)
            onTabSkinSelect(tabId);
    }

    public event Action onPrevSkinDoneRender;
    public void PrevSkinDoneRender()
    {
        if (onPrevSkinDoneRender != null)
            onPrevSkinDoneRender();
    }

    public event Action onGameStartAnimFinish;
    public void GameStartAnimFinish()
    {
        if (onGameStartAnimFinish != null)
            onGameStartAnimFinish();
    }

    public event Action onGameEndAnimFinish;
    public void GameEndAnimFinish()
    {
        if (onGameEndAnimFinish != null)
            onGameEndAnimFinish();
    }
}
