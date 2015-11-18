using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardUtils : MonoBehaviour {

    BoardManager boardManager;
    List<Unit> listOfHorizontalUnits;

    void Start()
    {
        boardManager = GetComponent<BoardManager>();

        listOfHorizontalUnits = new List<Unit>();
    }

    // 找到指定单位左边紧贴着的单位
    //Unit TopHalf_GetUnitOnTheLeftSideOfUnit(Unit unit)
    //{
    //    if (unit.boardX == 0) return null;
    //    return TopHalf_GetUnitAtPosition(unit.boardX - 1, unit.boardY);
    //}

    //// 找到指定单位左边紧贴着的单位
    //Unit BottomHalf_GetUnitOnTheLeftSideOfUnit(Unit unit)
    //{
    //    if (unit.boardX == 0) return null;
    //    return BottomHalf_GetUnitAtPosition(unit.boardX - 1, unit.boardY);
    //}

    //// 找到指定单位右边紧贴着的单位
    //Unit TopHalf_GetUnitOnTheRightSideOfUnit(Unit unit)
    //{
    //    if (unit.boardX == numColumns - 1) return null;
    //    return TopHalf_GetUnitAtPosition(unit.boardX + 1, unit.boardY);
    //}

    //// 找到指定单位右边紧贴着的单位
    //Unit BottomHalf_GetUnitOnTheRightSideOfUnit(Unit unit)
    //{
    //    if (unit.boardX == numColumns - 1) return null;
    //    return BottomHalf_GetUnitAtPosition(unit.boardX + 1, unit.boardY);
    //}

    bool CheckIfUnitQualifiesForBarricade(Unit otherUnit, Unit originalUnit)
    {
        if (originalUnit == null || otherUnit == null) return false;

        // 确保单位为同一兵种
        if (!originalUnit.GetTypeString().Equals(otherUnit.GetTypeString())) return false;

        // 确保单位都没有在蓄力或已变成路障  没失控
        if (originalUnit.isActivated || otherUnit.isActivated ||
            originalUnit.isBarricade || otherUnit.isBarricade||
        originalUnit.isLossControll || otherUnit.isLossControll ) return false;

        return true;
    }

    void TurnUnitsIntoBarricade(List<Unit> listOfUnits)
    {
        float healthMax = listOfUnits[0].healthMax;
        healthMax *= 2;

        for (int i = 0; i < listOfUnits.Count; i++)
        {
            listOfUnits[i].TurnIntoBarricade(healthMax);
        }
    }

    // 检查指定单位是否横着与其它单位连成墙
    public bool TopHalf_CheckBarricadeFormation(Unit unit)
    {
        if (CheckIfUnitQualifiesForBarricade(unit, unit))
        {
            listOfHorizontalUnits.Clear();
            listOfHorizontalUnits.Add(unit);

            int currentX = unit.boardX - 1;
            while (currentX >= 0)
            {
                Unit unitOnTheLeft = boardManager.TopHalf_GetUnitAtPosition(currentX, unit.boardY);
                if (CheckIfUnitQualifiesForBarricade(unitOnTheLeft, unit)) listOfHorizontalUnits.Add(unitOnTheLeft);
                else break;
                currentX--;
            }

            currentX = unit.boardX + 1;
            while (currentX <= boardManager.numColumns - 1)
            {
                Unit unitOnTheRight = boardManager.TopHalf_GetUnitAtPosition(currentX, unit.boardY);
                if (CheckIfUnitQualifiesForBarricade(unitOnTheRight, unit)) listOfHorizontalUnits.Add(unitOnTheRight);
                else break;
                currentX++;
            }

            if (listOfHorizontalUnits.Count >= 3)
            {
                TurnUnitsIntoBarricade(listOfHorizontalUnits); // 将连起来的单位变成路障
                return true;
            }
        }

        return false;
    }

    // 检查指定单位是否横着与其它单位连成墙
    public bool BottomHalf_CheckBarricadeFormation(Unit unit)
    {
        if (CheckIfUnitQualifiesForBarricade(unit, unit))
        {
            listOfHorizontalUnits.Clear();
            listOfHorizontalUnits.Add(unit);

            int currentX = unit.boardX - 1;
            while (currentX >= 0)
            {
                Unit unitOnTheLeft = boardManager.BottomHalf_GetUnitAtPosition(currentX, unit.boardY);
                if (CheckIfUnitQualifiesForBarricade(unitOnTheLeft, unit)) listOfHorizontalUnits.Add(unitOnTheLeft);
                else break;
                currentX--;
            }

            currentX = unit.boardX + 1;
            while (currentX <= boardManager.numColumns - 1)
            {
                Unit unitOnTheRight = boardManager.BottomHalf_GetUnitAtPosition(currentX, unit.boardY);
                if (CheckIfUnitQualifiesForBarricade(unitOnTheRight, unit)) listOfHorizontalUnits.Add(unitOnTheRight);
                else break;
                currentX++;
            }

            if (listOfHorizontalUnits.Count >= 3)
            {
                TurnUnitsIntoBarricade(listOfHorizontalUnits); // 将连起来的单位变成路障
                return true;
            }
        }

        return false;
    }
}
