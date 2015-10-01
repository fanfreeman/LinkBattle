using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour {

    public int boardX;
    public int boardY;
    public bool isAtBottom;
    public bool isActivated = false;

    static int outOfTopEdgeY = 12;
    static int outOfBottomEdgeY = -12;

    public abstract string GetTypeString();

    ShaderSetUp shaderSetUpScript;

    void Awake()
    {
        shaderSetUpScript = GetComponent<ShaderSetUp>();
    }

    // 设置此单位的位置
    public void SetPositionValues(int x, int y)
    {
        boardX = x;
        boardY = y;
        Debug.Log("setting position to x: " + x + " y: " + y);
    }

    // 设置此单位在棋盘上半部分或下半部分
    public void SetIsAtBottom(bool val)
    {
        isAtBottom = val;
    }

    // 将此单位移动至新的位置
    public void MoveToPosition(int newX, int newY)
    {
        if (isAtBottom)
        {
            BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.BottomHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(this.gameObject, moveToPos, 1f);
        }
        else
        {
            BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.TopHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(this.gameObject, moveToPos, 1f);
        }

        boardX = newX;
        boardY = newY;
    }

    // 移动column高亮光柱
    void OnMouseEnter()
    {
        if (!GameManager.instance.canMove) return;
        ColumnCollider.MoveSelectorToColumn(boardX);
    }

    // 点击此单位时将其捡起
    void OnMouseDown()
    {
        if (!GameManager.instance.canMove) return;
        if (isAtBottom)
        {
            if (GameManager.instance.playersTurn)
            {
                // 如此单位在column底部，将其移出该column（捡起）
                if (BoardManager.instance.unitBeingPickedUp == null && !isActivated && BoardManager.instance.BottomHalf_CheckIfUnitIsAtTail(this))
                {
                    Vector3 moveToPos = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(boardX, boardY);
                    moveToPos.y = outOfBottomEdgeY;
                    iTween.MoveTo(gameObject, moveToPos, 1f);
                    BoardManager.instance.unitBeingPickedUp = this;
                    BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);

                    GameManager.instance.SetColumnHighlightEnabled(true, moveToPos.x);
                }
            }
            else Debug.Log("It's the top player's turn");
        }
        else
        {
            if (!GameManager.instance.playersTurn)
            {
                // 如此单位在column底部，将其移出该column（捡起）
                if (BoardManager.instance.unitBeingPickedUp == null && !isActivated && BoardManager.instance.TopHalf_CheckIfUnitIsAtTail(this))
                {
                    Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(boardX, boardY);
                    moveToPos.y = outOfTopEdgeY;
                    iTween.MoveTo(gameObject, moveToPos, 1f);
                    BoardManager.instance.unitBeingPickedUp = this;
                    BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);

                    GameManager.instance.SetColumnHighlightEnabled(true, moveToPos.x);
                }
            }
            else Debug.Log("It's the bottom player's turn");
        }
    }

    // 此单位开始蓄力
    public void Activate()
    {
        isActivated = true;
        Transform activationParticleObject = transform.Find("ActivationParticles");
        activationParticleObject.gameObject.SetActive(true);
        shaderSetUpScript.isMouseOverEffectEnabled = false;
    }
}
