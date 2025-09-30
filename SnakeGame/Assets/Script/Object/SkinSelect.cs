#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelect : MonoBehaviour
{
  struct InstantiateData
  {
    public GameObject GameObj;
    public SkinSelectItem Item;
  }

  struct TabSelectData
  {
    public SkinSelectItem? Item;
    public bool IsPrimary;
  }

  [SerializeField]
  GameObject? skinPref = null;

  [SerializeField]
  StartSnakePrev? snakePrev = null;

  [SerializeField]
  SkinSelectItem? primarySkinPrev = null;

  [SerializeField]
  SkinSelectItem? secondSkinPrev = null;

  [SerializeField]
  CustomScollRect? customScroll = null;

  [SerializeField]
  TextAsset? JsonTex = null;

  public SkinList? SkinList = null;

  public List<SkinSelectItem> ItemList;

  void Start()
  {
    ItemList = new List<SkinSelectItem>();

    if (Application.isPlaying)
    {
      UiEvent.Instance.onPrevSkinDoneRender -= onPrevSkinDoneRender;
      UiEvent.Instance.onPrevSkinDoneRender += onPrevSkinDoneRender;
    }
    InitSkinSelect();
  }

  void onPrevSkinDoneRender()
  {
    UiEvent.Instance.onPrevSkinDoneRender -= onPrevSkinDoneRender;
  }

  void onTabSkinSelect(int id)
  {
    if (!primarySkinPrev || !secondSkinPrev) return;
    if (id == 0)
    {
      primarySkinPrev.IsSelected = true;
      secondSkinPrev.IsSelected = false;
      onItemSel(primarySkinPrev.SkinData.id, false);
    }
    else
    {
      primarySkinPrev.IsSelected = false;
      secondSkinPrev.IsSelected = true;
      onItemSel(secondSkinPrev.SkinData.id, false);
    }
  }

  public void InitSkinSelect()
  {
    StartCoroutine(getSkinJsonAndSetup());
  }

  IEnumerator<object> getSkinJsonAndSetup()
  {
    yield return null;

    if (JsonTex != null && JsonTex.text != "")
    {
      SkinList = JsonUtility.FromJson<SkinList>(JsonTex.text);

      foreach (SkinDetail skin in SkinList.skins)
      {
        if (customScroll == null) break;

        InstantiateData? item = createPref();

        if (item == null) break;

        item.Value.Item.SetSkinData(skin);
        customScroll?.AddItem(item.Value.GameObj.GetComponent<RectTransform>());
      }
      setListener();
      selectDefault();
    }
    else
    {
      Debug.LogError("Failed to load Texture Json:");
    }
  }

  private void setListener()
  {
    turnOffListener();
    UiEvent.Instance.onTabSkinSelect += onTabSkinSelect;
    UiEvent.Instance.onSkinSelected += onItemSel;
  }

  private void turnOffListener()
  {
    UiEvent.Instance.onTabSkinSelect -= onTabSkinSelect;
    UiEvent.Instance.onSkinSelected -= onItemSel;
  }

  private void selectDefault()
  {
    UiEvent.Instance.TabSkinSelect(1);
    UiEvent.Instance.SkinSelected(1001, true);

    UiEvent.Instance.TabSkinSelect(0);
    UiEvent.Instance.SkinSelected(1001, true);
  }

  private void onItemSel(int id, bool updateData = true)
  {
    TabSelectData? tabData = getSelectedTab();

    if (tabData == null) return;

    SkinSelectItem? selectedSkin = null;
    foreach (SkinSelectItem item in ItemList)
    {
      if (item.IsSelected)
      {
        item.IsSelected = false;
      }

      if (item.SkinData?.id == id)
      {
        item.IsSelected = true;
        selectedSkin = item;
      }
    }

    if (selectedSkin?.SkinData == null || !snakePrev || !updateData) return;

    tabData.Value.Item?.SetSkinData(selectedSkin.SkinData);
    snakePrev.SetSnakeSkin(selectedSkin.SkinData, tabData.Value.IsPrimary);
  }

  InstantiateData? createPref()
  {
    if (!skinPref) return null;

    GameObject gameObj = Instantiate(skinPref);
    SkinSelectItem selectSkinItem = gameObj.GetComponent<SkinSelectItem>();

    if (!selectSkinItem)
    {
      Destroy(gameObj);
      return null;
    }

    ItemList.Add(selectSkinItem);
    return new InstantiateData { GameObj = gameObj, Item = selectSkinItem };
  }

  public PlayerSkin GetPlayerSkinData()
  {
    return new PlayerSkin
    {
      SkinPrimary = snakePrev?.SkinDataPrim ?? new SkinDetail(),
      SkinSecond = snakePrev?.SkinDataSecond ?? new SkinDetail(),
      Type = snakePrev?.SnakeType ?? SNAKE_TYPE.NORMAL
    };
  }

  public PlayerSkin GetEnemySkinData()
  {
    SkinDetail? randomPrimSkin = null;

    if (SkinList?.skins != null)
    {
      List<SkinDetail> skinArray = new List<SkinDetail>(SkinList?.skins);
      List<SkinDetail> skins = Util.Filter(new List<SkinDetail>(skinArray), (skin) =>
      {
        return snakePrev?.SkinDataPrim?.id != null && skin.id != snakePrev?.SkinDataPrim?.id;
      });

      randomPrimSkin = skins[Mathf.FloorToInt(UnityEngine.Random.Range(0, skins.Count))];
    }

    SkinDetail? randomSecondSkin = null;

    if (SkinList?.skins != null)
    {
      List<SkinDetail> skinArray = new List<SkinDetail>(SkinList?.skins);
      List<SkinDetail> skins = Util.Filter(new List<SkinDetail>(skinArray), (skin) =>
      {
        return snakePrev?.SkinDataSecond?.id != null && skin.id != snakePrev?.SkinDataSecond?.id;
      });

      randomSecondSkin = skins[Mathf.FloorToInt(UnityEngine.Random.Range(0, skins.Count))];
    }

    Array enumVal = Enum.GetValues(typeof(SNAKE_TYPE));
    int randIdx = Mathf.FloorToInt(UnityEngine.Random.Range(0, enumVal.Length));
    SNAKE_TYPE randomType = (SNAKE_TYPE)enumVal.GetValue(randIdx);

    return new PlayerSkin
    {
      SkinPrimary = randomPrimSkin,
      SkinSecond = randomSecondSkin,
      Type = randomType,
    };
  }

  TabSelectData? getSelectedTab()
  {
    if (primarySkinPrev?.IsSelected == true)
    {
      return new TabSelectData
      {
        Item = primarySkinPrev,
        IsPrimary = true,
      };
    }
    else if (secondSkinPrev?.IsSelected == true)
    {
      return new TabSelectData
      {
        Item = secondSkinPrev,
        IsPrimary = false,
      };
    }

    return null;
  }

  public void SelectTabPrimary()
  {
    UiEvent.Instance.TabSkinSelect(0);
  }

  public void SelectTabSecond()
  {
    UiEvent.Instance.TabSkinSelect(1);
  }

  void OnEnable()
  {
    setListener();
  }
  void OnDisable()
  {
    turnOffListener();
  }

  void OnDestroy()
  {
    turnOffListener();
  }
}
