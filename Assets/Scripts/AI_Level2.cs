using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//普通AI : 進階棋型&積分判斷

public class PointPack //棋盤格資訊
{
    public int[,] originGrid; //棋盤狀態
    public int[,] newGrid; //更新後棋盤狀態
    public Vector2 pos; //棋盤格座標
    public int score = 0; //分數
    public int chessTypeNum = -1; //棋子陣營編號

    public PointPack (int[,] grid) //建構子
    {
        originGrid = grid;
    }

    public void RecordInfo(Vector2 p, int s, int type) //紀錄資訊
    {
        pos = p;
        score = s;
        chessTypeNum = type;

        newGrid = (int[,])originGrid.Clone(); //紀錄棋盤格變動
        newGrid[(int)p.x, (int)p.y] = type;
    }
}

public class AI_Level2 : IAIScriptInterface
{
    private Dictionary<string, int> scoreTable = new Dictionary<string, int>(); //評分表
    private List<string> scoreSort = new List<string>(); //分數排序
    private const int MaxValue = 1000000; //(常數)最高分數
    private int chessTypeNum; //棋子陣營編號

    //建構子
    public AI_Level2(int n)
    {
        chessTypeNum = n;
        List<string> shapes = new List<string>()
        {
            "aa___",
            "___aa",
            "_aa__",
            "__aa_",
            "_a_a_",

            "aaa__",
            "__aaa",
            "_aaa_",
            "_aa_a_",
            "_a_aa_",
            "_a_aa",
            "a_aa_",
            "_aa_a",
            "aa_a_",

            "aaaa_",
            "_aaaa",
            "_aaaa_",
            "aaa_a",
            "_aaa_a",
            "aaa_a_",
            "a_aaa",
            "_a_aaa",
            "a_aaa_",

            "aaaaa"
        };

        scoreTable.Add("aa___", 50);
        scoreTable.Add("___aa", 50);
        scoreTable.Add("_aa__", 150);
        scoreTable.Add("__aa_", 150);
        scoreTable.Add("_a_a_", 100);

        scoreTable.Add("aaa__", 300);
        scoreTable.Add("__aaa", 300);
        scoreTable.Add("_aaa_", 850);
        scoreTable.Add("_aa_a_", 700);
        scoreTable.Add("_a_aa_", 700);
        scoreTable.Add("_a_aa", 200);
        scoreTable.Add("a_aa_", 200);
        scoreTable.Add("_aa_a", 200);
        scoreTable.Add("aa_a_", 200);

        scoreTable.Add("aaaa_", 1500);
        scoreTable.Add("_aaaa", 1500);
        scoreTable.Add("_aaaa_", MaxValue);
        scoreTable.Add("aaa_a", 1000);
        scoreTable.Add("_aaa_a", 1350);
        scoreTable.Add("aaa_a_", 1350);
        scoreTable.Add("a_aaa", 1000);
        scoreTable.Add("_a_aaa", 1350);
        scoreTable.Add("a_aaa_", 1350);

        scoreTable.Add("aaaaa", MaxValue);

        while (shapes.Count > 0) //分數排序
        {
            int biggestIndex = 0;
            int biggestScore = 0;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (scoreTable.ContainsKey(shapes[i]))
                {
                    if (scoreTable[shapes[i]] >= biggestScore)
                    {
                        biggestIndex = i;
                        biggestScore = scoreTable[shapes[i]];
                    }
                }
            }

            scoreSort.Add(shapes[biggestIndex]);
            shapes.Remove(shapes[biggestIndex]);
        }
    }

    //測試方法
    public void TestMethod(Vector2 pos)
    {
        Debug.Log("--普通AI--");
    }

    //下棋點演算
    public Vector2 ChessOperator()
    {
        if (ChessBehavior.Instance.chessLine.Count == 0) return new Vector2(7, 7); //若為第一顆子則下正中央

        int[,] grid = ChessBehavior.Instance.grid; //取得棋局狀態
        int selfTotal = 0; //自身全局總分
        PointPack selfHighest = new PointPack(grid); //自身最高分點資訊
        int matchTotal = 0; //對方全局總分
        PointPack matchHighest = new PointPack(grid); //對方最高分點資訊
        int sumTotal = 0; //雙方全局總分
        PointPack sumHighest = new PointPack(grid); //雙方總和最高分點資訊

        for (int i = 0; i <= grid.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= grid.GetUpperBound(0); j++)
            {
                if (grid[j, i] == 0)
                {
                    Vector2 pos = new Vector2(j, i);
                    int selfScore = GetOnePointScore(grid, pos, chessTypeNum);
                    int matchScore = GetOnePointScore(grid, pos, (chessTypeNum == 1 ? 2 : 1));

                    if (selfScore + matchScore > 0)
                    {
                        selfTotal = selfTotal + selfScore > MaxValue ? MaxValue : selfTotal + selfScore;
                        matchTotal = matchTotal + matchScore > MaxValue ? MaxValue : matchTotal + matchScore;

                        if (selfHighest.score == 0 || selfScore > selfHighest.score) selfHighest.RecordInfo(pos, selfScore, chessTypeNum);
                        if (matchHighest.score == 0 || matchScore > matchHighest.score) matchHighest.RecordInfo(pos, matchScore, (chessTypeNum == 1 ? 2 : 1));
                        if (sumHighest.score == 0 || selfScore + matchScore > sumHighest.score) sumHighest.RecordInfo(pos, ((selfScore + matchScore) > MaxValue ? MaxValue : (selfScore + matchScore)), -1);
                    }
                }
            }
        }
        sumTotal = selfTotal + matchTotal > MaxValue ? MaxValue : selfTotal + matchTotal;

        //Debug.Log("自身全局總分 : " + ( selfTotal >= MaxValue ? "Max" : selfTotal.ToString()) + "/ 對方全局總分 : " + (matchTotal >= MaxValue ? "Max" : matchTotal.ToString()) + "/ 雙方全局總分 : " + (sumTotal >= MaxValue ? "Max" : sumTotal.ToString()));
        //Debug.Log("[自身最高分點資訊] 座標 : (" + selfHighest.pos.x + ", " + selfHighest.pos.y + ") / 分數 : " + (selfHighest.score >= MaxValue ? "Max" : selfHighest.score.ToString()));
        //Debug.Log("[對方最高分點資訊] 座標 : (" + matchHighest.pos.x + ", " + matchHighest.pos.y + ") / 分數 : " + (matchHighest.score >= MaxValue ? "Max" : matchHighest.score.ToString()));
        //Debug.Log("[雙方總和最高分點資訊] 座標 : (" + sumHighest.pos.x + ", " + sumHighest.pos.y + ") / 分數 : " + (sumHighest.score >= MaxValue ? "Max" : sumHighest.score.ToString()));

        int[,] nextGrid = (int[,])sumHighest.newGrid.Clone();
        int nextMatchTotal = 0;
        for (int i = 0; i <= nextGrid.GetUpperBound(1); i++)
        {
            for (int j = 0; j <= nextGrid.GetUpperBound(0); j++)
            {
                if (nextGrid[j, i] == 0)
                {
                    Vector2 pos = new Vector2(j, i);
                    int matchScore = GetOnePointScore(nextGrid, pos, (chessTypeNum == 1 ? 2 : 1));

                    nextMatchTotal = nextMatchTotal + matchScore > MaxValue ? MaxValue : nextMatchTotal + matchScore;
                }
            }
        }

        //Debug.Log("對方全局總分差 : " + (matchTotal - nextMatchTotal));

        return sumHighest.pos;
    }

    //取得某陣營其中一格的分數
    public int GetOnePointScore(int[,] grid, Vector2 pos, int typeNum)
    {
        int score = 0; //該格總和分數
        List<string> overlayShapes = new List<string>(); //該格疊加棋型
        int live_two = 0; //活二
        int live_three = 0; //活三
        int dead_four = 0; //死四

        overlayShapes.Add(CheckOneLine(grid, pos, new int[] { 0, 1 }, typeNum)); //紀錄棋型(共四種方向)
        overlayShapes.Add(CheckOneLine(grid, pos, new int[] { 1, 0 }, typeNum));
        overlayShapes.Add(CheckOneLine(grid, pos, new int[] { 1, 1 }, typeNum));
        overlayShapes.Add(CheckOneLine(grid, pos, new int[] { 1, -1 }, typeNum));

        for (int i = 0; i < overlayShapes.Count; i++)
        {
            if (overlayShapes[i] != null) score += scoreTable[overlayShapes[i]];

            if (overlayShapes[i] == "_aa__" || overlayShapes[i] == "__aa_" || overlayShapes[i] == "_a_a_") live_two++;
            if (overlayShapes[i] == "_aaa_" || overlayShapes[i] == "_aa_a_" || overlayShapes[i] == "_a_aa_") live_three++;
            if (overlayShapes[i] == "aaaa_" || overlayShapes[i] == "_aaaa" || overlayShapes[i] == "aaa_a" || overlayShapes[i] == "_aaa_a" || overlayShapes[i] == "aaa_a_" || overlayShapes[i] == "a_aaa" || overlayShapes[i] == "_a_aaa" || overlayShapes[i] == "a_aaa_") dead_four++;
        }

        if (live_two >= 2 && live_two < 4) score += scoreTable["_aaa_"] * (live_two - 1); //多活二加成
        if (live_three + dead_four >= 2 || live_two >= 4) score += 50000;
        if (score > MaxValue) score = MaxValue;

        return score;
    }

    //檢測棋子連線並評分(輸出最高分棋型)
    public string CheckOneLine(int[,] grid, Vector2 pos, int[] offset, int type)
    {
        string shape = "a";
        bool deadStep = true;

        //往右遍歷
        for (int i = offset[0], j = offset[1], k = 0; (pos.x + i >= 0 && pos.x + i <= ChessBehavior.Instance.size.x) && (pos.y + j >= 0 && pos.y + j <= ChessBehavior.Instance.size.y) && k <= 3; i += offset[0], j += offset[1], k++)
        {
            if (grid[(int)pos.x + i, (int)pos.y + j] == type)
            {
                shape = shape + "a";
            }
            else if (grid[(int)pos.x + i, (int)pos.y + j] == 0)
            {
                shape = shape + "_";
                deadStep = false;
            }
            else
            {
                break;
            }
        }
        //往左遍歷
        for (int i = -offset[0], j = -offset[1], k = 0; (pos.x + i >= 0 && pos.x + i <= ChessBehavior.Instance.size.x) && (pos.y + j >= 0 && pos.y + j <= ChessBehavior.Instance.size.y) && k <= 3; i -= offset[0], j -= offset[1], k++)
        {
            if (grid[(int)pos.x + i, (int)pos.y + j] == type)
            {
                shape = "a" + shape;
            }
            else if (grid[(int)pos.x + i, (int)pos.y + j] == 0)
            {
                shape = "_" + shape;
                deadStep = false;
            }
            else
            {
                break;
            }
        }

        if (shape.Contains("aaaaa")) //必勝狀況評比為最高分
        {
            return "aaaaa";
        }
        else if (shape.Length >= 5 && !deadStep) //評分任何有可能連成五子的棋局
        {
            for (int i = 0; i < scoreSort.Count; i++)
            {
                string key = scoreSort[i];
                if (shape.Contains(key))
                {
                    return key;
                }
            }
        }
        return null;
    }

}
