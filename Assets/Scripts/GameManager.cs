using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : Photon.PunBehaviour
{
    public static GameManager instance = null;

    public GameObject columnHighlightPrefab;
    public GameObject selectorTop;
    public GameObject selectorBottom;

    [HideInInspector] public BoardManager boardScript;
    [HideInInspector] public BattleLoader battleLoader;
    [HideInInspector] public bool playersTurn = false;
    [HideInInspector] public GameObject columnHighlight;
    [HideInInspector] public GameObject hudCanvas;
    [HideInInspector] public bool canMove = false;

    int movesLeftThisTurn = 3;
    Text moveTextTop;
    Text moveTextBottom;
    TurnMessageController turnMessageController;
    
    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        
        moveTextTop = selectorTop.GetComponentInChildren<Text>();
        moveTextBottom = selectorBottom.GetComponentInChildren<Text>();
        selectorTop.SetActive(false);
        selectorBottom.SetActive(false);

        hudCanvas = GameObject.Find("HudCanvas");
        turnMessageController = hudCanvas.GetComponentInChildren<TurnMessageController>();

        boardScript = GetComponent<BoardManager>();
        battleLoader = GetComponent<BattleLoader>();

        InitGame();

        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    // BOF Network Code
    int networkState = 0;

    public override void OnJoinedLobby()
    {
        Debug.Log("JoinRandom");
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.isMasterClient)
        {
            networkState = 1;
            Debug.Log("Joined room, staring new game...");
        }
        else
        {
            Debug.Log("Joined room, waiting for game state from master...");
        }
    }

    public void OnGUI()
    {
        string statusString = "";
        switch (networkState)
        {
            case 0:
                statusString = PhotonNetwork.connectionStateDetailed.ToString();
                break;
            case 1:
                statusString = "Waiting for other player to connect...";
                break;
        }
        GUILayout.Label(statusString);

        //if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        //{
            
        //}
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);

        if (PhotonNetwork.isMasterClient)
        {
            BoardManager.instance.SetupRandomBoardState();
        }
    }
    // EOF Network Code

    void InitGame()
    {
        boardScript.InitBoard();
        battleLoader.SetUpBattleLoader();
        CreateColumnHighlight();
    }

    void CreateColumnHighlight()
    {
        Vector3 coordinates = new Vector3();
        columnHighlight = Instantiate(columnHighlightPrefab, coordinates, Quaternion.identity) as GameObject;
        columnHighlight.SetActive(false);
    }

    public void SetColumnHighlightEnabled(bool isEnabled, float boardCoordX)
    {
        Vector3 currentCoords = columnHighlight.transform.position;
        currentCoords.x = boardCoordX;
        columnHighlight.transform.position = currentCoords;
        columnHighlight.SetActive(isEnabled);
        iTween.Stop(columnHighlight);
    }

    // 用掉一个move
    public void UseOneMove()
    {
        movesLeftThisTurn--;
        SetNumberOfMovesText();

        if (movesLeftThisTurn == 0)
        {
            GoToNextTurn();
        }
    }

    public int GetMoveNumber()
    {
        return 3 - movesLeftThisTurn;
    }

    void SetNumberOfMovesText()
    {
        if (playersTurn) moveTextBottom.text = movesLeftThisTurn.ToString();
        else moveTextTop.text = movesLeftThisTurn.ToString();
    }

    // 进入下一回合第一步，开始显示回合message
    public void GoToNextTurn()
    {
        playersTurn = !playersTurn;
        movesLeftThisTurn = 3;
        SetNumberOfMovesText();
        RevokeControl();
        turnMessageController.Show();
        BattleLoader.instance.DisableCallReserveButtons();
    }

    void GoToEnemyTurn()
    {
        playersTurn = true;
        GoToNextTurn();
    }

    // 显示回合开始message后，第二步是给蓄力中的单位减少蓄力countdown，以及蓄满的单位发动攻击
    public void TurnStartStep_ChargingUnitsTickDown()
    {
        if (playersTurn)
        {
            StartCoroutine(BoardManager.instance.BottomHalf_ChargingUnitsTickDown());
        }
        else
        {
            StartCoroutine(BoardManager.instance.TopHalf_ChargingUnitsTickDown());
        }

        Vector2 randomPosition = BoardManager.instance.GetNextRandomPosition();
        Debug.LogError("random position x:" + randomPosition.x + " y:" + randomPosition.y);
    }

    // 回合开始第三步，玩家获得控制（真正的回合开始）
    public void TurnStartStep_GrantPlayerControl()
    {
        if (playersTurn)
        {
            selectorBottom.SetActive(true);
            BattleLoader.instance.EnableBottomCallReserveButton();
        }
        else
        {
            selectorTop.SetActive(true);
            BattleLoader.instance.EnableTopCallReserveButton();

            // FOR DEBUGGING ONLY check board sync integrity
            //BoardManager.instance.CheckGameHealth();
        }

        canMove = true;
    }
    
    // 回合结束后，禁止玩家操作
    void RevokeControl()
    {
        canMove = false;
        selectorTop.SetActive(false);
        selectorBottom.SetActive(false);
    }
}
