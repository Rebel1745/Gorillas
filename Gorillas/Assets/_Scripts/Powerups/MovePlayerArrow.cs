using UnityEngine;

public class MovePlayerArrow : MonoBehaviour
{
    private int _arrowIndex;

    public void SetArrowIndex(int index)
    {
        _arrowIndex = index;
    }

    private void OnMouseEnter()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.SetPlayerMovementSprite(_arrowIndex);
    }

    private void OnMouseExit()
    {
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.HidePlayerMovementSprite();
    }
}
