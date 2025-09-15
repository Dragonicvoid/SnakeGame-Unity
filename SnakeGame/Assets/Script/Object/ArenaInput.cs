using UnityEngine;
using UnityEngine.EventSystems;

public class ArenaInput : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
  bool disableTouch = false;
  public void StartInputListener()
  {
    disableTouch = false;
  }

  public void StopInputListener()
  {
    disableTouch = true;
    GameplayMoveEvent.Instance.GameUiEndTouch();
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    if (disableTouch) return;

    Vector2 uiLoc = eventData.position;

    GameplayMoveEvent.Instance.GameUiStartTouch(uiLoc);
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (disableTouch) return;

    Vector2 uiLoc = eventData.position;

    GameplayMoveEvent.Instance.GameUiStartTouch(uiLoc);
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    if (disableTouch) return;

    GameplayMoveEvent.Instance.GameUiEndTouch();
  }
}