using UnityEngine;
using System.Collections;

public class ColumnCollider : MonoBehaviour
{
    public int columnNumber;

    public static void MoveSelectorToColumn(int col)
    {
        // 移动column highlight
        Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(col, 0);
        moveToPos.y = 0;
        iTween.MoveTo(GameManager.instance.columnHighlight, moveToPos, 0.5f);

        // 根据回合选定selector箭头
        GameObject selector;
        if (GameManager.instance.playersTurn) selector = GameManager.instance.selectorBottom;
        else selector = GameManager.instance.selectorTop;

        // 移动selector
        Vector3 pos = selector.transform.position;
        pos.x = moveToPos.x;
        iTween.MoveTo(selector, pos, 0.5f);
    }

    void OnMouseEnter()
    {
        if (!GameManager.instance.canMove) return;
        MoveSelectorToColumn(columnNumber);
    }

    void OnMouseDown()
    {
        if (!GameManager.instance.canMove) return;
        if (BoardManager.instance.unitBeingPickedUp != null) // 如果有单位被玩家捡起，将其放置在选定的column尾部
        {
            if (GameManager.instance.playersTurn)
            {
                if (BoardManager.instance.BottomHalf_LetUnitEnterColumnFromTail(BoardManager.instance.unitBeingPickedUp, columnNumber))
                {
                    BoardManager.instance.unitBeingPickedUp = null;
                    GameManager.instance.SetColumnHighlightEnabled(false, 0f);
                    GameManager.instance.UseOneMove();
                }
            }
            else
            {
                if (BoardManager.instance.TopHalf_LetUnitEnterColumnFromTail(BoardManager.instance.unitBeingPickedUp, columnNumber))
                {
                    BoardManager.instance.unitBeingPickedUp = null;
                    GameManager.instance.SetColumnHighlightEnabled(false, 0f);
                    GameManager.instance.UseOneMove();
                }
            }
        }
    }
}
