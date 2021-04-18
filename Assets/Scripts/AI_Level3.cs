using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//高手AI : 分析棋局判斷攻守時機

//評分節點
public class Node
{
    public Vector2 pos;
    public int score;

    public Node(Vector2 p, int s)
    {
        pos = p;
        score = s;
    }
}

public class AI_Level3 : IAIScriptInterface
{
    private Dictionary<string, int> d_ScoreTable = new Dictionary<string, int>(); //棋譜評分表
    private List<string> highScoreKifuList = new List<string>(); //最高分棋譜排列表
    private int aiChessType; //AI編號
    private const int c_maxScore = 1000000; //最高分(常數)

    public AI_Level3(int chessType) //建構子
    {
        aiChessType = chessType;

        AddScoreTable("aaaaa", c_maxScore);

        AddScoreTable("_aaaa_", 50000);
        AddScoreTable("aaaa_", 8000);
        AddScoreTable("aa_aa", 5000);
        AddScoreTable("aaa_a", 5000);

        AddScoreTable("_aaa_", 1500);
        AddScoreTable("_aa_a_", 1000);
        AddScoreTable("aaa__", 500);
        AddScoreTable("aa_a_", 400);
        AddScoreTable("a_a_a", 300);

        AddScoreTable("_aa__", 100);
        AddScoreTable("_a_a_", 70);
        AddScoreTable("aa___", 40);
        AddScoreTable("a_a__", 20);
    }

    //測試方法
    public void TestMethod(Vector2 pos)
    {
        Debug.Log("--高手AI--");
        Debug.Log("type[1] = 黑子 / pos[" + pos.x + "," + pos.y + "]總分 = " + ScoreChessPoint(pos, ChessBehavior.Instance.grid, 1, true, false));
        Debug.Log("type[2] = 白子 / pos[" + pos.x + "," + pos.y + "]總分 = " + ScoreChessPoint(pos, ChessBehavior.Instance.grid, 2, true, false));
    }

    //下棋演算法
    public Vector2 ChessOperator()
    {
        if (ChessBehavior.Instance.chessLine.Count == 0) return new Vector2(7, 7);

        int playerChessType = aiChessType == 1 ? 2 : 1; //玩家編號
        int[,] nowGrid = (int[,])ChessBehavior.Instance.grid.Clone();

        int totalScore_ai = 0;
        int totalScore_player = 0;
        int totalScore_plus = 0;
        List<Node> highestPoints_ai = ReturnHighestList(nowGrid, aiChessType, 3, out totalScore_ai); //取AI目前最高分格
        List<Node> highestPoints_player = ReturnHighestList(nowGrid, playerChessType, 3, out totalScore_player); //取玩家目前最高分格
        List<Node> highestPoints_total = ReturnHighestList(nowGrid, -1, 3, out totalScore_plus); //取雙方合計最高分格

        Vector2 targetPos;
        if ((highestPoints_ai.Count > 0 && highestPoints_ai[0].score >= c_maxScore) || (highestPoints_ai.Count > 0 && highestPoints_player.Count >= 0 && highestPoints_player[0].score <= d_ScoreTable["aaaa_"] + d_ScoreTable["_aa__"]))
        {
            if (highestPoints_ai[0].score == highestPoints_ai[1].score)
            {
                int _rdm = Random.Range(0, 2);
                targetPos = highestPoints_ai[_rdm].pos;
            }
            else targetPos = highestPoints_ai[0].pos;
        }
        else
        {
            if (highestPoints_total[0].score == highestPoints_total[1].score)
            {
                int _rdm = Random.Range(0, 2);
                targetPos = highestPoints_total[_rdm].pos;
            }
            else targetPos = highestPoints_total[0].pos;
        }

        nowGrid[(int)targetPos.x, (int)targetPos.y] = aiChessType;

        return targetPos;
    }

