using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//簡單AI : 基本棋型&積分判斷

public class AI_Level1 : IAIScriptInterface
{
    private Dictionary<string, int> ScoreTable = new Dictionary<string, int>(); //評分表
    private int[,] scoreMap; //分數地圖

    //建構子
    public AI_Level1()
    {
        ScoreTable.Add("aa_", 50);
        ScoreTable.Add("_aa", 50);
        ScoreTable.Add("_aa_", 100);

        ScoreTable.Add("aaa_", 250);
        ScoreTable.Add("_aaa", 250);
        ScoreTable.Add("_aaa_", 1500);

        ScoreTable.Add("aaaa_", 1500);
        ScoreTable.Add("_aaaa", 1500);
        ScoreTable.Add("_aaaa_", 50000);
    }

    //測試方法
    public void TestMethod(Vector2 pos)
    {
        Debug.Log("--簡單AI--");
    }

    //下棋點演算
    public Vector2 ChessOperator()
    {
        if (ChessBehavior.Instance.chessLine.Count == 0) return new Vector2(7, 7); //若為第一顆子則下正中央

        scoreMap = new int[(int)ChessBehavior.Instance.size.x + 1, (int)ChessBehavior.Instance.size.y + 1]; //初始化分數地圖
        int[,] grid = ChessBehavior.Instance.grid;

        for (int i = 0; i <= grid.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= grid.GetUpperBound(0); j++)
            {
                if (grid[j, i] == 0)
                {
                    for (int k = 1; k <= 2; k++)
                    {
                        CheckOneLine(new Vector2(j, i), new int[] { 0, 1 }, k);
                        CheckOneLine(new Vector2(j, i), new int[] { 1, 0 }, k);
                        CheckOneLine(new Vector2(j, i), new int[] { 1, 1 }, k);
                        CheckOneLine(new Vector2(j, i), new int[] { 1, -1 }, k);
                    }
                }
            }
        }

        Vector2 highest = new Vector2();
        for (int i = 0; i <= scoreMap.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= scoreMap.GetUpperBound(0); j++)
            {
                if (Mathf.Abs(scoreMap[j, i]) >= Mathf.Abs(scoreMap[(int)highest.x, (int)highest.y]) )
                {
                    highest = new Vector2(j, i);
                }
            }
        }

        return highest;
    }

    //檢測棋子連線並評分
    public void CheckOneLine(Vector2 pos, int[] offset, int type)
    {
        string shape = "a";
        bool deadStep = true;

        //往右遍歷
        for (int i = offset[0], j = offset[1]; (pos.x + i >= 0 && pos.x + i <= ChessBehavior.Instance.size.x) && (pos.y + j >= 0 && pos.y + j <= ChessBehavior.Instance.size.y); i += offset[0], j += offset[1])
        {
            if (ChessBehavior.Instance.grid[(int)pos.x + i, (int)pos.y + j] == type)
            {
                shape = shape + "a";
            }
            else if(ChessBehavior.Instance.grid[(int)pos.x + i, (int)pos.y + j] == 0)
            {
                shape = shape + "_";
                deadStep = false;
                break;
            }
            else
            {
                break;
            }
        }
        //往左遍歷
        for (int i = -offset[0], j = -offset[1]; (pos.x + i >= 0 && pos.x + i <= ChessBehavior.Instance.size.x) && (pos.y + j >= 0 && pos.y + j <= ChessBehavior.Instance.size.y); i -= offset[0], j -= offset[1])
        {
            if (ChessBehavior.Instance.grid[(int)pos.x + i, (int)pos.y + j] == type)
            {
                shape = "a" + shape;
            }
            else if (ChessBehavior.Instance.grid[(int)pos.x + i, (int)pos.y + j] == 0)
            {
                shape = "_" + shape;
                deadStep = false;
                break;
            }
            else
            {
                break;
            }
        }

        if (shape == "_aaaaa" || shape == "aaaaa" || shape == "aaaaa_" || shape == "_aaaaa_") //必勝狀況評比為最高分
        {
            scoreMap[(int)pos.x, (int)pos.y] = int.MaxValue;
        }
        else //其他狀況, 只要不是雙邊都被他子包圍則按照評分表評分
        {
            if (!deadStep && ScoreTable.ContainsKey(shape))
            {
                scoreMap[(int)pos.x, (int)pos.y] += ScoreTable[shape];
            }
        }

    }

}
