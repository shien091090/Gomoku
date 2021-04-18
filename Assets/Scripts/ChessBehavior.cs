//遊戲邏輯控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChessBehavior : MonoBehaviour
{
    #region 變數宣告 --------------------------------------------------------------------------------------------------------------
    private static ChessBehavior _instance; //單例模式
    public static ChessBehavior Instance
    {
        get { return _instance; }
    }

    [Header("遊戲進程變數")]
    public ChessType turn; //目前回合者
    public bool gameStart; //玩家可否動作
    public bool mousePosHint; //滑鼠位置提示
    public bool lastChessPosHint; //最後下棋點提示
    public Vector2 size; //棋盤尺寸
    public Vector2 chessRange; //棋子範圍(x,y)
    public int[,] grid; //棋盤格狀態
    public float timer; //計時器
    public float turnNextDelay; //輪流等待時間
    public List<GameObject> chessLine = new List<GameObject>(); //紀錄棋子順序的對列(用於悔棋)
    private List<AnimationScript> winnerChessAnim = new List<AnimationScript>(); //儲存連成五子的棋子的AnimationSrcipt
    [Header("參考物件")]
    public GameObject black; //黑子
    public GameObject white; //白子
    public GameObject cursor; //光標
    public GameObject lastChessIcon; //最後下棋點標示
    public Transform[] anchor; //範圍錨點
    public Transform chessHolder; //棋子父物件
    public IAIScriptInterface ai_white; //白棋AI
    public IAIScriptInterface ai_black; //黑棋AI
    [Header("棋盤監控參數")]
    public GameObject coordinatePanel; //座標提示面板
    public GameObject coordinateCursor; //座標提示跟隨游標
    public Text[] panelUnit; //面板單位

    private Vector2 _p; //光標位置

    #endregion
    #region 內建變數 --------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
    }

    void Update()
    {
        if (gameStart)
        {
            timer += Time.deltaTime;

            //光標顯示
            if (mousePosHint && PointConversion(Camera.main.ScreenToWorldPoint(Input.mousePosition), TransType.任意座標轉棋盤點座標, out _p))
            {
                if (!cursor.activeSelf) cursor.SetActive(true);
                cursor.transform.position = _p;
            }
            else if (cursor.activeSelf) cursor.SetActive(false);

            ////棋盤格座標提示面板(open&close)
            //if (Input.GetKeyDown(KeyCode.F1))
            //{
            //    coordinatePanel.SetActive(!coordinatePanel.activeSelf);
            //    coordinateCursor.SetActive(coordinatePanel.activeSelf);
            //}

            //棋盤格提示跟隨游標
            if (coordinateCursor.activeSelf)
            {
                Text _c = coordinateCursor.GetComponent<Text>();

                Vector2 _p;
                PointConversion(Camera.main.ScreenToWorldPoint(Input.mousePosition), TransType.任意座標轉棋盤格索引, out _p);
                _c.text = "(" + _p.x + "," + _p.y + ")";

                Vector3 _m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                coordinateCursor.transform.position = new Vector2(_m.x, _m.y);
            }

            ////AI測試方法
            //if (Input.GetMouseButtonDown(2))
            //{
            //    Vector2 _p;
            //    PointConversion(Camera.main.ScreenToWorldPoint(Input.mousePosition), TransType.任意座標轉棋盤格索引, out _p);

            //    if (ai_black != null) ai_black.TestMethod(_p);
            //    if (ai_white != null) ai_white.TestMethod(_p);
            //}
        }
    }
    #endregion
    #region 自訂方法 --------------------------------------------------------------------------------------------------------------

    //遊戲初始化
    public void GameInitialization()
    {
        grid = new int[(int)size.x + 1, (int)size.y + 1]; //定義棋盤格尺寸
        chessRange = new Vector2((anchor[0].transform.position.x - anchor[1].transform.position.x) / size.x, (anchor[0].transform.position.y - anchor[1].transform.position.y) / size.y); //定義棋盤間隔
        gameStart = true; //遊戲開始
        UIController.Instance.chessFocus[turn].SetRotating(true); //先手陣營棋盆圖示開始旋轉
    }

    //判斷值是否在範圍內
    private bool Between(Vector2 v, Vector2 a1, Vector2 a2, Vector2 range)
    {
        if (a2.x - (range.x / 2) <= v.x && v.x < a1.x + (range.x / 2) &&
            a2.y - (range.y / 2) <= v.y && v.y < a1.y + (range.y / 2)) return true;
        else return false;
    }

    //座標位置轉換
    public bool PointConversion(Vector2 p, TransType t, out Vector2 output)
    {
        if (Between(p, anchor[0].position, anchor[1].position, chessRange))
        {
            int xIndex;
            int yIndex;

            switch (t)
            {
                case TransType.任意座標轉棋盤格索引:
                    xIndex = (int)((p.x - (anchor[1].position.x - (chessRange.x / 2))) / chessRange.x);
                    yIndex = (int)((p.y - (anchor[1].position.y - (chessRange.y / 2))) / chessRange.y);
                    output = new Vector2(xIndex, yIndex);
                    return true;

                case TransType.任意座標轉棋盤點座標:
                    xIndex = (int)((p.x - (anchor[1].position.x - (chessRange.x / 2))) / chessRange.x);
                    yIndex = (int)((p.y - (anchor[1].position.y - (chessRange.y / 2))) / chessRange.y);
                    float xPos = anchor[1].position.x + (xIndex * chessRange.x);
                    float yPos = anchor[1].position.y + (yIndex * chessRange.y);
                    output = new Vector2(xPos, yPos);
                    return true;

                default:
                    output = new Vector2();
                    return false;
            }
        }
        else
        {
            output = new Vector2();
            return false;
        }
    }

    //下棋
    public void PlayChess(Vector2 index)
    {
        if (grid[(int)index.x, (int)index.y] == 0)
        {
            float xPos = anchor[1].position.x + (index.x * chessRange.x); //取得位置
            float yPos = anchor[1].position.y + (index.y * chessRange.y);
            GameObject chessType = turn == ChessType.黑子 ? black : white; //取得預置體

            UIController.Instance.RetractButtonState(false); //暫時禁用悔棋按鈕
            GameObject chess = Instantiate(chessType, new Vector2(xPos, yPos), Quaternion.identity, chessHolder); //產生棋子
            chess.GetComponent<ChessAttribute>().SetInfo(turn, index); //設定棋子資訊
            grid[(int)index.x, (int)index.y] = turn == ChessType.黑子 ? 1 : 2; //棋盤格註冊
            chessLine.Add(chess); //加入棋子對列
            UIController.Instance.chessFocus[turn].SetRotating(false); //下完棋後棋盆圖示停止旋轉
            AudioManagerScript.Instance.PlayAudioClip("chess"); //撥放音效

            if (lastChessPosHint)
            {
                if (!lastChessIcon.activeSelf) lastChessIcon.SetActive(true);
                lastChessIcon.transform.localPosition = chess.transform.localPosition + new Vector3(-0.52f, 1.6286f); //最後下棋點提示
            }
            else if (lastChessIcon.activeSelf) lastChessIcon.SetActive(false);

            if (CheckWinner(index))
            {
                AudioManagerScript.Instance.PlayAudioClip("game_over");
                UIController.Instance.cg_gameMenu.blocksRaycasts = false; //遊戲選單控件封鎖
                gameStart = false; //遊戲停止
                cursor.SetActive(false); //隱藏滑鼠光標

                UIController.Instance.speechBalloonScript_typeB.ShowSpeechBalloon(new UnityAction[] { () => { StartCoroutine(GameFlow.Instance.BackToMenu()); }, () => { StartCoroutine(GameFlow.Instance.PlayAgain()); } }, new string[] { "返回主選單", "再來一局" }, System.Enum.GetName(typeof(ChessType), turn) + "勝利");
            }
            else
            {
                //Debug.Log("turn over");
                turn = turn == ChessType.白子 ? ChessType.黑子 : ChessType.白子; //切換玩家行動
                timer = 0; //重製計時器
                UIController.Instance.chessFocus[turn].SetRotating(true); //換對方下棋, 對方棋盆圖示開始旋轉
            }
        }
        else
        {
            Debug.Log("overlapping!!");
        }
    }

    //檢測棋子連線
    public bool CheckOneLine(Vector2 pos, int[] offset)
    {
        int linkNum = 1;
        //針對右邊for迴圈,針對棋子的x與y座標做遍歷;限定遍歷範圍;每遍歷一次遞增
        for (int i = offset[0], j = offset[1]; (pos.x + i >= 0 && pos.x + i <= size.x) && (pos.y + j >= 0 && pos.y + j <= size.y); i += offset[0], j += offset[1])
        {
            if (grid[(int)pos.x + i, (int)pos.y + j] == (int)turn + 1)
            {
                linkNum++;
            }
            else
            {
                break;
            }
        }
        //針對左邊for迴圈,針對棋子的x與y座標做遍歷;限定遍歷範圍;每遍歷一次遞減
        for (int i = -offset[0], j = -offset[1]; (pos[0] + i >= 0 && pos[0] + i <= size.x) && (pos[1] + j >= 0 && pos[1] + j <= size.y); i -= offset[0], j -= offset[1])
        {
            if (grid[(int)pos.x + i, (int)pos.y + j] == (int)turn + 1)
            {
                linkNum++;
            }
            else
            {
                break;
            }
        }

        if (linkNum > 4) //當棋子大於4顆
        {
            //針對連成五子(以上)的棋子, 賦予動畫
            for (int k = 0; k < chessLine.Count; k++)
            {
                ChessAttribute info = chessLine[k].GetComponent<ChessAttribute>();
                if (info.gridPos.x == (int)pos.x && info.gridPos.y == (int)pos.y)
                {
                    AnimationScript anim = chessLine[k].GetComponent<AnimationScript>();
                    winnerChessAnim.Add(anim);
                    anim.PlayAnimation("Interlude_SuccessChess", new AnimationBehavior(PlayMode.循環撥放), false);
                }
            }

            for (int i = offset[0], j = offset[1]; (pos.x + i >= 0 && pos.x + i <= size.x) && (pos.y + j >= 0 && pos.y + j <= size.y); i += offset[0], j += offset[1])
            {
                if (grid[(int)pos.x + i, (int)pos.y + j] == (int)turn + 1)
                {
                    for (int k = 0; k < chessLine.Count; k++)
                    {
                        ChessAttribute info = chessLine[k].GetComponent<ChessAttribute>();
                        if (info.gridPos.x == (int)pos.x + i && info.gridPos.y == (int)pos.y + j)
                        {
                            AnimationScript anim = chessLine[k].GetComponent<AnimationScript>();
                            winnerChessAnim.Add(anim);
                            anim.PlayAnimation("Interlude_SuccessChess", new AnimationBehavior(PlayMode.循環撥放), false);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            for (int i = -offset[0], j = -offset[1]; (pos[0] + i >= 0 && pos[0] + i <= size.x) && (pos[1] + j >= 0 && pos[1] + j <= size.y); i -= offset[0], j -= offset[1])
            {
                if (grid[(int)pos.x + i, (int)pos.y + j] == (int)turn + 1)
                {
                    for (int k = 0; k < chessLine.Count; k++)
                    {
                        ChessAttribute info = chessLine[k].GetComponent<ChessAttribute>();
                        if (info.gridPos.x == (int)pos.x + i && info.gridPos.y == (int)pos.y + j)
                        {
                            AnimationScript anim = chessLine[k].GetComponent<AnimationScript>();
                            winnerChessAnim.Add(anim);
                            anim.PlayAnimation("Interlude_SuccessChess", new AnimationBehavior(PlayMode.循環撥放), false);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return true;
        }
        return false;
    }

    //判斷是否勝利
    public bool CheckWinner(Vector2 v)
    {
        if (CheckOneLine(v, new int[2] { 1, 0 })) return true;
        if (CheckOneLine(v, new int[2] { 0, 1 })) return true;
        if (CheckOneLine(v, new int[2] { 1, 1 })) return true;
        if (CheckOneLine(v, new int[2] { 1, -1 })) return true;

        return false;
    }

    //悔棋
    public bool RetractChess()
    {
        if (chessLine.Count > 1)//最少大於一顆
        {
            for (int i = 0; i < 2; i++)
            {
                ChessAttribute chessInfo = chessLine[chessLine.Count - 1].GetComponent<ChessAttribute>();
                grid[(int)chessInfo.gridPos.x, (int)chessInfo.gridPos.y] = 0; //清空該棋盤格位置
                Destroy(chessLine[chessLine.Count - 1].gameObject); //移除物件
                chessLine.RemoveAt(chessLine.Count - 1); //縮減對列
            }
            return true;
        }
        return false;
    }

    #endregion
    #region 協同程序 --------------------------------------------------------------------------------------------------------------

    #endregion

}
