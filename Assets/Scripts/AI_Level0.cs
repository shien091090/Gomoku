using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//智障AI : 隨便亂下

public class AI_Level0 : IAIScriptInterface
{
    public Vector2 ChessOperator()
    {
        List<Vector2> space = new List<Vector2>();

        for (int i = 0; i <= ChessBehavior.Instance.grid.GetUpperBound(1); i++) //取得空白棋格
        {
            for (int j = 0; j <= ChessBehavior.Instance.grid.GetUpperBound(0); j++)
            {
                if (ChessBehavior.Instance.grid[j, i] == 0)
                {
                    space.Add(new Vector2(j, i));
                }
            }
        }

        int rnd = Random.Range(0, space.Count);

        return space[rnd];
    }

    public void TestMethod(Vector2 pos) { }

}
