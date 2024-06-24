using UnityEngine;

public enum DiretionType { Nothing, Top, Down, Left, Right };
public abstract class DirectionalOperationBase : OperationBase
{
    public DiretionType direction;

    protected Vector3 GetDirection()
    {
        return direction switch
        {
            DiretionType.Top => Vector3.up,
            DiretionType.Down => Vector3.down,
            DiretionType.Left => Vector3.left,
            DiretionType.Right => Vector3.right,
            _ => default,
        };
    }

    protected void OnChangeSide()
    {
        direction = direction switch
        {
            DiretionType.Top => DiretionType.Down,
            DiretionType.Down => DiretionType.Top,
            DiretionType.Left => DiretionType.Right,
            DiretionType.Right => DiretionType.Left,
            _ => default
        };
    }
}