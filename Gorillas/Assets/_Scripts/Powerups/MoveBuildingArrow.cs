using UnityEngine;

public class MoveBuildingArrow : MonoBehaviour
{
    private int _arrowIndex;

    public void SetArrowIndex(int index)
    {
        _arrowIndex = index;
    }

    private void OnMouseEnter()
    {
        LevelManager.Instance.SetLevelElementMovementIndex(_arrowIndex);
    }

    private void OnMouseExit()
    {
        LevelManager.Instance.SetLevelElementMovementIndex(-1);
    }
}
