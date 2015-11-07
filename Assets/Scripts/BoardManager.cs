using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * 术语定义：
 * TopHalf 上半棋盘
 * BottomHalf 下半棋盘
 * Column 某一列
 * Row 某一行
 * Position 单位/格位在某半边棋盘上的xy坐标，x = 0 ~ numColumns - 1, y = 0 ~ numRowsPerSide - 1, 与在屏幕上的像素位置无关
 * Coordinates 单位/格位在屏幕上的像素位置
 * Head 某一列的头部的格位
 * Tail 某一列的尾部的格位
 * Move 步数，每回合可以走三步
 */
public class BoardManager : Photon.MonoBehaviour {

    public static BoardManager instance = null;

    // 兵种
    public GameObject[] unitPrefabs;

    // 棋盘每一边的大小
    public int numColumns = 8;
    public int numRowsPerSide = 6;

    // 玩家捡起的单位
    public Unit unitBeingPickedUp = null;

    // 空档格位的xy坐标列表
    private List<Vector3> gridCoordinatesTop = new List<Vector3>();
    private List<Vector3> gridCoordinatesBottom = new List<Vector3>();

    // 存储所有单位的数组matrix
    private List<Unit> unitGridTop;
    private List<Unit> unitGridBottom;

    // 兵种种类列表
    public enum UnitTypes : int {
        Archer = 0,
        Knight = 1
    };

