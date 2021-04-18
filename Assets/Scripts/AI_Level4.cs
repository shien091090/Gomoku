using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxMinNode
{
    public int score; //分數
    public Vector2 pos; //位置
    public int layer = 0; //所在層數
    public string number = null; //編號
    public List<MaxMinNode> childBranch = new List<MaxMinNode>();
    public MaxMinNode headNode;
    public List<int> nodeAcc = new List<int>(); //node往上累加分數的紀錄

    public MaxMinNode(int s, Vector2 p, int l, string n)
    {
        score = s;
        pos = p;
        layer = l;
        number = n;
    }

    public void TestMessage()
    {
        for (int i = 0; i < nodeAcc.Count; i++)
        {
            Debug.Log("[" + number + "] : [" + pos.x + "," + pos.y + "]=" + score + "(累計=[" + i + "]" + nodeAcc[i] + ")");
        }

    }
}

public class AI_Level4 : IAIScriptInterface
{
    public enum ScoreMode
    {
        此陣營最高分, 對方最高分, 雙方合計最高分
    }
    private Dictionary<string, int> d_ScoreTable = new Dictionary<string, int>(); //棋譜評分表
    private List<string> highScoreKifuList = new List<string>(); //最高分棋譜排列表
    private int aiChessType; //AI編號
    private int playerType; //玩家編號
    private const int c_maxScore = 1000000; //最高分(常數)


    public AI_Level4(int chessType) //建構子
    {
        aiChessType = chessType;
        playerType = chessType == 1 ? 2 : 1;

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
        Debug.Log("--夭壽強AI--");
        Debug.Log("type[1] = 黑子 / pos[" + pos.x + "," + pos.y + "]總分 = " + ScoreChessPoint(pos, ChessBehavior.Instance.grid, 1, true, false));
        Debug.Log("type[2] = 白子 / pos[" + pos.x + "," + pos.y + "]總分 = " + ScoreChessPoint(pos, ChessBehavior.Instance.grid, 2, true, false));
    }

    //下棋演算法
    public Vector2 ChessOperator()
    {
        if (ChessBehavior.Instance.chessLine.Count == 0) return new Vector2(7, 7); //第一顆子的場合, 下正中央

        List<MaxMinNode> selections = BuildNodeList(ChessBehavior.Instance.grid, 4, 3, "[NodeList]", true, null); //展開極大極小演算法並排序分數高低
        selections.Sort((MaxMinNode x, MaxMinNode y) =>
        {
            if (x.nodeAcc[0] < y.nodeAcc[0]) return 1;
            if (x.nodeAcc[0] > y.nodeAcc[0]) return -1;
            return 0;
        }
        );

        //if (selections.Count == 0) //若演算結果為null(無自己的棋子, 也就是對方下了第一步, 而輪到自己下第二步時), 下在對方的高分點
        //{
        //    return ReturnHighestList(ChessBehavior.Instance.grid, playerType, 1, 0, null)[0].pos;
        //}

        //if (selections[0].nodeAcc[0] <= -50000)
        //{
        //    return ReturnHighestList(ChessBehavior.Instance.grid, playerType, 1, 0, null)[0].pos;
        //}

        return selections[0].pos;
    }

    public List<MaxMinNode> BuildNodeList(int[,] grid, int amount, int deep, string numberName, bool assOrAway, MaxMinNode headNode)
    {
        List<MaxMinNode> _nodeList = new List<MaxMinNode>();
        bool breakBranch = false; //是否斷枝

        if (assOrAway && deep > 0) //我方回合且深度大於0時(非最後一項), 抓出amount個分數最大值位置
        {
            _nodeList = ReturnHighestList(grid, aiChessType, ScoreMode.雙方合計最高分, amount, deep, numberName);
        }
        else if (assOrAway && deep == 0) //我方回合且深度等於0時(最後一項), 只取最大分數位置
        {
            //_nodeList = ReturnHighestList(grid, aiChessType, ScoreMode.雙方合計最高分, 1, deep, numberName);
            _nodeList = Deduce(grid, aiChessType, deep, numberName);
        }
        else if (!assOrAway) //對方回合時, 使用特殊演算法去預測可能會下的位置
        {
            _nodeList = Deduce(grid, playerType, deep, numberName);

            for (int i = 0; i < _nodeList.Count; i++)
            {
                _nodeList[i].score *= -1;
            }
        }

        //若分數在分數上限值以上時剪枝, 不繼續推演
        for (int i = 0; i < _nodeList.Count; i++)
        {
            if (Mathf.Abs(_nodeList[i].score) >= c_maxScore) breakBranch = true;
        }

        for (int i = 0; i < _nodeList.Count; i++) //生成的所有node再往下生成分支
        {
            if (deep > 0) //若深度大於0(尚未到最後一項), 則向下生成分枝
            {
                int[,] nextGrid = (int[,])grid.Clone();
                nextGrid[(int)_nodeList[i].pos.x, (int)_nodeList[i].pos.y] = assOrAway ? aiChessType : playerType; //推演棋盤

                if (!breakBranch) _nodeList[i].childBranch = BuildNodeList(nextGrid, amount, deep - 1, _nodeList[i].number, !assOrAway, _nodeList[i]);
            }

            if (_nodeList[i].nodeAcc.Count == 0) _nodeList[i].nodeAcc.Add(_nodeList[i].score); //若累計分數的項目數等於0(最後一項), 則將自己的分數加進累計總分
            else //若累計分數的項目數大於1(前面已有子分枝將分數加上來), 則只要對累計總分再累加自己的分數上去即可
            {
                for (int j = 0; j < _nodeList[i].nodeAcc.Count; j++)
                {
                    _nodeList[i].nodeAcc[j] += _nodeList[i].score;
                    if (_nodeList[i].nodeAcc[j] >= c_maxScore) _nodeList[i].nodeAcc[j] = c_maxScore; //設置分數上限
                }
            }

            if (headNode != null) //若上層node不為null, 則將累計總分往母分枝丟, 若項目數為多個(來自不同子分枝的累加總分), 則只挑出最大值
            {
                if (_nodeList[i].nodeAcc.Count > 1) _nodeList[i].nodeAcc.Sort();
                headNode.nodeAcc.Add(_nodeList[i].nodeAcc[_nodeList[i].nodeAcc.Count - 1]);
            }
            else _nodeList[i].nodeAcc.Sort();

            //_nodeList[i].TestMessage();
        }

        return _nodeList;
    }