    //依據棋局狀態找出最高分點(若 type = -1 則計算雙方分數和)
    private List<Node> ReturnHighestList(int[,] grid, int type, int amount, out int totalScore)
    {
        List<Node> nodeList = new List<Node>();

        //記錄所有棋盤格的分數
        for (int i = 0; i <= grid.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= grid.GetUpperBound(0); j++)
            {
                if (grid[j, i] == 0)
                {
                    int score = 0;

                    if (type == -1)
                    {
                        score += ScoreChessPoint(new Vector2(j, i), grid, 1, false, false);
                        score += ScoreChessPoint(new Vector2(j, i), grid, 2, false, false);
                    }
                    else score += ScoreChessPoint(new Vector2(j, i), grid, type, false, false);

                    if (score != 0) //只有分數不為0時才會採用至List
                    {
                        if (score > c_maxScore) score = c_maxScore; //設置上限
                        Node _n = new Node(new Vector2(j, i), score);
                        nodeList.Add(_n);
                    }
                }
            }
        }

        //排序
        nodeList.Sort((Node x, Node y) =>
        {
            if (x.score < y.score) return 1;
            if (x.score > y.score) return -1;
            return 0;
        }
        );

        //只保留amount個最大值
        if (nodeList.Count != 0) nodeList.RemoveRange(amount, nodeList.Count - amount);

        int _s = 0;
        for (int i = 0; i < nodeList.Count; i++) //排序後的總計分數
        {
            //Debug.Log("[" + i + "][highest] pos[" + nodeList[i].pos.x + "," + nodeList[i].pos.y + "] /score:" + nodeList[i].score);
            _s += nodeList[i].score;
        }

