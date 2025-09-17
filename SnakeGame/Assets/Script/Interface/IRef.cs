using UnityEngine;

[System.Serializable]
public class IRef<T> : ISerializationCallbackReceiver where T : class
{
  public UnityEngine.Object target;
  public T I { get => target as T; }
  public static implicit operator bool(IRef<T> ir) => ir.target != null;
  void OnValidate()
  {
    if (!(target is T))
    {
      if (target is GameObject go)
      {
        target = null;
        foreach (Component c in go.GetComponents<Component>())
        {
          if (c is T)
          {
            target = c;
            break;
          }
        }
      }
    }
  }

  void ISerializationCallbackReceiver.OnBeforeSerialize() => OnValidate();
  void ISerializationCallbackReceiver.OnAfterDeserialize() { }
}