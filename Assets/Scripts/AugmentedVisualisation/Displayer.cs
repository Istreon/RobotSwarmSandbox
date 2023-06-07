using UnityEngine;

public abstract class Displayer : MonoBehaviour
{
    public abstract void DisplayVisual(LogClipFrame frame);
    public abstract void ClearVisual();
}
