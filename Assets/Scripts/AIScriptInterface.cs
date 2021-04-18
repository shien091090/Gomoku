using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIScriptInterface
{
    Vector2 ChessOperator(); //返回演算後的下棋點
    void TestMethod(Vector2 pos); //測試方法
}
