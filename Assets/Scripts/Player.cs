using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public ChessType chessType; //棋子陣營
    public PlayerType playerType; //玩家類型
    public AiType aiType; //AI類型
    public bool retractSwitch = false; //悔棋判斷(1次/每輪)
    public IAIScriptInterface aiScript; //AI腳本

    void Update()
    {
        if (ChessBehavior.Instance.gameStart)
        {
            if (ChessBehavior.Instance.turn == chessType)
            {
                //if (playerType == PlayerType.Debug) //DEBUG模式時每按下F10下一步棋
                //{
                //    if (Input.GetKeyDown(KeyCode.F10)) MovePiece();
                //}
                if (ChessBehavior.Instance.timer >= ChessBehavior.Instance.turnNextDelay) //正常遊戲模式時需等待一定時間才可下棋
                {
                    MovePiece();
                }
            }
            else if (retractSwitch) //輪到對方下棋時, 重置悔棋判斷開關
            {
                retractSwitch = false;
            }
        }
    }

    //設定AI
    public void SetAI(AiType type)
    {
        aiType = type;

        switch (type)
        {
            case AiType.智障:
                aiScript = new AI_Level0();
                break;

            case AiType.簡單:
                aiScript = new AI_Level1();
                break;

            case AiType.普通:
                aiScript = new AI_Level2((chessType == ChessType.黑子 ? 1 : 2));
                break;

            case AiType.高手:
                aiScript = new AI_Level3((chessType == ChessType.黑子 ? 1 : 2));
                break;

            case AiType.夭壽強:
                aiScript = new AI_Level4((chessType == ChessType.黑子 ? 1 : 2));
                break;

            default:
                break;
        }
    }

    public void MovePiece() //下棋動作
    {
        //if (playerType == PlayerType.電腦 || playerType == PlayerType.Debug) //AI控制時
        //{
        //    ChessBehavior.Instance.PlayChess(aiScript.ChessOperator());
        //}

        if (playerType == PlayerType.電腦) //AI控制時
        {
            ChessBehavior.Instance.PlayChess(aiScript.ChessOperator());
        }
        else //玩家控制時
        {
            if (!retractSwitch) //判斷一次悔棋按鈕是否可用
            {
                if (UIController.Instance.retractRemain > 0 && ChessBehavior.Instance.chessLine.Count >= 2)
                {
                    UIController.Instance.RetractButtonState(true);
                }
                retractSwitch = true;
            }

            if (Input.GetMouseButtonDown(0)) //滑鼠左鍵 = 下棋
            {
                Vector2 chessIndex;
                if (ChessBehavior.Instance.PointConversion(Camera.main.ScreenToWorldPoint(Input.mousePosition), TransType.任意座標轉棋盤格索引, out chessIndex))
                {
                    ChessBehavior.Instance.PlayChess(chessIndex);
                }
            }
        }
    }

}
