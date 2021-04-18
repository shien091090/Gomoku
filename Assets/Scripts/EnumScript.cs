using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessType //棋子陣營
{
    黑子, 白子
}

public enum PlayerType //玩家類型
{
    玩家, 電腦
}

public enum AiType //AI類型
{
    智障, 簡單, 普通, 高手, 夭壽強
}

public enum TransType
{
    任意座標轉棋盤格索引, 任意座標轉棋盤點座標
}

public class EnumScript : MonoBehaviour { }
