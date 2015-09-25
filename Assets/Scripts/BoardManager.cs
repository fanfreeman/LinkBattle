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


        for (int y = numRowsPerSide - 1; y >= 0; y--)
        {
            for (int x = 0; x < numColumns; x++)
            {
                Transform t = GetTopUnitAtPosition(x, y);
                if (t != null)
                {
                    Unit u = t.GetComponent<Unit>();
                    Debug.Log(u.GetTypeString());
                }
            }
        }
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
                Instantiate(gameObject, coordinates, Quaternion.identity);
                unitGridTop[randomIndex] = gameObject.transform;
            }
            else
            {
                randomIndex = RandomPositionBottom();
                Vector3 coordinates = gridCoordinatesBottom[randomIndex];
                Instantiate(gameObject, coordinates, Quaternion.identity);
                unitGridBottom[randomIndex] = gameObject.transform;
            }
            
        }
    }

    Transform GetTopUnitAtPosition(int x, int y)
    {
        return unitGridTop[y * numColumns + x];
    }

    Transform GetBottomUnitAtPosition(int x, int y)
    {
        return unitGridBottom[y * numColumns + x];
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
}
