#nullable enable
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

  public SkinList? SkinList = null;

  public List<SkinSelectItem> ItemList;

  GameObject? scrollViewContent = null;

  string jsonFilePath = "JSON/texList";

  void Start()
  {
    ItemList = new List<SkinSelectItem>();

    if (Application.isPlaying)
    {
      UiEvent.Instance.onPrevSkinDoneRender += onPrevSkinDoneRender;
    }
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
    ResourceRequest request = Resources.LoadAsync<Texture2D>(jsonFilePath);

    while (!request.isDone)
    {
      yield return null;
    }
    TextAsset? loadedJson = request.asset as TextAsset;

    if (loadedJson != null)
    {
      SkinList = JsonUtility.FromJson<SkinList>(loadedJson.ToString());

      foreach (SkinDetail skin in SkinList.skins)
      {
        if (scrollViewContent == null) break;

        InstantiateData? item = createPref();

        if (item == null) break;

        item.Value.Item.SetSkinData(skin);
        item.Value.GameObj.transform.SetParent(scrollViewContent.transform);
      }

      setListener();
      selectDefault();
    }
    else
    {
      Debug.LogError("Failed to load asset at path: " + jsonFilePath);
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
      SkinDetail[] skinArray = Util.DeepCopy(SkinList?.skins) ?? new SkinDetail[0];
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