        totalScore = _s > c_maxScore ? c_maxScore : _s;
        return nodeList;
    }

    //依據棋局狀態對某一個棋盤點評分
    private int ScoreChessPoint(Vector2 pos, int[,] grid, int type, bool debug, bool deepDebug)
    {
        int score = 0;
        byte a2 = 0; //活二
        byte a3 = 0; //活三
        byte d4 = 0; //死四
        string _kifu; //棋型
        List<int[]> dir = new List<int[]>() //檢測方向
        {
            new int[]{ 0, 1 },
            new int[]{ 1, 0 },
            new int[]{ 1, 1 },
            new int[]{ 1, -1 },
        };

        for (int i = 0; i < dir.Count; i++)
        {
            int _s = CheckOneLine(pos, grid, dir[i], type, deepDebug, out _kifu);
            score += _s;

            string _dirText = null;
            switch (i)
            {
                case 0:
                    _dirText = "|";
                    break;

                case 1:
                    _dirText = "—";
                    break;

                case 2:
                    _dirText = "／";
                    break;

                case 3:
                    _dirText = "＼";
                    break;
            }

            if (debug) Debug.Log("[" + _dirText + "] type = " + type + " / " + _kifu + " = " + _s);

            if (_kifu == "_aa__" || _kifu == "__aa_" || _kifu == "_a_a_") a2++; //活二次數
            if (_kifu == "_aaa_" || _kifu == "_aa_a_" || _kifu == "_a_aa_") a3++; //活三次數
            if (_kifu == "aaaa_" || _kifu == "_aaaa" || _kifu == "aa_aa" || _kifu == "a_aaa" || _kifu == "aaa_a") a3++; //死四次數
        }

        if (a2 == 2) //雙活二
        {
            if (debug) Debug.Log("雙活二額外加分");
            return (score + 200);
        }
        if (a2 >= 3) //三活二
        {
            if (debug) Debug.Log("三活二額外加分");
            return (score + 500);
        }
        if (a3 >= 2) //雙活三
        {
            if (debug) Debug.Log("雙活三額外加分");
            return (score + 8000);
        }
        if (d4 >= 2) //雙死四
        {
            if (debug) Debug.Log("雙死四額外加分");
            return (score + 50000);
        }
        if (d4 >= 1 && a3 >= 1) //死四+活三
        {
            if (debug) Debug.Log("死四活三額外加分");
            return (score + 30000);
        }

        if (score > c_maxScore) score = c_maxScore;
        return score;
    }

    //建構棋譜評分表(自動增加對稱的棋譜, 可省略一半code)
    private void AddScoreTable(string kifu, int score)
    {
        string reverseKifu = "";
        for (int i = kifu.Length - 1; i >= 0; i--)
        {
            reverseKifu += kifu.Substring(i, 1);
        }

        d_ScoreTable.Add(kifu, score); //增加棋譜
        if (reverseKifu != kifu) d_ScoreTable.Add(reverseKifu, score); //增加棋譜(反轉)

        highScoreKifuList.Add(kifu); //排進最高分棋譜順序列表
        if (reverseKifu != kifu) highScoreKifuList.Add(reverseKifu);
    }

    //評分單一方向棋譜分數
    private int CheckOneLine(Vector2 pos, int[,] grid, int[] offset, int type, bool debug, out string targetKifu)
    {
        string shape = "a";
        string _shape = "A";

        int i = 0;
        int j = 0;
        int plus_space = 2; //往正值方向遍歷時, 只允許重複出現2個空格
        int minus_space = 2; //往負值方向遍歷時, 只允許重複出現2個空格
        bool plusStop = false; //正值遍歷中止
        bool minusStop = false; //負值遍歷中止

        while (!plusStop || !minusStop)
        {
            i += offset[0];
            j += offset[1];

            if (!plusStop && (pos.x + i >= 0 && pos.x + i <= ChessBehavior.Instance.size.x) && (pos.y + j >= 0 && pos.y + j <= ChessBehavior.Instance.size.y) && plus_space >= 1) //往正值遍歷
            {
                if (grid[(int)pos.x + i, (int)pos.y + j] == type)
                {
                    shape = shape + "a";
                    if (debug) Debug.Log("grid[" + (pos.x + i) + "," + (pos.y + j) + "] = " + type);
                    _shape = _shape + "a";
                }
                else if (grid[(int)pos.x + i, (int)pos.y + j] == 0)
                {
                    shape = shape + "_";
                    if (debug) Debug.Log("grid[" + (pos.x + i) + "," + (pos.y + j) + "] = 0");
                    _shape = _shape + "O";
                    plus_space--;
                }
                else plusStop = true;
            }
            else plusStop = true;

            if (!minusStop && (pos.x - i >= 0 && pos.x - i <= ChessBehavior.Instance.size.x) && (pos.y - j >= 0 && pos.y - j <= ChessBehavior.Instance.size.y) && minus_space >= 1) //往負值遍歷
            {
                if (grid[(int)pos.x - i, (int)pos.y - j] == type)
                {
                    shape = "a" + shape;
                    if (debug) Debug.Log("grid[" + (pos.x - i) + "," + (pos.y - j) + "] = " + type);
                    _shape = "a" + _shape;
                }
                else if (grid[(int)pos.x - i, (int)pos.y - j] == 0)
                {
                    shape = "_" + shape;
                    if (debug) Debug.Log("grid[" + (pos.x - i) + "," + (pos.y - j) + "] = 0");
                    _shape = "O" + _shape;
                    minus_space--;
                }
                else minusStop = true;
            }
            else minusStop = true;

            if (shape.Length >= 7 || (plusStop && minusStop)) break;
        }

        for (int k = 0; k < highScoreKifuList.Count; k++)
        {
            if (shape.Contains(highScoreKifuList[k]))
            {
                targetKifu = highScoreKifuList[k];
                if (debug) Debug.Log("plusStop: " + plusStop + " /minusStop:" + minusStop + " /shape.Length:" + shape.Length + " /shape:" + _shape + " /kifu:" + highScoreKifuList[k]);
                return d_ScoreTable[highScoreKifuList[k]];
            }
        }

        targetKifu = "";
        return 0;
    }

}
