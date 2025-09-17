#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkinSelectItem : MonoBehaviour, IPointerExitHandler
{
  [SerializeField]
  RawImage? preview = null;

  [SerializeField]
  Text? labelName = null;

  [SerializeField]
  GameObject? selectSprite = null;

  Material? mat = null;

  public SkinDetail? SkinData = null;

  float imageSize = 256f;

  public bool IsSelected = false;

  Texture2D? tex = null;

  public void SetSkinData(SkinDetail data)
  {
    SkinData = data;
    clearMem();
    setName();
    setImageUI();
  }

  void setName()
  {
    if (!labelName || SkinData == null) return;

    labelName.text = SkinData.name;
  }

  void setImageUI()
  {
    if (!preview || SkinData == null) return;

    if (!mat)
    {
      Shader shader = Shader.Find(SkinData.shader_name);
      mat = new Material(shader);
    }

    preview.material = mat;
    StartCoroutine(getTextureAndLoadImage());
  }

  IEnumerator<object> getTextureAndLoadImage()
  {

    ResourceRequest request = Resources.LoadAsync<Texture2D>(SkinData?.texture_name ?? "");

    while (!request.isDone)
    {
      yield return null;
    }
    Texture2D? loadedTexture = request.asset as Texture2D;

    if (loadedTexture != null)
    {
      setSpriteFrame(loadedTexture);
    }
    else
    {
      Debug.LogError("Failed to load asset at path: " + SkinData?.texture_name);
    }
  }

  void setSpriteFrame(Texture tex)
  {
    if (!preview || SkinData == null) return;

    if (preview.texture)
    {
      Texture temp = preview.texture;
      Destroy(temp);
    }
    preview.texture = tex;
  }


  void setBackground()
  {
    if (!selectSprite) return;

    selectSprite.SetActive(IsSelected);
  }

  void clearMem()
  {
    if (!Application.isEditor)
    {
      Destroy(mat);
      Destroy(tex);
    }
  }

  void OnDestroy()
  {
    clearMem();
  }

  public void OnPointerExit(PointerEventData _)
  {
    UiEvent.Instance.SkinSelected(
      SkinData?.id ?? 0
    );
  }
}
