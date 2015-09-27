using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {

    public static BoardManager instance = null;

    // 兵种
    public GameObject archer;

    public int numColumns = 8;
    public int numRowsPerSide = 6;

    // 空档格位的xy坐标列表
    private List<Vector3> gridCoordinatesTop = new List<Vector3>();
    private List<Vector3> gridCoordinatesBottom = new List<Vector3>();

    // 存储所有单位的数组matrix
    private List<Transform> unitGridTop;
    private List<Transform> unitGridBottom;

    void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void SetupScene() {
        SetUpGrids();
        InitializeGridCoordinates();
        LayoutObjectAtRandom(true, archer, 2, 12); // top
        LayoutObjectAtRandom(false, archer, 2, 12); // bottom

        PrintGrids();

        StartCoroutine(ConsolidateTop());
        StartCoroutine(ConsolidateBottom());


        //for (int y = 0; y < numRowsPerSide; y++)
        //{
        //    for (int x = 0; x < numColumns; x++)
        //    {
        //        Transform t = GetTopUnitAtPosition(x, y);
        //        if (t != null)
        //        {
        //            Unit u = t.GetComponent<Unit>();
        //            Debug.Log(u.GetTypeString());

        //            if (y > 0)
        //            {
        //                Vector3 moveToPos = GetTopCoordinatesAtPosition(x, y - 1);
        //                iTween.MoveTo(t.gameObject, moveToPos, 2);
        //            }
        //        }
        //    }
        //}
    }

    void SetUpGrids() {
        // 初始化棋盘上半部分的grid为null
        unitGridTop = new List<Transform>(numColumns * numRowsPerSide);
        for (int i = 0; i < numColumns * numRowsPerSide; i++)
        {
            unitGridTop.Add(null);
        }

        // 初始化棋盘下半部分的grid为null
        unitGridBottom = new List<Transform>(numColumns * numRowsPerSide);
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
        // top
        for (int y = 0; y < numRowsPerSide; y++) {
            for (int x = 0; x < numColumns; x++) {
                gridCoordinatesTop.Add(new Vector3(x - numColumns / 2 + 0.5f, y + 1, 0f));
            }
        }
        
        // bottom
        for (int y = numRowsPerSide - 1; y >= 0; y--) {
            for (int x = 0; x < numColumns; x++) {
                gridCoordinatesBottom.Add(new Vector3(x - numColumns / 2 + 0.5f, y - 7, 0f));
            }
        }
    }

    int RandomPositionTop() {
        int randomIndex = Random.Range(0, unitGridTop.Count);
        Transform unit = unitGridTop[randomIndex];
        while (unit != null)
        {
            randomIndex = Random.Range(0, unitGridTop.Count);
            unit = unitGridTop[randomIndex];
        }
        return randomIndex;
    }

    int RandomPositionBottom() {
        int randomIndex = Random.Range(0, unitGridBottom.Count);
        Transform unit = unitGridBottom[randomIndex];
        while (unit != null)
        {
            randomIndex = Random.Range(0, unitGridBottom.Count);
            unit = unitGridBottom[randomIndex];
        }
        return randomIndex;
    }

    // 随机找空格位放置单位
    void LayoutObjectAtRandom(bool isTop, GameObject gameObject, int minimum, int maximum) {
        int objectCount = Random.Range(minimum, maximum + 1);
        for (int i = 0; i < objectCount; i++)  {
            int randomIndex;
            if (isTop)
            {
                randomIndex = RandomPositionTop();
                Vector3 coordinates = gridCoordinatesTop[randomIndex];
                GameObject obj = Instantiate(gameObject, coordinates, Quaternion.identity) as GameObject;
                unitGridTop[randomIndex] = obj.transform;
            }
            else
            {
                randomIndex = RandomPositionBottom();
                Vector3 coordinates = gridCoordinatesBottom[randomIndex];
                GameObject obj = Instantiate(gameObject, coordinates, Quaternion.identity) as GameObject;
                unitGridBottom[randomIndex] = obj.transform;
            }
            
        }
    }

    // 提供xy，获得该格位的单位（或null）
    Transform GetTopUnitAtPosition(int x, int y)
    {
        return unitGridTop[y * numColumns + x];
    }

    Transform GetBottomUnitAtPosition(int x, int y)
    {
        return unitGridBottom[y * numColumns + x];
    }

    void SetTopUnitAtPosition(Transform unit, int x, int y)
    {
        unitGridTop[y * numColumns + x] = unit;
    }

    void SetBottomUnitAtPosition(Transform unit, int x, int y)
    {
        unitGridBottom[y * numColumns + x] = unit;
    }

    // 提供xy，获得该格位的screen coordinates
    Vector3 GetTopCoordinatesAtPosition(int x, int y)
    {
        return gridCoordinatesTop[y * numColumns + x];
    }
    Vector3 GetBottomCoordinatesAtPosition(int x, int y)
    {
        return gridCoordinatesBottom[y * numColumns + x];
    }

    // 将上半部分所有单位往下推
    IEnumerator ConsolidateTop()
    {
        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往下推
        {
            for (int x = 0; x < numColumns; x++)
            {
                Transform t = GetTopUnitAtPosition(x, y);
                if (t != null)
                {
                    int currentY = y;
                    int newY = y - 1;
                    while (newY >= 0)
                    {
                        if (GetTopUnitAtPosition(x, newY) == null) // 下一行有空位
                        {
                            SetTopUnitAtPosition(null, x, currentY);
                            SetTopUnitAtPosition(t, x, newY);
                            currentY = newY;

                            Vector3 moveToPos = GetTopCoordinatesAtPosition(x, newY);
                            iTween.MoveTo(t.gameObject, moveToPos, 1f);
                            yield return new WaitForSeconds(0.1f);
                        }
                        else break; // 如果下一行没有空位，则再往下也不会有空位了
                        newY--;
                    }
                }

                yield return null;
            }
        }
    }

    // 将下半部分所有单位往上推
    IEnumerator ConsolidateBottom()
    {
        for (int y = 1; y < numRowsPerSide; y++) // 从第二行开始往上推
        {
            for (int x = 0; x < numColumns; x++)
            {
                Transform t = GetBottomUnitAtPosition(x, y);
                if (t != null)
                {
                    int currentY = y;
                    int newY = y - 1;
                    while (newY >= 0)
                    {
                        if (GetBottomUnitAtPosition(x, newY) == null) // 上一行有空位
                        {
                            SetBottomUnitAtPosition(null, x, currentY);
                            SetBottomUnitAtPosition(t, x, newY);
                            currentY = newY;

                            Vector3 moveToPos = GetBottomCoordinatesAtPosition(x, newY);
                            iTween.MoveTo(t.gameObject, moveToPos, 1f);
                            yield return new WaitForSeconds(0.1f);
                        }
                        else break; // 如果上一行没有空位，则再往上也不会有空位了
                        newY--;
                    }
                }

                yield return null;
            }
        }
    }

    void PrintGrids()
    {
        // top
        string output = "";
        for (int y = numRowsPerSide - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < numColumns; x++)
            {
                if (GetTopUnitAtPosition(x, y) != null) row += "U ";
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
                if (GetBottomUnitAtPosition(x, y) != null) row += "U ";
                else row += "o ";
            }
            output += row + "\n";
        }
        Debug.Log(output);
    }

    void Update()
    {
        
    }
}
