using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAttribute : MonoBehaviour
{
    public ChessType thisChess; //棋子類型
    public Vector2 gridPos; //棋盤位置

    public void SetInfo(ChessType c, Vector2 p)
    {
        thisChess = c;
        gridPos = p;
    }

}