    // 初始化棋盘
    public void InitBoard()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        SetUpGrids();
        InitializeGridCoordinates();
    }

    // 给棋盘随机摆兵
    public void SetupRandomBoardState() {
        LayoutObjectAtRandom(true, unitPrefabs, 20, 20); // top
        LayoutObjectAtRandom(false, unitPrefabs, 20, 20); // bottom

        PrintGrids();

        // send board state to other player
        SendBoardState();

        StartCoroutine(TopHalf_ConsolidateUnits());
        StartCoroutine(BottomHalf_ConsolidateUnits(GameManager.instance, "GoToNextTurn"));
    }

    void SetUpGrids() {
        // 初始化棋盘上半部分的grid为null
        unitGridTop = new List<Unit>(numColumns * numRowsPerSide);
        for (int i = 0; i < numColumns * numRowsPerSide; i++)
        {
            unitGridTop.Add(null);
        }

        // 初始化棋盘下半部分的grid为null
        unitGridBottom = new List<Unit>(numColumns * numRowsPerSide);
        for (int i = 0; i < numColumns * numRowsPerSide; i++)
        {
            unitGridBottom.Add(null);
        }
    }

    /**
     * 定义每个格位的xy坐标
     */
    void InitializeGridCoordinates()
    {
        // top half
        for (int y = 0; y < numRowsPerSide; y++) {
            for (int x = 0; x < numColumns; x++) {
                gridCoordinatesTop.Add(new Vector3(x - numColumns / 2 + 0.5f, y + 1, 0f));
            }
        }
        
        // bottom half
        for (int y = numRowsPerSide - 1; y >= 0; y--) {
            for (int x = 0; x < numColumns; x++) {
                gridCoordinatesBottom.Add(new Vector3(x - numColumns / 2 + 0.5f, y - 7, 0f));
            }
        }
    }

    // 在上半棋盘随机找个格位
    int TopHalf_RandomPosition() {
        int randomIndex = Random.Range(0, unitGridTop.Count);
        Unit unit = unitGridTop[randomIndex];
        while (unit != null)
        {
            randomIndex = Random.Range(0, unitGridTop.Count);
            unit = unitGridTop[randomIndex];
        }
        return randomIndex;
    }

    // 在下半棋盘随机找个格位
    int BottomHalf_RandomPosition() {
        int randomIndex = Random.Range(0, unitGridBottom.Count);
        Unit unit = unitGridBottom[randomIndex];
        while (unit != null)
        {
            randomIndex = Random.Range(0, unitGridBottom.Count);
            unit = unitGridBottom[randomIndex];
        }
        return randomIndex;
    }

    // 从回收的池子中补兵
    public void CallReserveUnits(List<Unit> listOfReserveUnits, bool isBottomHalf)
    {
        int objectCount = listOfReserveUnits.Count;
        for (int i = 0; i < objectCount; i++)  {
            Unit reserveUnit = listOfReserveUnits[i];
            if (isBottomHalf)
            {
                while (!BottomHalf_LetUnitEnterColumnFromTail(reserveUnit, Random.Range(0, numColumns)))
                {
                    // 如果没成功（放不下），就再来一次
                }
            }
            else
            {
                while (!TopHalf_LetUnitEnterColumnFromTail(reserveUnit, Random.Range(0, numColumns)))
                {
                    // 如果没成功（放不下），就再来一次
                }
            }
        }
    }

    // 随机找空格位放置单位
    void LayoutObjectAtRandom(bool isTopHalf, GameObject[] unitPrefabs, int minimum, int maximum) {
        int objectCount = Random.Range(minimum, maximum + 1);
        for (int i = 0; i < objectCount; i++)  { // loop through creation count
            int randomIndex;
            if (isTopHalf)
            {
                randomIndex = TopHalf_RandomPosition();
                Vector3 coordinates = gridCoordinatesTop[randomIndex];
                var randomUnitIndex = Random.Range(0, unitPrefabs.Length);
                GameObject obj = Instantiate(unitPrefabs[randomUnitIndex], coordinates, Quaternion.identity) as GameObject;
                Vector2 position = GetPositionGivenArrayIndex(randomIndex);
                unitGridTop[randomIndex] = obj.GetComponent<Unit>();
                unitGridTop[randomIndex].SetPositionValues((int)position.x, (int)position.y);
                unitGridTop[randomIndex].SetIsAtBottom(false);
                unitGridTop[randomIndex].SetSortingLayer();
            }
            else
            {
                randomIndex = BottomHalf_RandomPosition();
                Vector3 coordinates = gridCoordinatesBottom[randomIndex];
                var randomUnitIndex = Random.Range(0, unitPrefabs.Length);
                GameObject obj = Instantiate(unitPrefabs[randomUnitIndex], coordinates, Quaternion.identity) as GameObject;
                Vector2 position = GetPositionGivenArrayIndex(randomIndex);
                unitGridBottom[randomIndex] = obj.GetComponent<Unit>();
                unitGridBottom[randomIndex].SetPositionValues((int)position.x, (int)position.y);
                unitGridBottom[randomIndex].SetIsAtBottom(true);
                unitGridBottom[randomIndex].SetSortingLayer();
            }
        }
    }

    // 提供xy，获得该格位的单位（或null）
    Unit TopHalf_GetUnitAtPosition(int x, int y)
    {
        return unitGridTop[y * numColumns + x];
    }

    // 提供xy，获得该格位的单位（或null）
    Unit BottomHalf_GetUnitAtPosition(int x, int y)
    {
        return unitGridBottom[y * numColumns + x];
    }

    // 提供单位及xy，将该单位放置在此格位
    public void TopHalf_SetUnitAtPosition(Unit unit, int x, int y)
    {
        unitGridTop[y * numColumns + x] = unit;
    }

    // 提供单位及xy，将该单位放置在此格位
    public void BottomHalf_SetUnitAtPosition(Unit unit, int x, int y)
    {
        unitGridBottom[y * numColumns + x] = unit;
    }

    // 提供xy，获得该格位的screen coordinates
    public Vector3 TopHalf_GetCoordinatesAtPosition(int x, int y)
    {
        return gridCoordinatesTop[y * numColumns + x];
    }

    // 提供xy，获得该格位的screen coordinates
    public Vector3 BottomHalf_GetCoordinatesAtPosition(int x, int y)
    {
        return gridCoordinatesBottom[y * numColumns + x];
    }

    // 提供array index，获得该格位的xy（上下部分通用）
    Vector2 GetPositionGivenArrayIndex(int index)
    {
        int x = index % numColumns;
        int y = index / numColumns;
        return new Vector2(x, y);
    }

    // 将上半部分所有单位往下推
    IEnumerator TopHalf_ConsolidateUnits(MonoBehaviour callbackScriptInstance = null, string callbackMethod = null)
    {
        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往下推
        {
            for (int x = 0; x < numColumns; x++)
            {
                Unit unit = TopHalf_GetUnitAtPosition(x, y);
                if (unit != null)
                {
                    int newY = y - 1;
                    while (newY >= 0)
                    {
                        if (TopHalf_GetUnitAtPosition(x, newY) == null) // 往前一行有空位
                        {
                            unit.MoveToPosition(x, newY, true);
                            yield return null;
                        }
                        else
                        {
                            TopHalf_CheckFormationForUnit(unit);
                            break; // 如果往前一行没有空位，则再往前也不会有空位了
                        }
                        newY--;
                    }
                }

                yield return null;
            }
        }

        if (callbackScriptInstance != null) callbackScriptInstance.SendMessage(callbackMethod);
    }

    // 将下半部分所有单位往上推
    IEnumerator BottomHalf_ConsolidateUnits(MonoBehaviour callbackScriptInstance = null, string callbackMethod = null)
    {
        Debug.Log("consolidating bottom half");

        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往上推
        {
            for (int x = 0; x < numColumns; x++)
            {
                Unit unit = BottomHalf_GetUnitAtPosition(x, y);
                if (unit != null)
                {
                    int newY = y - 1;
                    while (newY >= 0)
                    {
                        if (BottomHalf_GetUnitAtPosition(x, newY) == null) // 往前一行有空位
                        {
                            unit.MoveToPosition(x, newY, true);
                            yield return null;
                        }
                        else
                        {
                            BottomHalf_CheckFormationForUnit(unit);
                            break; // 如果往前一行没有空位，则再往前也不会有空位了
                        }
                        newY--;
                    }
                }

                yield return null;
            }
        }

        if (callbackScriptInstance != null) callbackScriptInstance.SendMessage(callbackMethod);
    }

    // 找到指定单位离队首更近一格的单位
    Unit TopHalf_GetUnitInFrontOfUnit(Unit unit)
    {
        if (unit.boardY == 0) return null;
        return TopHalf_GetUnitAtPosition(unit.boardX, unit.boardY - 1);
    }

    // 找到指定单位离队首更近一格的单位
    Unit BottomHalf_GetUnitInFrontOfUnit(Unit unit)
    {
        if (unit.boardY == 0) return null;
        return BottomHalf_GetUnitAtPosition(unit.boardX, unit.boardY - 1);
    }

    // 找到指定单位离队首更近两格的单位
    Unit TopHalf_GetUnitTwoInFrontOfUnit(Unit unit)
    {
        if (unit.boardY <= 1) return null;
        return TopHalf_GetUnitAtPosition(unit.boardX, unit.boardY - 2);
    }

    // 找到指定单位离队首更近两格的单位
    Unit BottomHalf_GetUnitTwoInFrontOfUnit(Unit unit)
    {
        if (unit.boardY <= 1) return null;
        return BottomHalf_GetUnitAtPosition(unit.boardX, unit.boardY - 2);
    }

    // 检测指定单位是否形成三连
    public void TopHalf_CheckFormationForUnit(Unit unit)
    {
        if (unit.boardY <= 1) return;
        Unit unitInFront = TopHalf_GetUnitInFrontOfUnit(unit);
        Unit unitTwoInFront = TopHalf_GetUnitTwoInFrontOfUnit(unit);
        if (unitInFront != null && unitTwoInFront != null)
        {
            // 确保三个单位为同一兵种
            if (unit.GetTypeString() != unitInFront.GetTypeString()) return;
            if (unit.GetTypeString() != unitTwoInFront.GetTypeString()) return;

            if (!unit.isActivated && !unitInFront.isActivated && !unitTwoInFront.isActivated)
            {
                unitTwoInFront.ActivateChargeUp(unitInFront, unit, true);
            }
        }
    }

    // 检测指定单位是否形成三连
    public void BottomHalf_CheckFormationForUnit(Unit unit)
    {
        if (unit.boardY <= 1) return;
        Unit unitInFront = BottomHalf_GetUnitInFrontOfUnit(unit);
        Unit unitTwoInFront = BottomHalf_GetUnitTwoInFrontOfUnit(unit);
        if (unitInFront != null && unitTwoInFront != null)
        {
            // 确保三个单位为同一兵种
            if (unit.GetTypeString() != unitInFront.GetTypeString()) return;
            if (unit.GetTypeString() != unitTwoInFront.GetTypeString()) return;

            if (!unit.isActivated && !unitInFront.isActivated && !unitTwoInFront.isActivated)
            {
                unitTwoInFront.ActivateChargeUp(unitInFront, unit, true); // 组成三连formation
            }
        }
    }

    // 检测指定单位是否在其所在column的最尾部
    public bool TopHalf_CheckIfUnitIsAtTail(Unit unit)
    {
        int unitX = unit.boardX;
        int unitY = unit.boardY;

        for (int y = unitY + 1; y < numRowsPerSide; y++)
        {
            if (TopHalf_GetUnitAtPosition(unitX, y) != null) // 下边的某一行有其它单位
            {
                return false;
            }
        }
        return true;
    }

    // 检测指定单位是否在其所在column的最尾部
    public bool BottomHalf_CheckIfUnitIsAtTail(Unit unit)
    {
        int unitX = unit.boardX;
        int unitY = unit.boardY;

        for (int y = unitY + 1; y < numRowsPerSide; y++)
        {
            if (BottomHalf_GetUnitAtPosition(unitX, y) != null) // 下边的某一行有其它单位
            {
                return false;
            }
        }
        return true;
    }

    // 找到某一列最靠头部的空格位
    public int TopHalf_FindEmptySpaceInColumnClosestToHead(int col)
    {
        for (int y = 0; y < numRowsPerSide; y++)
        {
            if (TopHalf_GetUnitAtPosition(col, y) == null) // 某一row没有其它单位
            {
                return y;
            }
        }

        return -1; // 该column没有空格位
    }

    // 找到某一列最靠头部的空格位
    public int BottomHalf_FindEmptySpaceInColumnClosestToHead(int col)
    {
        for (int y = 0; y < numRowsPerSide; y++)
        {
            if (BottomHalf_GetUnitAtPosition(col, y) == null) // 某一row没有其它单位
            {
                return y;
            }
        }

        return -1; // 该column没有空格位
    }

    // 让单位从column尾部走入该column
    public bool TopHalf_LetUnitEnterColumnFromTail(Unit unit, int columnNumber)
    {
        int y = BoardManager.instance.TopHalf_FindEmptySpaceInColumnClosestToHead(columnNumber);
        if (y != -1) // 选定的column有空格位
        {
            iTween.Stop(unit.gameObject);
            Vector3 moveToCoords = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(columnNumber, y);
            Vector3 currentCoords = unit.transform.position;
            currentCoords.x = moveToCoords.x;
            unit.transform.position = currentCoords;
            unit.MoveToPosition(columnNumber, y);
            TopHalf_CheckFormationForUnit(unit);
            return true;
        }

        return false;
    }

    // 让单位从column尾部走入该column
    public bool BottomHalf_LetUnitEnterColumnFromTail(Unit unit, int columnNumber)
    {
        int y = BoardManager.instance.BottomHalf_FindEmptySpaceInColumnClosestToHead(columnNumber);
        if (y != -1) // 选定的column有空格位
        {
            iTween.Stop(unit.gameObject);
            Vector3 moveToCoords = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(columnNumber, y);
            Vector3 currentCoords = unit.transform.position;
            currentCoords.x = moveToCoords.x;
            unit.transform.position = currentCoords;
            unit.MoveToPosition(columnNumber, y);
            BottomHalf_CheckFormationForUnit(unit);
            return true;
        }

        return false;
    }

    // 给所有正在蓄力的单位减少蓄力回合，并让蓄力完毕的单位发动攻击
    public IEnumerator TopHalf_ChargingUnitsTickDown()
    {
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = numRowsPerSide - 1; y >= 0; y--) // 从最尾部开始检查
            {
                Unit unit = TopHalf_GetUnitAtPosition(x, y);
                if (unit != null)
                {
                    if (unit.ChargeUpTickDown() == 0)
                    {
                        yield return new WaitForSeconds(1f); // 攻击后等待久一点
                    }
                    else if (unit.ChargeUpTickDown() > 0)
                    {
                        yield return new WaitForSeconds(0.3f); // 只是蓄力的话等待短一点
                    }
                    else
                    {
                        yield return null; // 不蓄力或攻击的话不等待
                    }
                }
            }
        }

        // consolidate棋盘，然后允许玩家控制
        StartCoroutine(TopHalf_ConsolidateUnits(GameManager.instance, "TurnStartStep_GrantPlayerControl"));
    }

    // 给所有正在蓄力的单位减少蓄力回合，并让蓄力完毕的单位发动攻击
    public IEnumerator BottomHalf_ChargingUnitsTickDown()
    {
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = numRowsPerSide - 1; y >= 0; y--) // 从最尾部开始检查
            {
                Unit unit = BottomHalf_GetUnitAtPosition(x, y);
                if (unit != null)
                {
                    if (unit.ChargeUpTickDown() == 0)
                    {
                        yield return new WaitForSeconds(1f); // 攻击后等待久一点
                    }
                    else if (unit.ChargeUpTickDown() > 0)
                    {
                        yield return new WaitForSeconds(0.3f); // 只是蓄力的话等待短一点
                    }
                    else
                    {
                        yield return null; // 不蓄力或攻击的话不等待
                    }
                }
            }
        }

        // consolidate棋盘，然后允许玩家控制
        StartCoroutine(BottomHalf_ConsolidateUnits(GameManager.instance, "TurnStartStep_GrantPlayerControl"));
    }

    // consolidate某一列
    public void TopHalf_DoConsolidateColumn(int col)
    {
        StartCoroutine(TopHalf_ConsolidateColumn(col));
    }
    // consolidate某一列
    IEnumerator TopHalf_ConsolidateColumn(int col)
    {
        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往尾部推
        {
            Unit unit = TopHalf_GetUnitAtPosition(col, y);
            if (unit != null)
            {
                int newY = y - 1;
                while (newY >= 0)
                {
                    if (TopHalf_GetUnitAtPosition(col, newY) == null) // 往前一行有空位
                    {
                        unit.MoveToPosition(col, newY, true);
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        TopHalf_CheckFormationForUnit(unit);
                        break; // 如果往前一行没有空位，则再往前也不会有空位了
                    }
                    newY--;
                }
            }

            yield return null;
        }
    }

    // consolidate某一列
    public void BottomHalf_DoConsolidateColumn(int col)
    {
        StartCoroutine(BottomHalf_ConsolidateColumn(col));
    }
    // consolidate某一列
    IEnumerator BottomHalf_ConsolidateColumn(int col)
    {
        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往尾部推
        {
            Unit unit = BottomHalf_GetUnitAtPosition(col, y);
            if (unit != null)
            {
                int newY = y - 1;
                while (newY >= 0)
                {
                    if (BottomHalf_GetUnitAtPosition(col, newY) == null) // 往前一行有空位
                    {
                        unit.MoveToPosition(col, newY, true);
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        BottomHalf_CheckFormationForUnit(unit);
                        break; // 如果往前一行没有空位，则再往前也不会有空位了
                    }
                    newY--;
                }
            }

            yield return null;
        }
    }

    // 显示棋盘，测试用
    void PrintGrids()
    {
        // top
        string output = "";
        for (int y = numRowsPerSide - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < numColumns; x++)
            {
                if (TopHalf_GetUnitAtPosition(x, y) != null) row += "U ";
                else row += "o ";
            }
            output += row + "\n";
        }
        Debug.Log(output);

        // bottom
        output = "";
        for (int y = 0; y < numRowsPerSide; y++)
        {
            string row = "";
            for (int x = 0; x < numColumns; x++)
            {
                if (BottomHalf_GetUnitAtPosition(x, y) != null) row += "U ";
                else row += "o ";
            }
            output += row + "\n";
        }
        Debug.Log(output);
    }

    // BOF Network Code
    void SendBoardState()
    {
        photonView.RPC("SyncBoard", PhotonTargets.Others, SerializeUnitGridAsUnitTypes(unitGridTop), SerializeUnitGridAsUnitTypes(unitGridBottom));
    }

    public void SendPickUpUnit(int x, int y)
    {
        photonView.RPC("SyncPickUpUnit", PhotonTargets.Others, x, y);
    }

    [PunRPC]
    void SyncPickUpUnit(int x, int y)
    {
        Unit unit = TopHalf_GetUnitAtPosition(x, y);
        unit.NetworkPickUpEnemyUnit();
    }

    public void SendPutDownUnit(int x)
    {
        photonView.RPC("SyncPutDownUnit", PhotonTargets.Others, x);
    }

    [PunRPC]
    void SyncPutDownUnit(int columnNumber)
    {
        if (BoardManager.instance.unitBeingPickedUp != null) // 如果有单位被玩家捡起，将其放置在选定的column尾部
        {
            if (!GameManager.instance.playersTurn)
            {
                if (BoardManager.instance.unitBeingPickedUp.boardX == columnNumber) // 玩家把单位放回至其所在的列，不减少move
                {
                    if (BoardManager.instance.TopHalf_LetUnitEnterColumnFromTail(BoardManager.instance.unitBeingPickedUp, columnNumber))
                    {
                        BoardManager.instance.unitBeingPickedUp = null;
                        GameManager.instance.SetColumnHighlightEnabled(false, 0f);
                    }
                }
                else // 玩家将单位放在另一列，用掉一个move
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

    int[] SerializeUnitGridAsUnitTypes(List<Unit> unitGrid)
    {
        int[] arrUnitGrid = new int[unitGrid.Count];
        for (int index = 0; index < unitGrid.Count; index++)
        {
            Unit unit = unitGrid[index];
            if (unit != null) arrUnitGrid[index] = (int)unit.GetUnitType();
            else arrUnitGrid[index] = -1;
        }
        return arrUnitGrid;
    }
    
    [PunRPC]
    void SyncBoard(int[] myGrid, int[] enemyGrid)
    {
        Debug.Log("syncing board: ");

        for (int index = 0; index < myGrid.Length; index++)
        {
            int unitType = myGrid[index];
            if (unitType >= 0) // skip empty spaces
            {
                Vector3 coordinates = gridCoordinatesBottom[index];
                GameObject obj = Instantiate(unitPrefabs[unitType], coordinates, Quaternion.identity) as GameObject;
                Vector2 position = GetPositionGivenArrayIndex(index);
                unitGridBottom[index] = obj.GetComponent<Unit>();
                unitGridBottom[index].SetPositionValues((int)position.x, (int)position.y);
                unitGridBottom[index].SetIsAtBottom(true);
                unitGridBottom[index].SetSortingLayer();
            }
        }

        for (int index = 0; index < enemyGrid.Length; index++)
        {
            int unitType = enemyGrid[index];
            if (unitType >= 0) // skip empty spaces
            {
                Vector3 coordinates = gridCoordinatesTop[index];
                GameObject obj = Instantiate(unitPrefabs[unitType], coordinates, Quaternion.identity) as GameObject;
                Vector2 position = GetPositionGivenArrayIndex(index);
                unitGridTop[index] = obj.GetComponent<Unit>();
                unitGridTop[index].SetPositionValues((int)position.x, (int)position.y);
                unitGridTop[index].SetIsAtBottom(false);
                unitGridTop[index].SetSortingLayer();
            }
        }

        StartCoroutine(TopHalf_ConsolidateUnits());
        StartCoroutine(BottomHalf_ConsolidateUnits(GameManager.instance, "GoToEnemyTurn"));
    }
    // EOF Network Code
}
