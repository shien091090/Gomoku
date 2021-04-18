//主選單UI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Networking;

public class UIController : MonoBehaviour
{
    #region 列舉值(Enum) ----------------------------------------------------------------------------------------------------------

    //選項淡入模式
    public enum SelectionFadeInMode
    {
        橫向放射, 單純漸顯, 降下
    }

    #endregion
    #region 變數宣告 --------------------------------------------------------------------------------------------------------------

    private static UIController _instance; //單例模式
    public static UIController Instance
    {
        get { return _instance; }
    }

    [Header("主選單面板")]
    public List<string> menuList = new List<string>() { "開局設定", "遊戲設定", "聯網模式" }; //主選單項目列表
    public AnimationScript[] childMenuScripts; //子選單的AnimationScript集合
    public AnimationScript animScript_mainMenu; //主選單動畫腳本
    public AnimationScript animSript_curtain; //布幕動畫腳本
    public AnimationScript animSript_stagePanel; //場景畫面動畫腳本
    public AnimationScript animScrip_interlude; //過場動畫腳本
    public int menuSelectedIndex = 0; //所選擇選單索引
    public Text tx_switcherTitle; //選項文字
    public Text tx_switcherTitle_hide; //選項文字(隱藏)
    public AnimationCurve changeMenuByCurve; //切換選單時的特效曲線
    [Header("過場動畫物件")]
    public GameObject interlude_gameStart; //開場動畫
    [Header("開局設定")]
    public ChessType senteChess; //先手陣營
    public PlayerType blackPlayer; //黑子玩家類型
    public PlayerType whitePlayer; //白子玩家類型
    public AiType blackAi; //黑子AI類型
    public AiType whiteAi; //白子AI類型
    public Player player_black; //黑子玩家
    public Player player_white; //白子玩家
    public AnimationScript animScript_changeBlackPlayer; //動畫撥放腳本(選擇黑子玩家類型)
    public AnimationScript animScript_changeWhitePlayer; //動畫撥放腳本(選擇白子玩家類型)
    public AnimationScript animScript_changeBlackAi; //動畫撥放腳本(選擇黑子AI類型)
    public AnimationScript animScript_changeWhiteAi; //動畫撥放腳本(選擇白子AI類型)
    [Header("棋子陣營")]
    public Sprite whiteChess; //白棋
    public Sprite blackChess; //黑棋
    public Text tx_blackLabel; //黑子先後手標籤
    public Text tx_blackLabel_hide; //黑子先後手標籤(隱藏)
    public Text tx_whiteLabel; //白子先後手標籤
    public Text tx_whiteLabel_hide; //白子先後手標籤(隱藏)
    public AutoRotating rot_blackChess; //黑子圖示旋轉
    public AutoRotating rot_whiteChess; //白子圖示旋轉
    public Dictionary<ChessType, AutoRotating> chessFocus = new Dictionary<ChessType, AutoRotating>(); //取得目前下棋陣營的對應圖示旋轉腳本
    public AnimationCurve chessReturnCurve; //棋子回家動畫曲線
    [Header("玩家類型")]
    public List<string> playerTypeList = new List<string>(); //玩家類型字串List
    public Text tx_blackPlayerType; //黑子玩家類型
    public Text tx_blackPlayerType_hide; //黑子玩家類型(隱藏)
    public Text tx_whitePlayerType; //白子玩家類型
    public Text tx_whitePlayerType_hide; //白子玩家類型(隱藏)
    [Header("AI類型")]
    public List<string> aiTypeList = new List<string>(); //AI類型字串List
    public Button[] blackSelectionButtons; //黑子選擇按鈕
    public Button[] whiteSelectionButtons; //白子選擇按鈕
    public Text tx_blackAiType; //黑子AI類型
    public Text tx_blackAiType_hide; //黑子AI類型(隱藏)
    public Text tx_whiteAiType; //白子AI類型
    public Text tx_whiteAiType_hide; //白子AI類型(隱藏)
    public AnimationScript animScript_blackAiButtonFrame; //黑子AI按鈕框架的動畫腳本
    public AnimationScript animScript_whiteAiButtonFrame; //白子AI按鈕框架的動畫腳本
    [Header("對話框")]
    public SpeechBalloonScript speechBalloonScript_typeA; //對話框腳本A(一般選擇)
    public SpeechBalloonScript speechBalloonScript_typeB; //對話框腳本B(遊戲結束時)
    public SpeechBalloonScript speechBalloonScript_typeC; //對話框腳本C(遊戲設定)
    public SpeechBalloonScript speechBalloonScript_typeD_Test;
    [Header("遊戲選單")]
    public AnimationScript animScript_gameMenu; //遊戲選單動畫腳本
    public sbyte retractRemain; //剩餘悔棋次數
    public const sbyte retractTimes = 3; //預設悔棋次數(預設3次)
    public GameObject retractButton; //悔棋按鈕
    public Text tx_retractRemain; //剩餘悔棋次數Text
    [Header("CanvasGroup")]
    public CanvasGroup cg_mainMenu; //主選單CG
    public CanvasGroup cg_gameMenu; //遊戲選單CG
    public CanvasGroup cg_setting; //設定選單CG

