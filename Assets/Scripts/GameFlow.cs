//遊戲流程控制
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

#region 額外Class ---------------------------------------------------------------------------------------------------------------
#endregion

public class GameFlow : MonoBehaviour
{
    #region 變數宣告 ---------------------------------------------------------------------------------------------------------------

    private static GameFlow _instance; //單例模式
    public static GameFlow Instance
    {
        get { return _instance; }
    }

    #endregion
    #region 列舉值(enum) ---------------------------------------------------------------------------------------------------------------
    #endregion
    #region 內建方法 ---------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
        UIController.Instance.chessFocus.Add(ChessType.黑子, UIController.Instance.rot_blackChess);
        UIController.Instance.chessFocus.Add(ChessType.白子, UIController.Instance.rot_whiteChess);
    }

    void Start()
    {
        UIController.Instance.GameSetting_Load(); //讀取設定值

        StartCoroutine(Opening()); //開場動畫

    }

    #endregion
    #region 自訂方法 ---------------------------------------------------------------------------------------------------------------

    #endregion
    #region 協同程序 ---------------------------------------------------------------------------------------------------------------

    //開頭過場
    private IEnumerator Opening()
    {
        GameObject go = UIController.Instance.animScript_mainMenu.gameObject;
        CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();

        AudioManagerScript.Instance.PlayAudioClip("music_main_menu"); //撥放音樂

        go.SetActive(false);
        UIController.Instance.animSript_curtain.AnimationCommand(new AlphaScript(2.5f, 1, 0.7f), new AnimationBehavior(PlayMode.狀態延續, new ActionSetting(() => { UIController.Instance.animSript_curtain.GetComponent<Image>().enabled = true; }, TaskMode.僅開始時)));
        UIController.Instance.animSript_stagePanel.PlayAnimation("EnterStage", new AnimationBehavior(PlayMode.狀態延續, new ActionSetting(() => { UIController.Instance.animSript_stagePanel.GetComponent<Image>().enabled = false; }, TaskMode.僅結束時)), false);

        yield return new WaitForSeconds(2.3f);

        UIController.Instance.animScript_mainMenu.AnimationCommand(new AlphaScript(1, 0, 1), new AnimationBehavior(PlayMode.狀態延續, new ActionSetting(() => { canvasGroup.blocksRaycasts = true; }, TaskMode.僅結束時)));
    }

    //遊戲開始
    public IEnumerator GameStart()
    {
        UIController.Instance.animScript_mainMenu.PlayAnimation("MainMenuGameStart", new AnimationBehavior(PlayMode.狀態延續), false);
        UIController.Instance.animSript_curtain.AnimationCommand(new AlphaScript(0.25f, 0), new AnimationBehavior(PlayMode.狀態延續) { disableObject = true });

        yield return new WaitForSeconds(1.1f);

        AudioManagerScript.Instance.PlayAudioClip("music_playing"); //撥放音樂
        UIController.Instance.animScript_gameMenu.PlayAnimation("GamingMenu_FadeIn", new AnimationBehavior(PlayMode.狀態延續), false);

        yield return new WaitForSeconds(1.2f);

        UIController.Instance.animScrip_interlude.PlayAnimation("Interlude_GameStart", new AnimationBehavior(PlayMode.初始化, new ActionSetting(
            () =>
            {
                UIController.Instance.cg_gameMenu.blocksRaycasts = true;
                ChessBehavior.Instance.GameInitialization();
            },
            TaskMode.僅結束時)), false);
    }

    //返回主選單
    public IEnumerator BackToMenu()
    {
        AudioManagerScript.Instance.Stop(0);

        UIController.Instance.rot_blackChess.SetRotating(false); //停止棋盆旋轉
        UIController.Instance.rot_whiteChess.SetRotating(false);

        ChessBehavior.Instance.ai_black = null; //AI重置
        ChessBehavior.Instance.ai_white = null;

        yield return new WaitForSeconds(0.5f);

        UIController.Instance.animScript_gameMenu.PlayAnimation("GamingMenu_FadeOut", new AnimationBehavior(PlayMode.狀態延續) { disableObject = true }, false); //遊戲選單淡出

        Vector2 blackPoint = UIController.Instance.rot_blackChess.gameObject.transform.position; //黑子棋盆位置
        Vector2 whitePoint = UIController.Instance.rot_whiteChess.gameObject.transform.position; //白子棋盆位置
        float freq = 0.02f; //頻率

        ChessBehavior.Instance.lastChessIcon.SetActive(false); //隱藏最後下棋點提示
        for (int i = 0; i < ChessBehavior.Instance.chessLine.Count; i++) //清除棋盤上的所有棋子(動畫展示)
        {
            ChessType t = ChessBehavior.Instance.chessLine[i].GetComponent<ChessAttribute>().thisChess;
            AnimationScript animationScript = ChessBehavior.Instance.chessLine[i].GetComponent<AnimationScript>();
            if (animationScript.animationPlaying)
            {
                animationScript.animationPlaying = false;
            }
            animationScript.AnimationCommand(new PositionScript(UIController.Instance.chessReturnCurve, t == ChessType.白子 ? whitePoint : blackPoint, true), new AnimationBehavior(PlayMode.狀態延續) { destroyThis = true });
            AudioManagerScript.Instance.PlayAudioClip("chess_return");
            yield return new WaitForSeconds(freq);
        }

        ChessBehavior.Instance.chessLine = new List<GameObject>(); //初始化棋子隊列

        yield return new WaitForSeconds(UIController.Instance.chessReturnCurve.keys[UIController.Instance.chessReturnCurve.keys.Length - 1].time);

        UIController.Instance.animScript_mainMenu.PlayAnimation("MainMenuGameOver", new AnimationBehavior(PlayMode.狀態延續, new ActionSetting(() => { UIController.Instance.cg_mainMenu.blocksRaycasts = true; }, TaskMode.僅結束時)), false); //選單UI復歸

        yield return new WaitForSeconds(0.9f);

        UIController.Instance.animSript_curtain.AnimationCommand(new AlphaScript(0.35f, 0.7f), new AnimationBehavior(PlayMode.狀態延續)); //遮布淡入
        AudioManagerScript.Instance.PlayAudioClip("music_main_menu"); //撥放音樂

    }

    //再來一局
    public IEnumerator PlayAgain()
    {
        UIController.Instance.rot_blackChess.SetRotating(false); //停止棋盆旋轉
        UIController.Instance.rot_whiteChess.SetRotating(false);

        Vector2 blackPoint = UIController.Instance.rot_blackChess.gameObject.transform.position; //黑子棋盆位置
        Vector2 whitePoint = UIController.Instance.rot_whiteChess.gameObject.transform.position; //白子棋盆位置
        float freq = 0.02f; //頻率

        ChessBehavior.Instance.lastChessIcon.SetActive(false); //隱藏最後下棋點提示
        for (int i = 0; i < ChessBehavior.Instance.chessLine.Count; i++) //清除棋盤上的所有棋子(動畫展示)
        {
            ChessType t = ChessBehavior.Instance.chessLine[i].GetComponent<ChessAttribute>().thisChess;
            AnimationScript animationScript = ChessBehavior.Instance.chessLine[i].GetComponent<AnimationScript>();
            if (animationScript.animationPlaying)
            {
                animationScript.animationPlaying = false;
            }
            animationScript.AnimationCommand(new PositionScript(UIController.Instance.chessReturnCurve, t == ChessType.白子 ? whitePoint : blackPoint, true), new AnimationBehavior(PlayMode.狀態延續) { destroyThis = true });
            AudioManagerScript.Instance.PlayAudioClip("chess_return");
            yield return new WaitForSeconds(freq);
        }

        ChessBehavior.Instance.turn = UIController.Instance.senteChess; //設定先手
        ChessBehavior.Instance.chessLine = new List<GameObject>(); //初始化棋子隊列

        UIController.Instance.retractRemain = UIController.retractTimes; //重置悔棋次數
        UIController.Instance.tx_retractRemain.text = "剩下  " + UIController.Instance.retractRemain + "  次";
        UIController.Instance.RetractButtonState(false); //因棋盤上的棋子數<2, 禁用悔棋按鈕

        yield return new WaitForSeconds(0.3f);

        UIController.Instance.animScrip_interlude.PlayAnimation("Interlude_GameStart", new AnimationBehavior(PlayMode.初始化, new ActionSetting(
            () =>
            {
                UIController.Instance.cg_gameMenu.blocksRaycasts = true;
                ChessBehavior.Instance.GameInitialization();
            },
            TaskMode.僅結束時)), false);
    }

    #endregion
}
