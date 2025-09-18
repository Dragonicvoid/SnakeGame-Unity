#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class CustomScollRect : MonoBehaviour
{
  struct ScrollData
  {
    public RectTransform RectTrans;
    public float Top;
    public float Bottom;
  }
  [SerializeField]
  float yMargin = 0f;
  [SerializeField]
  float padding = 0f;
  ScrollRect scrollRect;
  List<ScrollData>? items = null;

  void Start()
  {
    scrollRect = GetComponent<ScrollRect>();
    items = new List<ScrollData>();

    updateSize();

    scrollRect.onValueChanged.AddListener(OnScroll);
  }

  public void OnScroll(Vector2 delta)
  {
    UpdateVisibility();
  }

  public void AddItem(RectTransform trans)
  {

    RectTransform content = scrollRect.content;
    trans.SetParent(content.transform, false);
    float margin = items?.Count > 0 ? yMargin : 0;

    float targetY = content.rect.height + margin + (trans.rect.height * (1.0f - trans.pivot.y));
    trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, -targetY);

    ScrollData scrollData = new ScrollData
    {
      RectTrans = trans,
      Top = -(content.rect.height + margin),
      Bottom = -(content.rect.height + margin + trans.rect.height),
    };
    items?.Add(scrollData);

    updateSize();
  }

  public void UpdateVisibility()
  {
    items?.ForEach((i) =>
    {
      i.RectTrans.gameObject.SetActive(isInsideContent(i));
    });
  }

  bool isInsideContent(ScrollData item)
  {
    RectTransform content = scrollRect.content;
    RectTransform scroll = scrollRect.GetComponent<RectTransform>();

    float topContent = -content.localPosition.y;
    float bottomContent = -(content.localPosition.y + scroll.rect.height);
    return !((item.Bottom > topContent && item.Top > topContent) || (item.Bottom < bottomContent && item.Top < bottomContent));
  }

  void updateSize()
  {
    float size = padding;

    int idx = 0;
    items?.ForEach((i) =>
    {
      size += (idx > 0 ? yMargin : 0) + i.RectTrans.rect.height * i.RectTrans.localScale.y;
      idx++;
    });

    size += padding;

    RectTransform content = scrollRect.content;
    content.sizeDelta = new Vector2(content.rect.width, size);
  }
}