    private bool set_initialize; //初始化設定模式(true=直接設定/false=玩家選擇)
    private Vector2 set_settingMenuPos; //遊戲設定選單位置(還原用)
    [Header("遊戲設定")]
    public Slider set_bgmVolume; //BGM音量Slider
    public Image set_bgmMuteButton; //BGM靜音按鈕Image
    public Slider set_seVolume; //音效音量Slider
    public Image set_seMuteButton; //音效靜音按鈕Image
    public Dropdown set_dpiDropdown; //分辨率選單
    public Image set_fullScreenButton; //全螢幕按鈕Image
    public Slider set_wattingTime; //輪流等待時間Slider
    public Text set_wattingTimeLabel; //輪流等待時間Text
    public Image set_mousePosHintButton; //鼠標位置提示按鈕Image
    public Image set_lastChessPosHintButton; //最後下棋點提示按鈕Image
    public Sprite[] set_muteIcons = new Sprite[2]; //靜音與否圖示
    public Sprite[] set_onOffIcons = new Sprite[2]; //開關圖示

    #endregion
    #region 內建方法 --------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
    }

    void Start()
    {
        set_settingMenuPos = cg_setting.transform.localPosition; //儲存設定選單位置

        playerTypeList.AddRange(System.Enum.GetNames(typeof(PlayerType)));
        aiTypeList.AddRange(System.Enum.GetNames(typeof(AiType)));

        //※未來將用playerPrefs儲存
        senteChess = ChessType.黑子; //先手棋子類型
        tx_blackLabel.text = "先 手";
        tx_blackLabel_hide.text = "後 手";
        tx_whiteLabel.text = "後 手";
        tx_whiteLabel_hide.text = "先 手";

        tx_blackPlayerType.text = playerTypeList[0];
        tx_whitePlayerType.text = playerTypeList[0];

        int[] buttonCount = new int[] { blackSelectionButtons.Length, whiteSelectionButtons.Length };
        Button[][] buttons = new Button[][] { blackSelectionButtons, whiteSelectionButtons };
        bool[] isPlayer = new bool[] { (blackPlayer == PlayerType.玩家), (whitePlayer == PlayerType.玩家) };
        AnimationScript[] animationScripts = new AnimationScript[] { animScript_blackAiButtonFrame, animScript_whiteAiButtonFrame };

        Color32 enableColor = new Color32(255, 255, 255, 255);
        Color32 disableColor = new Color32(100, 100, 100, 255);
        for (int i = 0; i < buttonCount.Length; i++)
        {
            for (int j = 0; j < buttonCount[i]; j++)
            {
                if (isPlayer[i])
                {
                    animationScripts[i].gameObject.GetComponent<Image>().color = disableColor;
                    buttons[i][j].interactable = false;
                }
                else
                {
                    animationScripts[i].gameObject.GetComponent<Image>().color = enableColor;
                    buttons[i][j].interactable = true;
                }
            }
        }

    }

    #endregion
    #region 自訂方法 --------------------------------------------------------------------------------------------------------------

    //(按鈕事件)切換主選單
    public void Btn_ChangeMainMenu(bool leftOrRight)
    {
        int nextIndex = leftOrRight ? ((menuSelectedIndex + 1) % (menuList.Count)) : (menuSelectedIndex - 1 < 0 ? menuList.Count - 1 : menuSelectedIndex - 1);
        float leftPoint = -1285; //左方目標x座標
        float rightPoint = 1285; //右方目標y座標

        tx_switcherTitle.text = menuList[menuSelectedIndex];
        if (menuSelectedIndex == 1) GameSetting_CompletelyImitate(); //移出遊戲設定選單時, 儲存設定值
        tx_switcherTitle_hide.text = menuList[nextIndex];
        childMenuScripts[menuSelectedIndex].AnimationCommand(new PositionScript(changeMenuByCurve, new Vector2(leftOrRight ? leftPoint : rightPoint, 0), false), new AnimationBehavior(PlayMode.狀態延續) { disableObject = true });
        childMenuScripts[nextIndex].AnimationCommand(new PositionScript(changeMenuByCurve, new Vector2(leftOrRight ? rightPoint : leftPoint, 0), new Vector2(0, 0), false), new AnimationBehavior(PlayMode.狀態延續));
        menuSelectedIndex = nextIndex;

        if (nextIndex == 1) cg_setting.blocksRaycasts = true; //移進遊戲設定選單時, 設定按鈕為可按
        else cg_setting.blocksRaycasts = false; //移出遊戲設定選單時, 設定按鈕為不可按

        animScript_mainMenu.PlayAnimation(leftOrRight ? "MainMenuSwitchOver_TurnLeft" : "MainMenuSwitchOver_TurnRight", new AnimationBehavior(PlayMode.狀態延續), false);
    }

    //(按鈕事件)改變先後手
    public void Btn_ChangeSequence(bool leftOrRight)
    {
        bool blackIsSente = (senteChess == ChessType.黑子);
        senteChess = blackIsSente ? ChessType.白子 : ChessType.黑子;
        Dictionary<bool, string> str = new Dictionary<bool, string> { { true, "先手" }, { false, "後手" } };
        tx_blackLabel.text = str[blackIsSente];
        tx_blackLabel_hide.text = str[!blackIsSente];
        tx_whiteLabel.text = str[!blackIsSente];
        tx_whiteLabel_hide.text = str[blackIsSente];

        animScript_mainMenu.PlayAnimation(leftOrRight ? "ChessTypeSwitchOver_TurnLeft" : "ChessTypeSwitchOver_TurnRight", new AnimationBehavior(PlayMode.狀態延續), false);
    }

    //(按鈕事件)改變先手玩家類型
    public void Btn_ChangeSentePlayerType(bool upOrDown)
    {
        _ChangePlayerType(upOrDown, true);
    }

    //(按鈕事件)改變後手玩家類型
    public void Btn_ChangeGotePlayerType(bool upOrDown)
    {
        _ChangePlayerType(upOrDown, false);
    }

    //改變玩家類型(upOrDown true=上,false=下 , senteOrGote true=先手,false=後手)
    //內部執行方法
    private void _ChangePlayerType(bool upOrDown, bool blackOrWhite)
    {
        int index = blackOrWhite ? (int)blackPlayer : (int)whitePlayer;
        int nextIndex = upOrDown ? ((index + 1) % (playerTypeList.Count)) : (index - 1 < 0 ? playerTypeList.Count - 1 : index - 1);

        if (blackOrWhite)
        {
            tx_blackPlayerType.text = playerTypeList[index];
            tx_blackPlayerType_hide.text = playerTypeList[nextIndex];
            blackPlayer = (PlayerType)nextIndex;
        }
        else
        {
            tx_whitePlayerType.text = playerTypeList[index];
            tx_whitePlayerType_hide.text = playerTypeList[nextIndex];
            whitePlayer = (PlayerType)nextIndex;
        }

        int buttonCount = blackOrWhite ? blackSelectionButtons.Length : whiteSelectionButtons.Length;
        Button[] buttons = blackOrWhite ? blackSelectionButtons : whiteSelectionButtons;

        float animTime = 0.18f; //圖示顯亮or顯暗作動時間
        Color32 enableColor = new Color32(255, 255, 255, 255);
        Color32 disableColor = new Color32(100, 100, 100, 255);
        for (int i = 0; i < buttonCount; i++)
        {
            if ((PlayerType)nextIndex == PlayerType.玩家)
            {
                if (blackOrWhite)
                {
                    animScript_blackAiButtonFrame.AnimationCommand(new ColorScript(animTime, disableColor), new AnimationBehavior(PlayMode.狀態延續));
                    tx_blackAiType.text = aiTypeList[(int)blackAi];
                    tx_blackAiType_hide.text = "-";
                    animScript_changeBlackAi.PlayAnimation("SelectionSwitchOver_TurnUp", new AnimationBehavior(PlayMode.狀態延續), false);
                }
                else
                {
                    animScript_whiteAiButtonFrame.AnimationCommand(new ColorScript(animTime, disableColor), new AnimationBehavior(PlayMode.狀態延續));
                    tx_whiteAiType.text = aiTypeList[(int)whiteAi];
                    tx_whiteAiType_hide.text = "-";
                    animScript_changeWhiteAi.PlayAnimation("SelectionSwitchOver_TurnUp", new AnimationBehavior(PlayMode.狀態延續), false);
                }
                buttons[i].interactable = false;
            }
            else
            {
                if (blackOrWhite)
                {
                    animScript_blackAiButtonFrame.AnimationCommand(new ColorScript(animTime, enableColor), new AnimationBehavior(PlayMode.狀態延續));
                    if (tx_blackAiType_hide.text == "-")
                    {
                        tx_blackAiType.text = "-";
                        tx_blackAiType_hide.text = aiTypeList[(int)blackAi];
                        animScript_changeBlackAi.PlayAnimation("SelectionSwitchOver_TurnUp", new AnimationBehavior(PlayMode.狀態延續), false);
                    }
                }
                else
                {
                    animScript_whiteAiButtonFrame.AnimationCommand(new ColorScript(animTime, enableColor), new AnimationBehavior(PlayMode.狀態延續));
                    if (tx_whiteAiType_hide.text == "-")
                    {
                        tx_whiteAiType.text = "-";
                        tx_whiteAiType_hide.text = aiTypeList[(int)whiteAi];
                        animScript_changeWhiteAi.PlayAnimation("SelectionSwitchOver_TurnUp", new AnimationBehavior(PlayMode.狀態延續), false);
                    }
                }
                buttons[i].interactable = true;
            }
        }

        if (blackOrWhite) animScript_changeBlackPlayer.PlayAnimation(upOrDown ? "SelectionSwitchOver_TurnUp" : "SelectionSwitchOver_TurnDown", new AnimationBehavior(PlayMode.狀態延續), false);
        else animScript_changeWhitePlayer.PlayAnimation(upOrDown ? "SelectionSwitchOver_TurnUp" : "SelectionSwitchOver_TurnDown", new AnimationBehavior(PlayMode.狀態延續), false);
    }

    //(按鈕事件)改變先手AI類型
    public void Btn_ChangeSenteAiType(bool upOrDown)
    {
        int index = (int)blackAi;
        int nextIndex = upOrDown ? ((index + 1) % (aiTypeList.Count)) : (index - 1 < 0 ? aiTypeList.Count - 1 : index - 1);

        tx_blackAiType.text = aiTypeList[index];
        tx_blackAiType_hide.text = aiTypeList[nextIndex];
        blackAi = (AiType)nextIndex;

        animScript_changeBlackAi.PlayAnimation(upOrDown ? "SelectionSwitchOver_TurnUp" : "SelectionSwitchOver_TurnDown", new AnimationBehavior(PlayMode.狀態延續), false);
    }

    //(按鈕事件)改變後手AI類型
    public void Btn_ChangeGoteAiType(bool upOrDown)
    {
        int index = (int)whiteAi;
        int nextIndex = upOrDown ? ((index + 1) % (aiTypeList.Count)) : (index - 1 < 0 ? aiTypeList.Count - 1 : index - 1);

        tx_whiteAiType.text = aiTypeList[index];
        tx_whiteAiType_hide.text = aiTypeList[nextIndex];
        whiteAi = (AiType)nextIndex;

        animScript_changeWhiteAi.PlayAnimation(upOrDown ? "SelectionSwitchOver_TurnUp" : "SelectionSwitchOver_TurnDown", new AnimationBehavior(PlayMode.狀態延續), false);
    }

    //(按鈕事件)開始遊戲
    public void Btn_GameStart()
    {
        cg_mainMenu.blocksRaycasts = false; //主選單控件封鎖
        cg_gameMenu.blocksRaycasts = false; //遊戲選單控件封鎖

        UnityAction cancelEvent = () => //選擇"否"的事件
        {
            cg_mainMenu.blocksRaycasts = true;
        };
        UnityAction gameStartPrepose = () => //選擇"是"的事件
        {
            AudioManagerScript.Instance.Stop(0); //停止撥放音樂

            ChessBehavior.Instance.turn = senteChess; //設定先手
            player_black.playerType = blackPlayer; //設定黑子玩家類型
            player_white.playerType = whitePlayer; //設定白子玩家類型
            player_black.SetAI(blackAi); //設定黑子AI等級
            if (player_black.playerType != PlayerType.玩家) ChessBehavior.Instance.ai_black = player_black.aiScript;
            player_white.SetAI(whiteAi); //設定白子AI等級
            if (player_white.playerType != PlayerType.玩家) ChessBehavior.Instance.ai_white = player_white.aiScript;
            retractRemain = retractTimes; //重置悔棋次數
            tx_retractRemain.text = "剩下  " + retractRemain + "  次";
            RetractButtonState(false); //因棋盤上的棋子數<2, 禁用悔棋按鈕
            StartCoroutine(GameFlow.Instance.GameStart());
        };

        speechBalloonScript_typeA.ShowSpeechBalloon(new UnityAction[2] { gameStartPrepose, cancelEvent }, new string[] { "是", "否" }, "是否開始遊戲 ?");
    }

    //(按鈕事件)悔棋
    public void Btn_Retract()
    {
        if (retractRemain > 0)
        {
            if (ChessBehavior.Instance.RetractChess()) //悔棋成功
            {
                retractRemain--;
                tx_retractRemain.text = "剩下  " + retractRemain + "  次";
                RetractButtonState(false); //暫時禁用悔棋按鈕
                if (ChessBehavior.Instance.chessLine.Count > 0)
                {
                    if (ChessBehavior.Instance.lastChessPosHint && !ChessBehavior.Instance.lastChessIcon.activeSelf) ChessBehavior.Instance.lastChessIcon.SetActive(true);
                    ChessBehavior.Instance.lastChessIcon.transform.localPosition = ChessBehavior.Instance.chessLine[ChessBehavior.Instance.chessLine.Count - 1].transform.localPosition + new Vector3(-0.52f, 1.6286f); //改變最後下棋點提示的位置
                }
                else if (ChessBehavior.Instance.lastChessIcon.activeSelf) ChessBehavior.Instance.lastChessIcon.SetActive(false);

            }
            else //悔棋失敗
            {

            }
        }
    }

    //設定悔棋按鈕是否可用
    public void RetractButtonState(bool enable)
    {
        Image img = retractButton.GetComponent<Image>();
        Button button = retractButton.GetComponent<Button>();
        Color32 enableColor = new Color32(255, 255, 255, 255);
        Color32 disableColor = new Color32(100, 100, 100, 255);

        if (enable) img.color = enableColor;
        else img.color = disableColor;

        button.interactable = enable;
    }

    //(按鈕事件)從遊戲返回選單
    public void Btn_BackToMenu()
    {
        cg_gameMenu.blocksRaycasts = false; //遊戲選單控件封鎖
        cg_mainMenu.blocksRaycasts = false; //主選單控件封鎖

        ChessBehavior.Instance.gameStart = false; //遊戲停止
        ChessBehavior.Instance.cursor.SetActive(false); //隱藏滑鼠光標

        UnityAction selectionFadeOut = () =>
        {
            ChessBehavior.Instance.gameStart = true;
            cg_gameMenu.blocksRaycasts = true; //遊戲選單控件封鎖解除
        };

        speechBalloonScript_typeA.ShowSpeechBalloon(new UnityAction[2] { () => { StartCoroutine(GameFlow.Instance.BackToMenu()); }, selectionFadeOut }, new string[] { "是", "否" }, "是否返回主選單 ?");
    }

    //(按鈕事件)退出遊戲
    public void Btn_Exit()
    {
        UnityAction exit = () =>
        {
            GameSetting_CompletelyImitate(); //儲存遊戲設定值
            Application.Quit(); //離開遊戲
        };

        speechBalloonScript_typeC.ShowSpeechBalloon(new UnityAction[] { exit, null }, new string[] { "是", "否" }, "是否離開遊戲 ?");
    }

    //(按鈕事件)呼叫遊戲設定選單
    public void Btn_ShowSettingMenu(bool onOff)
    {
        if (!onOff) GameSetting_CompletelyImitate(); //關閉選單時, 儲存設定值
        ChessBehavior.Instance.gameStart = !onOff; //開啟選單時, 暫停遊戲
        cg_setting.transform.localPosition = onOff ? new Vector2(0, 0) : set_settingMenuPos; //移動選單位置
        cg_setting.blocksRaycasts = onOff;
    }

    //設定先手
    public void SetSenteChess(int type) { senteChess = (ChessType)type; }
    public void SetSenteChess(string type) { senteChess = (ChessType)System.Enum.Parse(typeof(ChessType), type); }

    //(遊戲設定)讀取設定
    public void GameSetting_Load()
    {
        set_initialize = true;

        float _bgmVolume = PlayerPrefs.GetFloat("SET_BgmVolume", 0.8f); //讀取BGM音量
        if (set_bgmVolume.value != _bgmVolume) set_bgmVolume.value = _bgmVolume; //校正BGM音量Slider的值
        if (AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.volume != _bgmVolume) AudiosPack.Instance.SetVolume(0, _bgmVolume); //調整BGM音量

        bool _bgmIsMute = PlayerPrefs.GetInt("SET_BgmIsMute", 0) == 0 ? false : true; //讀取BGM靜音狀態
        if (AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.mute != _bgmIsMute) AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.mute = _bgmIsMute; //調整BGM靜音狀態
        GameSetting_BgmIsMute(false); //校正BGM靜音按鈕圖示

        float _seVolume = PlayerPrefs.GetFloat("SET_SeVolume", 0.8f); //讀取音效音量
        if (set_seVolume.value != _seVolume) set_seVolume.value = _seVolume; //校正音效音量Slider的值
        if (AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.volume != _seVolume) AudiosPack.Instance.SetVolume(1, _bgmVolume); //調整音效音量

        bool _seIsMute = PlayerPrefs.GetInt("SET_SeIsMute", 0) == 0 ? false : true; //取得音效靜音狀態
        if (AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.mute != _seIsMute) AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.mute = _seIsMute; //調整音效靜音狀態
        GameSetting_SeIsMute(false); //校正音效靜音按鈕圖示

        int _dpi = PlayerPrefs.GetInt("SET_DPI", 3); //取得DPI設定
        set_dpiDropdown.value = _dpi; //調整分辨率

        bool _fullScreen = PlayerPrefs.GetInt("SET_FullScreen", 0) == 0 ? false : true; //取得全螢幕狀態
        if (Screen.fullScreen != _fullScreen) Screen.fullScreen = _fullScreen; //調整全螢幕狀態
        set_fullScreenButton.sprite = _fullScreen ? set_onOffIcons[1] : set_onOffIcons[0]; //校正全螢幕按鈕圖示

        float _turnNextDelay = PlayerPrefs.GetFloat("SET_TurnNextDelay", 0.8f); //取得輪流等待時間
        if (ChessBehavior.Instance.turnNextDelay != _turnNextDelay) ChessBehavior.Instance.turnNextDelay = _turnNextDelay; //調整輪流等待時間
        if (set_wattingTime.value != _turnNextDelay || set_wattingTimeLabel.text != _turnNextDelay.ToString("0.0")) //校正輪流等待時間的Slider和Text
        {
            set_wattingTime.value = _turnNextDelay;
            set_wattingTimeLabel.text = _turnNextDelay.ToString("0.0");
        }

        bool _mousePosHint = PlayerPrefs.GetInt("SET_MousePosHint", 1) == 1 ? true : false; //取得滑鼠位置提示的開關狀態
        if (ChessBehavior.Instance.mousePosHint != _mousePosHint) ChessBehavior.Instance.mousePosHint = _mousePosHint; //調整鼠標位置提示開關
        set_mousePosHintButton.sprite = _mousePosHint ? set_onOffIcons[1] : set_onOffIcons[0]; //校正鼠標位置提示按鈕的圖示

        bool _lastChessPosHint = PlayerPrefs.GetInt("SET_LastChessPosHint", 1) == 1 ? true : false; //取得最後下棋點提示的開關狀態
        if (ChessBehavior.Instance.lastChessPosHint != _lastChessPosHint) ChessBehavior.Instance.lastChessPosHint = _lastChessPosHint; //調整最後下棋點提示開關
        set_lastChessPosHintButton.sprite = _lastChessPosHint ? set_onOffIcons[1] : set_onOffIcons[0]; //校正最後下棋點提示按鈕的圖示

        set_initialize = false;
    }

    //(遊戲設定)儲存設定
    public void GameSetting_CompletelyImitate()
    {
        PlayerPrefs.SetFloat("SET_BgmVolume", AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.volume); //BGM音量
        PlayerPrefs.SetInt("SET_BgmIsMute", AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.mute ? 1 : 0); //BGM靜音狀態
        PlayerPrefs.SetFloat("SET_SeVolume", AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.volume); //音效音量
        PlayerPrefs.SetInt("SET_SeIsMute", AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.mute ? 1 : 0); //音效靜音狀態
        PlayerPrefs.SetInt("SET_FullScreen", Screen.fullScreen ? 1 : 0); //全螢幕狀態
        PlayerPrefs.SetFloat("SET_TurnNextDelay", ChessBehavior.Instance.turnNextDelay); //輪流等待時間
        PlayerPrefs.SetInt("SET_MousePosHint", ChessBehavior.Instance.mousePosHint ? 1 : 0); //滑鼠位置提示
        PlayerPrefs.SetInt("SET_LastChessPosHint", ChessBehavior.Instance.lastChessPosHint ? 1 : 0); //最後下棋點提示
    }

    //(遊戲設定)設定BGM音量
    public void GameSetting_SetBgmVolume(float v)
    {
        AudiosPack.Instance.SetVolume(0, v);
    }

    //(遊戲設定)背景音樂是否靜音
    public void GameSetting_BgmIsMute(bool type) //true=改變狀態;false=校正狀態
    {
        bool isMute = AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.mute;
        if (!type) isMute = !isMute;
        set_bgmVolume.interactable = isMute; //靜音時, 封鎖音量Slider
        set_bgmVolume.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = isMute ? new Color32(118, 249, 118, 255) : new Color32(82, 104, 82, 255); //靜音時, 改變音量Slider的顏色
        if (type) AudiosPack.Instance.GetAudioSourceUnit("AS_BGM").source.mute = !isMute; //切換靜音狀態
        set_bgmMuteButton.sprite = set_muteIcons[isMute ? 0 : 1]; //切換圖示
    }

    //(遊戲設定)設定SE音量
    public void GameSetting_SetSeVolume(float v)
    {
        AudiosPack.Instance.SetVolume(1, v);
    }

    //(遊戲設定)音效是否靜音
    public void GameSetting_SeIsMute(bool type) //true=改變狀態;false=校正狀態
    {
        bool isMute = AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.mute;
        if (!type) isMute = !isMute;
        set_seVolume.interactable = isMute; //靜音時, 封鎖音量Slider
        set_seVolume.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = isMute ? new Color32(118, 249, 118, 255) : new Color32(82, 104, 82, 255); //靜音時, 改變音量Slider的顏色
        if (type) AudiosPack.Instance.GetAudioSourceUnit("AS_SE").source.mute = !isMute; //切換靜音狀態
        set_seMuteButton.sprite = set_muteIcons[isMute ? 0 : 1]; //切換圖示
    }

    //(遊戲設定)分辨率設定
    public void GameSetting_DPIChange(int index)
    {
        UnityAction setValue = () =>  //套用至新值
        {
            switch (index)
            {
                case 0: //1024 × 768
                    Screen.SetResolution(1024, 768, Screen.fullScreen);
                    break;

                case 1: //1280 × 720
                    Screen.SetResolution(1280, 720, Screen.fullScreen);
                    break;

                case 2: //1280 × 1024
                    Screen.SetResolution(1280, 1024, Screen.fullScreen);
                    break;

                case 3: //1440 × 900
                    Screen.SetResolution(1440, 900, Screen.fullScreen);
                    break;

                case 4: //1600 × 900
                    Screen.SetResolution(1600, 900, Screen.fullScreen);
                    break;

                case 5: //1920 × 1080
                    Screen.SetResolution(1920, 1080, Screen.fullScreen);
                    break;
            }

            PlayerPrefs.SetInt("SET_DPI", index);
        };
        UnityAction returnValue = () => //返回至原值
        {
            set_dpiDropdown.value = PlayerPrefs.GetInt("SET_DPI", 3); //預設為index=3(1440 × 900)
        };

        if (!set_initialize) speechBalloonScript_typeC.ShowSpeechBalloon(new UnityAction[] { setValue, returnValue }, new string[] { "是", "否" }, "是否改變分辨率 ?");
        else setValue.Invoke();
    }

    //(遊戲設定)全螢幕設定
    public void GameSetting_IsFullScreen()
    {
        bool isFullScreen = Screen.fullScreen;

        UnityAction change = () => //改變全螢幕狀態
        {
            set_fullScreenButton.sprite = isFullScreen ? set_onOffIcons[0] : set_onOffIcons[1]; //改變開關圖示
            Screen.fullScreen = !isFullScreen; //改變全螢幕狀態
        };

        speechBalloonScript_typeC.ShowSpeechBalloon(new UnityAction[] { change, null }, new string[] { "是", "否" }, isFullScreen ? "是否使用視窗模式 ?" : "是否使用全螢幕模式 ?");
    }

    //(遊戲設定)輪流等待時間
    public void GameSetting_TurnNextDelay(float v)
    {
        ChessBehavior.Instance.turnNextDelay = v;
        set_wattingTimeLabel.text = v.ToString("0.0");
    }

    //(遊戲設定)鼠標位置提示
    public void GameSetting_MousePosHint()
    {
        bool mousePosHint = ChessBehavior.Instance.mousePosHint;

        set_mousePosHintButton.sprite = mousePosHint ? set_onOffIcons[0] : set_onOffIcons[1]; //改變開關圖示
        ChessBehavior.Instance.mousePosHint = !mousePosHint; //開啟或關閉鼠標位置提示
    }

    //(遊戲設定)最後下棋點提示
    public void GameSetting_LastChessPosHint()
    {
        bool lastChessPosHint = ChessBehavior.Instance.lastChessPosHint;

        set_lastChessPosHintButton.sprite = lastChessPosHint ? set_onOffIcons[0] : set_onOffIcons[1]; //改變開關圖示
        ChessBehavior.Instance.lastChessPosHint = !lastChessPosHint; //開啟或關閉最後下棋點提示
    }


    #endregion
    #region 協同程序 --------------------------------------------------------------------------------------------------------------

    #endregion
}