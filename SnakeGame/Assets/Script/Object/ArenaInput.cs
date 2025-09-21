using UnityEngine;
using UnityEngine.EventSystems;

public class ArenaInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
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

  public void OnDrag(PointerEventData eventData)
  {
    if (disableTouch) return;

    Vector2 uiLoc = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 0));

    GameplayMoveEvent.Instance.GameUiMoveTouch(uiLoc);
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if (disableTouch) return;

    Vector2 uiLoc = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 0));

    GameplayMoveEvent.Instance.GameUiStartTouch(uiLoc);
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    if (disableTouch) return;

    GameplayMoveEvent.Instance.GameUiEndTouch();
  }
}