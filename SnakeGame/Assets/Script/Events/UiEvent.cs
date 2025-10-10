using System;
using UnityEngine;

public class UiEvent : MonoBehaviour
{
    public static UiEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<int, bool>? onSkinSelected;
    public void SkinSelected(int skinId, bool updateData)
    {
        if (onSkinSelected != null)
            onSkinSelected(skinId, updateData);
    }

    public event Action<int>? onTabSkinSelect;
    public void TabSkinSelect(int tabId)
    {
        if (onTabSkinSelect != null)
            onTabSkinSelect(tabId);
    }

    public event Action? onPrevSkinDoneRender;
    public void PrevSkinDoneRender()
    {
        if (onPrevSkinDoneRender != null)
            onPrevSkinDoneRender();
    }

    public event Action? onGameStartAnimFinish;
    public void GameStartAnimFinish()
    {
        if (onGameStartAnimFinish != null)
            onGameStartAnimFinish();
    }

    public event Action? onGameEndAnimFinish;
    public void GameEndAnimFinish()
    {
        if (onGameEndAnimFinish != null)
            onGameEndAnimFinish();
    }

    public event Action? onCameraMoveFinish;
    public void CameraMoveFinish()
    {
        if (onCameraMoveFinish != null)
            onCameraMoveFinish();
    }

    public event Action<Vortex>? onVortexComplete;
    public void VortexComplete(Vortex vortex)
    {
        if (onVortexComplete != null)
            onVortexComplete(vortex);
    }

    public event Action? onMainPlayerVortexSpawn;
    public void MainPlayerVortexSpawn()
    {
        if (onMainPlayerVortexSpawn != null)
            onMainPlayerVortexSpawn();
    }

    public event Action? onEnemyVortexSpawn;
    public void EnemyVortexSpawn()
    {
        if (onEnemyVortexSpawn != null)
            onEnemyVortexSpawn();
    }

    public event Action? onSpikeAnimationComplete;
    public void SpikeAnimationComplete()
    {
        if (onSpikeAnimationComplete != null)
            onSpikeAnimationComplete();
    }
}
