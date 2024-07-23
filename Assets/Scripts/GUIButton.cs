using UnityEditor;
using UnityEngine;

public abstract class GUIButton<T> : ISelected 
    where T : IID
{
    public abstract T Element { get; }
    public abstract int GetID {  get; }
    public abstract Vector3 Position { get; }
    public virtual bool IsSelect { get; }
    public virtual bool IsPreselect { get; }
    public abstract Vector2 Size { get; }
    public abstract void HandleUpdatePosition(Vector3 pos);
    public abstract void HandleUpdatePositionDelta(Vector3 delta);
    public abstract void UpdatePosition();
    public abstract void UpdateButtonSize();
    public abstract void UpdateID();
    public abstract bool SetData(T element);

    public virtual void Select()
    {

    }
    public virtual void Preselect()
    {

    }
    public Vector2 GetScreenPosition()
    {
        return HandleUtility.GUIPointToScreenPixelCoordinate(HandleUtility.WorldToGUIPoint(Position));
    }
}
