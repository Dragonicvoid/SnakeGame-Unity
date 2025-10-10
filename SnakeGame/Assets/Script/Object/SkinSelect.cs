
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
  RectTransform? selectTab = null;

  [SerializeField]
  RectTransform? selectItem = null;

  [SerializeField]
  TextAsset? JsonTex = null;

  Coroutine? selectTabAnimCour = null;

  Coroutine? selectItemAnimCour = null;

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
      if (primarySkinPrev.SkinData != null) onItemSel(primarySkinPrev.SkinData.id, false);

      RectTransform rect = primarySkinPrev.GetComponent<RectTransform>();
      animateSelectTab(rect.anchoredPosition);
    }
    else
    {
      primarySkinPrev.IsSelected = false;
      secondSkinPrev.IsSelected = true;
      if (secondSkinPrev.SkinData != null) onItemSel(secondSkinPrev.SkinData.id, false);

      RectTransform rect = secondSkinPrev.GetComponent<RectTransform>();
      animateSelectTab(rect.anchoredPosition);
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

    if (selectedSkin)
    {
      RectTransform rect = selectedSkin.GetComponent<RectTransform>();
      animateSelectItem(rect.anchoredPosition);
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

  void animateSelectTab(Vector2 target)
  {
    if (!selectTab) return;

    if (selectTabAnimCour != null)
    {
      StopCoroutine(selectTabAnimCour);
    }

    Vector2 startPos = new Vector2(selectTab.anchoredPosition.x, selectTab.anchoredPosition.y);
    target.Set(startPos.x, target.y);

    BaseTween<object> tweenData = new BaseTween<object>(
      0.3f,
      null,
      (dist, _) =>
      {
        selectTab.anchoredPosition = startPos;
      },
      (dist, _) =>
      {
        float outDist = Util.EaseOut(dist, 3);
        Vector2 delta = (target - startPos) * outDist;
        selectTab.anchoredPosition = startPos + delta;
      },
      (dist, _) =>
      {
        selectTab.anchoredPosition = target;
      }
    );

    IEnumerator<object> tween = Tween.Create(tweenData);
    selectTabAnimCour = StartCoroutine(tween);
  }

  void animateSelectItem(Vector2 target)
  {
    if (!selectItem) return;
    if (selectItemAnimCour != null)
    {
      StopCoroutine(selectItemAnimCour);
    }

    Vector2 startPos = new Vector2(selectItem.anchoredPosition.x, selectItem.anchoredPosition.y);

    BaseTween<object> tweenData = new BaseTween<object>(
      0.3f,
      null,
      (dist, _) =>
      {
        selectItem.anchoredPosition = startPos;
      },
      (dist, _) =>
      {
        float outDist = Util.EaseOut(dist, 3);
        Vector2 delta = (target - startPos) * outDist;
        selectItem.anchoredPosition = startPos + delta;
      },
      (dist, _) =>
      {
        selectItem.anchoredPosition = target;
      }
    );

    IEnumerator<object> tween = Tween.Create(tweenData);
    selectItemAnimCour = StartCoroutine(tween);
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
