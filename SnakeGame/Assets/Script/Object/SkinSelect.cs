#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelect : MonoBehaviour
{
  struct InstantiateData
  {
    public GameObject GameObj;
    public SkinSelectItem Item;
  }

  [SerializeField]
  GameObject? skinPref = null;

  [SerializeField]
  StartSnakePrev? snakePrev = null;

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
      UiEvent.Instance.onPrevSkinDoneRender += onPrevSkinDoneRender;
    }
    InitSkinSelect();
  }

  void onPrevSkinDoneRender()
  {
    UiEvent.Instance.onPrevSkinDoneRender -= onPrevSkinDoneRender;
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
      SkinList text = JsonUtility.FromJson<SkinList>(JsonTex.text);

      foreach (SkinDetail skin in text.skins)
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
    UiEvent.Instance.onSkinSelected += onItemSel;
  }

  private void turnOffListener()
  {
    UiEvent.Instance.onSkinSelected -= onItemSel;
  }

  private void selectDefault()
  {
    UiEvent.Instance.SkinSelected(2001);
  }

  private void onItemSel(int id)
  {
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

    if (selectedSkin?.SkinData == null || !snakePrev) return;

    snakePrev.SkinData = selectedSkin.SkinData;
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
      Skin = snakePrev?.SkinData ?? new SkinDetail(),
      Type = snakePrev?.SnakeType ?? SNAKE_TYPE.NORMAL
    };
  }

  public PlayerSkin GetEnemySkinData()
  {
    SkinDetail? randomSkin = null;
    SNAKE_TYPE snakeType = SNAKE_TYPE.NORMAL;

    if (SkinList?.skins != null)
    {
      List<SkinDetail> skinArray = Util.DeepCopy(SkinList?.skins) ?? new List<SkinDetail>();
      List<SkinDetail> skins = Util.Filter(new List<SkinDetail>(skinArray), (skin) =>
      {
        return snakePrev?.SkinData?.id != null && skin.id != snakePrev?.SkinData?.id;
      });

      randomSkin = skins[Mathf.FloorToInt(UnityEngine.Random.Range(0, skins.Count))];
    }

    Array enumVal = Enum.GetValues(typeof(SNAKE_TYPE));
    int randIdx = Mathf.FloorToInt(UnityEngine.Random.Range(0, enumVal.Length));
    SNAKE_TYPE randomType = (SNAKE_TYPE)enumVal.GetValue(randIdx);

    return new PlayerSkin
    {
      Skin = randomSkin,
      Type = randomType,
    };
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