    public List<MaxMinNode> Deduce(int[,] grid, int type, int layer, string numberName)
    {
        int assType = type; //推演方type num
        //int awayType = type == 1 ? 2 : 1; //對方type num

        List<MaxMinNode> highestPoints_ass = ReturnHighestList(grid, assType, ScoreMode.此陣營最高分, 1, layer, numberName); //取推演方最高分格
        List<MaxMinNode> highestPoints_away = ReturnHighestList(grid, assType, ScoreMode.對方最高分, 1, layer, numberName); //取對方最高分格
        List<MaxMinNode> highestPoints_total = ReturnHighestList(grid, assType, ScoreMode.雙方合計最高分, 1, layer, numberName); //取雙方合計最高分格

        if (highestPoints_ass[0].score >= c_maxScore) return highestPoints_ass; //若推演方必勝則下在推演方最高分格上
        else if (highestPoints_away[0].score >= c_maxScore) return highestPoints_away; //若對方必勝則下在對方的最高分格上
        else if (highestPoints_ass[0].score >= d_ScoreTable["_aaaa_"] || (highestPoints_ass[0].score >= d_ScoreTable["aaaa_"] + d_ScoreTable["_aa__"] && highestPoints_ass[0].score > highestPoints_away[0].score)) return highestPoints_ass; //推演方有絕對優勢時下在推演方最高分格上
        else if (highestPoints_away[0].score >= d_ScoreTable["_aaaa_"] || (highestPoints_away[0].score >= d_ScoreTable["aaaa_"] + d_ScoreTable["_aa__"] && highestPoints_away[0].score > highestPoints_away[0].score)) return highestPoints_away; //對方有絕對優勢時下在對方最高分格上
        else return highestPoints_total; //其他狀況則下在雙方合計最高分格上
    }

    //依據棋局狀態找出最高分點(依ScoreMode排序分數高低之後, 再以陣營(type)個別對格子進行評分)
    private List<MaxMinNode> ReturnHighestList(int[,] grid, int type, ScoreMode mode, int amount, int layer, string numberName)
    {
        List<MaxMinNode> nodeList = new List<MaxMinNode>();

        //記錄所有棋盤格的分數
        for (int i = 0; i <= grid.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= grid.GetUpperBound(0); j++)
            {
                if (grid[j, i] == 0)
                {
                    int score = 0;

                    switch (mode)
                    {
                        case ScoreMode.此陣營最高分:
                            score += ScoreChessPoint(new Vector2(j, i), grid, type, false, false);
                            break;

                        case ScoreMode.對方最高分:
                            int awayType = type == 1 ? 2 : 1;
                            score += ScoreChessPoint(new Vector2(j, i), grid, awayType, false, false);
                            break;

                        case ScoreMode.雙方合計最高分:
                            score += ScoreChessPoint(new Vector2(j, i), grid, 1, false, false);
                            score += ScoreChessPoint(new Vector2(j, i), grid, 2, false, false);
                            break;
                    }

                    if (score != 0) //只有分數不為0時才會採用至List
                    {
                        if (score > c_maxScore) score = c_maxScore; //設置上限

                        MaxMinNode _n = new MaxMinNode(score, new Vector2(j, i), layer, numberName);
                        nodeList.Add(_n);
                    }
                }
            }
        }

        //排序
        nodeList.Sort((MaxMinNode x, MaxMinNode y) =>
        {
            if (x.score < y.score) return 1;
            if (x.score > y.score) return -1;
            return 0;
        }
        );

        //只保留amount個最大值
        if (nodeList.Count != 0) nodeList.RemoveRange(amount, nodeList.Count - amount);

        //設定編號名稱
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].number = nodeList[i].number + i.ToString();
        }

        if(mode != ScoreMode.此陣營最高分) //根據陣營(type)改變node分數(但如果mode為"此陣營最高分", 則無需重複評分)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                nodeList[i].score = ScoreChessPoint(new Vector2(nodeList[i].pos.x, nodeList[i].pos.y), grid, type, false, false);
            }
        }

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
