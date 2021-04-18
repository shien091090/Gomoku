//提供選項對話框功能的腳本

//※適用於所有專案的功能性獨立腳本※
//※搭配Editor腳本"SpeechBalloonScriptEditor"※

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#region 額外Class ---------------------------------------------------------------------------------------------------------------

public class ClipInfo //儲存音效剪輯和音量的類
{
    public AudioClip GetClip { get; private set; } //音效剪輯
    public float GetVolume { get; private set; } //音量

    public ClipInfo(AudioClip c, float v)
    {
        GetClip = c;
        GetVolume = v;
    }
}

#endregion

public class SpeechBalloonScript : MonoBehaviour
{
    #region 變數宣告 ---------------------------------------------------------------------------------------------------------------
    static public bool s_speechBalloonShowing; //(靜態變數)對話框顯示中
    public bool[] foldStates; //摺疊夾狀態
    private float timer; //計時器
    private bool isTimerStart; //是否計時
    private bool[] fadeInOver = new bool[2] { false, false }; //淡入程序完成(布幕淡入&選項淡入)
    private bool[] fadeOutOver = new bool[4] { false, false, false, false }; //淡入程序完成(布幕淡出&選項淡出&對話框淡出&選項事件發生)
    private int selected; //玩家是否按下選擇
    private List<GameObject> selectionPack = new List<GameObject>(); //目前選項集合
    private UnityAction[] selectionEvents = new UnityAction[] { }; //目前選項事件集合
    private delegate float GetSeletionPos_X(RectTransform balloon, RectTransform selection, int length, int index, float padding); //(演算法)選項產生時的位置座標X
    private delegate float GetSeletionPos_Y(RectTransform balloon, RectTransform selection, float padding); //(演算法)選項產生時的位置座標X

    public enum Enum_mainType { Dialogue, SystemMessage } //對話框類型
    public Enum_mainType sel_mainType = Enum_mainType.Dialogue;

    //對話框執行流程 :
    //0.前置事件
    public enum Enum_preEvent { On, Off }
    public Enum_preEvent sel_preEvent = Enum_preEvent.Off;
    public UnityEvent preEvent = new UnityEvent();

    //1.控件封鎖
    public enum Enum_blockade { CanvasGroup_blocksRaycasts, Image_raycastTarget, Off }
    public Enum_blockade sel_blockade = Enum_blockade.Off;
    public CanvasGroup blockade_cg; //CanvasGroup場合
    public List<Image> blockade_img = new List<Image>(); //Image[]場合

    //2.布幕淡入
    public enum Enum_curtainFadeIn { CanvasGroup_alpha, Image_Color, Off }
    public Enum_curtainFadeIn sel_curtainFadeIn = Enum_curtainFadeIn.Off;
    public CanvasGroup curtain_cg; //布幕透過CanvasGroup控制Alpha的場合
    public Vector2 curtainFadeIn_tranStates = new Vector2(); //CanvasGroup.Alpha的起始與結束值
    public Vector2 curtainFadeIn_trasDuration; //動畫作動期間
    public Image curtain_img; //布幕透過Image控制Color的場合
    public Color[] curtainFadeIn_transColors = new Color[2]; //動畫作動期間的起始與結束顏色

    //3.對話框淡入
    public enum Enum_balloonFadeIn { On, Off }
    public Enum_balloonFadeIn sel_balloonFadeIn = Enum_balloonFadeIn.Off;
    public Image balloon_img; //對話框Image
    public Color[] balloon_transColors = new Color[2]; //動畫作動期間的起始與結束顏色
    public Vector2 balloon_trasDuration; //動畫作動期間

    //3.5 等待視窗選項
    public bool isWaiting = false; //是否在等待中
    public bool isTxtCircle = false; //是否啟用循環讀字
    //public bool stopWaitingBybtn = false; //是否由按下按鈕來中止等待視窗(而非透過呼叫訊息視窗或是切換isWaiting來中止)
    public enum Enum_waitingType { 圖片旋轉, 進度條循環, 激活物件, Off }
    public Enum_waitingType sel_waitingType = Enum_waitingType.Off;
    public float wait_circleSpeed; //圖片旋轉或進度條循環的速度
    public float wait_textCircleSpeed; //循環讀字速度
    public Image wait_rotateImg; //旋轉圖片(Image)
    public Slider wait_circulateSlider; //循環進度條(Slider)
    public GameObject wait_activeGameobject; //激活之物件(GameObject)

    //4.對話框文字顯示
    public enum Enum_textView { 張貼, 讀字 }
    public enum Enum_continueSpeechKey { 任意, 指定 }
    public Enum_textView sel_textView = Enum_textView.張貼;
    public Enum_continueSpeechKey sel_continueSpeechKey = Enum_continueSpeechKey.任意;
    public KeyCode sel_key;
    public Text textView_text; //對話框Text
    public string temp_text = null; //對話文字暫存
    public GameObject textView_cursor; //對話進行閃爍標
    public float textView_readSpeed; //讀字速度

    //5.選項淡入動畫
    public enum Enum_selectionFadeInAnimation { 淡入, 橫向擴散, 浮現, 掉落, 無動畫 }
    public Enum_selectionFadeInAnimation sel_selectionFadeInAnimation = Enum_selectionFadeInAnimation.無動畫;
    public enum Enum_waitingButtonActiveMode { 隱藏, 顯示但未激活, 顯示且激活 }
    public Enum_waitingButtonActiveMode sel_waitingButtonActiveMode = Enum_waitingButtonActiveMode.隱藏;
    public GameObject seletion_prefab; //選項預置體
    public Transform selection_holder; //選項父區域
    public Vector2 selection_padding; //選項間隔
    public float selectionFadeIn_costTime; //選項淡入花費時間

    //6.選擇選項後
    public float afterSelect_delay; //選項選擇後事件發生延遲時間

    //7.選項淡出動畫
    public enum Enum_selectionFadeOutAnimation { 變色後延遲淡出, 向中央移動後淡出, 放大後淡出, 無動畫 }
    public Enum_selectionFadeOutAnimation sel_selectionFadeOutAnimation = Enum_selectionFadeOutAnimation.無動畫;

    //8.對話框淡出
    public Color[] balloonFadeOut_transColors = new Color[2]; //動畫作動期間的起始與結束顏色
    public Vector2 balloonFadeOut_trasDuration; //動畫作動期間

    //9.布幕淡出
    public Vector2 curtainFadeOut_tranStates = new Vector2(); //CanvasGroup.Alpha的起始與結束值
    public Vector2 curtainFadeOut_trasDuration; //動畫作動期間
    public Color[] curtainFadeOut_transColors = new Color[2]; //動畫作動期間的起始與結束顏色

    //10.控件封鎖解除

    //11.後置事件
    public enum Enum_endingEvent { On, Off }
    public Enum_endingEvent sel_endingEvent = Enum_endingEvent.Off;
    public UnityEvent endingEvent = new UnityEvent();

    //Sound Effect
    public AudioListener audioListener; //聲音接收器
    public enum Enum_soundTiming { 呼叫對話框時, 布幕淡入時, 對話框淡入時, 選項淡入時, 每次讀字時, 閃爍標鍵入時, 選擇任意選項時, 選擇選項1時, 選擇選項2時, 選擇選項3時, 選擇選項4時, 選項事件發生時, 選項淡出時, 對話框淡出時, 布幕淡出時, 結束時, 對話框呼叫失敗時, 呼叫等待視窗時 }
    public bool se_UseCustomAudioSource; //是否自訂音源
    public AudioSource se_AudioSoucre; //音源參考
    public List<Enum_soundTiming> se_SoundTimings = new List<Enum_soundTiming>(); //音效發生點
    public List<AudioClip> se_clips = new List<AudioClip>(); //音效剪輯
    public List<float> se_volume = new List<float>(); //各音效的音量
    private Dictionary<string, List<ClipInfo>> se_preparingClipsInfo = new Dictionary<string, List<ClipInfo>>(); //在某時機對應撥放所有剪輯

    #endregion
    #region 列舉值(enum) ---------------------------------------------------------------------------------------------------------------
    #endregion

    #region 內建方法 ---------------------------------------------------------------------------------------------------------------

    void Start()
    {
        List<int> allotList = new List<int>();
        for (int i = 0; i < se_SoundTimings.Count; i++) //儲存所有索引, 照撥放時機分配至se_playClipsPack
        {
            allotList.Add(i);
        }

        for (int i = 0; i < System.Enum.GetValues(typeof(Enum_soundTiming)).Length; i++) //檢查所有時間點
        {
            List<ClipInfo> clipInfo = new List<ClipInfo>();

            List<int> picked = new List<int>();
            for (int j = 0; j < allotList.Count; j++) //檢查所有時間點音效
            {
                if (System.Enum.GetName(typeof(Enum_soundTiming), se_SoundTimings[allotList[j]]) == System.Enum.GetNames(typeof(Enum_soundTiming))[i]) //若設定的時機點符合
                {
                    ClipInfo info = new ClipInfo(se_clips[allotList[j]], se_volume[allotList[j]]); //彙整剪輯與該剪輯的音量資訊
                    clipInfo.Add(info);
                    picked.Add(allotList[j]); //列入預備挑掉的索引值
                }
            }

            if (clipInfo.Count > 0) //若檢查該時間點有設定音效
            {
                se_preparingClipsInfo.Add(System.Enum.GetNames(typeof(Enum_soundTiming))[i], clipInfo); //插入音效剪輯陣列至該時間點
                for (int j = 0; j < picked.Count; j++)
                {
                    allotList.Remove(picked[j]);
                }
            }
        }
    }

    #endregion
    #region 自訂方法 ---------------------------------------------------------------------------------------------------------------

    //呼叫對話框(1.actions:選項對應的執行方法 / 2.selections:選項文字  / 3.content:對話框文字)
    public void ShowSpeechBalloon(UnityAction[] actions, string[] selections, params string[] content)
    {
        if (!s_speechBalloonShowing)
        {
            s_speechBalloonShowing = true; //避免再次呼叫對話框

            Initialize(); //初始化參數
            selectionEvents = actions; //儲存選項事件

            PlayArrangeSound(Enum_soundTiming.呼叫對話框時); //撥放音效
            StartCoroutine(ClockTimer()); //開始計時器
            StartCoroutine(MainProcess(actions, selections, content)); //對話框實作協程
        }
        else PlayArrangeSound(Enum_soundTiming.對話框呼叫失敗時); //撥放音效
    }

    //呼叫等待視窗(1.action:選項的執行方法 / 2.selection:選項文字 / 3.content:對話框文字 / 4.循環文字)
    public void ShowWaitingMessage(UnityAction action, string selection, string content, string circleTxt)
    {
        if (sel_mainType != Enum_mainType.SystemMessage)
        {
            Debug.Log("[ERROR]必須設定SystemMessage才能呼叫等待視窗");
            return;
        }

        if (!s_speechBalloonShowing)
        {
            s_speechBalloonShowing = true; //避免再次呼叫對話框
            isWaiting = true; //開啟等待狀態

            Initialize(); //初始化參數
            selectionEvents = new UnityAction[] { action }; //儲存選項事件

            PlayArrangeSound(Enum_soundTiming.呼叫等待視窗時); //撥放音效
            StartCoroutine(ClockTimer()); //開始計時器
            StartCoroutine(MainProcess(new UnityAction[] { action }, new string[] { selection }, new string[] { content, circleTxt })); //對話框實作協程
        }
    }

    //呼叫訊息視窗(1.action:選項的執行方法 / 2.selection:選項文字 / 3.content:對話框文字) ※更新等待視窗的情況下, 參數若為null則設定為不取代原等待視窗的文字
    //可用於 1.結束等待視窗,並更新成新的按鈕或對話框文字 2.直接呼叫一個訊息視窗
    public void SetSystemMessage(UnityAction action, string selection, string content)
    {
        if (sel_mainType != Enum_mainType.SystemMessage)
        {
            Debug.Log("[ERROR]必須設定SystemMessage才能呼叫等待視窗");
            return;
        }

        if (!s_speechBalloonShowing)
        {
            s_speechBalloonShowing = true; //避免再次呼叫對話框

            Initialize(); //初始化參數
            selectionEvents = new UnityAction[] { action }; //儲存選項事件

            PlayArrangeSound(Enum_soundTiming.呼叫等待視窗時); //撥放音效
            StartCoroutine(ClockTimer()); //開始計時器
            StartCoroutine(MainProcess(new UnityAction[] { action }, new string[] { selection }, content)); //對話框實作協程
        }
        else if (s_speechBalloonShowing && isWaiting)
        {
            if (action != null) selectionEvents = new UnityAction[] { action }; //更新選項事件
            if (selection != null) //更新選項文字
            {
                Text text = null; //取得物件上的Text
                if (selectionPack[0].GetComponent<Text>() == null && selectionPack[0].GetComponentInChildren<Text>() == null) text = selectionPack[0].AddComponent<Text>();
                else if (selectionPack[0].GetComponentInChildren<Text>() != null) text = selectionPack[0].GetComponentInChildren<Text>();
                else if (selectionPack[0].GetComponent<Text>() != null && selectionPack[0].GetComponentInChildren<Text>() == null) text = selectionPack[0].GetComponent<Text>();
                text.text = selection; //設定選項字串
            }
            if (content != null) temp_text = content; //更新對話框文字

            isWaiting = false;
        }
    }

    //初始化參數
    private void Initialize()
    {
        //Debug.Log("參數初始化 [" + timer + "]");
        for (int i = 0; i < selectionPack.Count; i++) //銷毀選項物件
        {
            Destroy(selectionPack[i].gameObject);
        }

        selected = -1; //初始化選擇項目
        fadeInOver = new bool[2] { false, false }; //重置偵測淡入是否完成之開關
        fadeOutOver = new bool[4] { false, false, false, false }; //重置偵測淡出是否完成之開關
        selectionPack = new List<GameObject>(); //初始化選項物件
        selectionEvents = new UnityAction[] { }; //初始化選項事件
        temp_text = null; //清除暫存對話框文字
        //stopWaitingBybtn = false;
    }

    //模組化方法
    //控件封鎖
    private void PartMethod_Blockade(Enum_blockade sel, bool state)
    {
        switch (sel)
        {
            case Enum_blockade.CanvasGroup_blocksRaycasts:
                if (blockade_cg != null) blockade_cg.blocksRaycasts = state;
                break;

            case Enum_blockade.Image_raycastTarget:
                if (blockade_img != null && blockade_img.Count > 0)
                {
                    for (int i = 0; i < blockade_img.Count; i++)
                    {
                        blockade_img[i].raycastTarget = state;
                    }
                }
                break;
        }
    }

    //選項選擇後執行方法
    private void SelectFunction()
    {
        int index = 0;
        for (int i = 0; i < selectionPack.Count; i++) //取得所選擇選項索引
        {
            if (EventSystem.current.currentSelectedGameObject.gameObject == selectionPack[i])
            {
                index = i;
                break;
            }
        }
        selected = index; //指定選擇項目索引

        PlayArrangeSound(Enum_soundTiming.選擇任意選項時); //撥放音效
        switch (index)
        {
            case 0:
                PlayArrangeSound(Enum_soundTiming.選擇選項1時); //撥放音效
                break;

            case 1:
                PlayArrangeSound(Enum_soundTiming.選擇選項2時); //撥放音效
                break;

            case 2:
                PlayArrangeSound(Enum_soundTiming.選擇選項3時); //撥放音效
                break;

            case 3:
                PlayArrangeSound(Enum_soundTiming.選擇選項4時); //撥放音效
                break;
        }
    }

    //撥放預備撥放的音效
    private void PlayArrangeSound(Enum_soundTiming timing)
    {
        if (se_preparingClipsInfo.ContainsKey(System.Enum.GetName(typeof(Enum_soundTiming), timing))) //安全性校驗(是否包含key)
        {
            for (int i = 0; i < se_preparingClipsInfo[System.Enum.GetName(typeof(Enum_soundTiming), timing)].Count; i++)
            {
                ClipInfo info = se_preparingClipsInfo[System.Enum.GetName(typeof(Enum_soundTiming), timing)][i];
                if (info.GetClip != null)
                {
                    //不指定AudioSouce時
                    if (se_AudioSoucre == null) AudioSource.PlayClipAtPoint(info.GetClip, audioListener.transform.position, info.GetVolume); //撥放音效
                    //自訂AudioSource時
                    else se_AudioSoucre.PlayOneShot(info.GetClip, info.GetVolume); //撥放音校(此時的info.GetVolume為Volume Scale)

                }
            }
        }
    }

    #endregion
    #region 協同程序 ---------------------------------------------------------------------------------------------------------------

    //計時器
    private IEnumerator ClockTimer()
    {
        timer = 0;
        //Debug.Log("計時器開始 [" + timer + "]");

        isTimerStart = true;
        while (isTimerStart)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("計時器結束 [" + timer + "]");
        timer = 0;
    }

    //對話框執行協程
    private IEnumerator MainProcess(UnityAction[] actions, string[] selections, params string[] content)
    {
        //前置事件
        if (sel_preEvent == Enum_preEvent.On)
        {
            //Debug.Log("前置事件 [" + timer + "]");
            preEvent.Invoke();
        }
        //else Debug.Log("無前置事件 [" + timer + "]");

        if (sel_mainType == Enum_mainType.SystemMessage) StartCoroutine(Cor_PartMethod_SelectionFadeIn(sel_selectionFadeInAnimation, actions, selections)); //訊息視窗模式時, 在呼叫對話框的初期就會產生選項

        //控件封鎖
        if (sel_blockade != Enum_blockade.Off)
        {
            //Debug.Log("控件封鎖 [" + timer + "]");
            PartMethod_Blockade(sel_blockade, false);
        }
        //else Debug.Log("無控件封鎖 [" + timer + "]");

        //布幕淡入
        if (sel_curtainFadeIn != Enum_curtainFadeIn.Off) StartCoroutine(Cor_PartMethod_curtainFadeIn(sel_curtainFadeIn));
        else
        {
            //Debug.Log("無布幕淡入 [" + timer + "]");
            fadeInOver[0] = true;
        }

        //對話框淡入
        yield return StartCoroutine(Cor_PartMethod_SpeechBalloonFadeIn(sel_balloonFadeIn, sel_textView, content));

        if (sel_mainType == Enum_mainType.SystemMessage) //若為訊息視窗
        {
            string circleTxt = content.Length > 1 ? content[1] : ""; //取得content的第二項(循環文字)
            if (isWaiting) yield return StartCoroutine(Cor_PartMethod_MessageWindowWaiting(circleTxt)); //等待特效
            else
            {
                //選項可正常操作
                selectionPack[0].gameObject.SetActive(true);
                selectionPack[0].GetComponent<CanvasGroup>().alpha = 1;
                selectionPack[0].GetComponent<CanvasGroup>().blocksRaycasts = true;
                selectionPack[0].GetComponent<CanvasGroup>().interactable = true;
                fadeInOver[1] = true;
            }

            if (temp_text != null) textView_text.text = temp_text; //更新對話框文字
        }
        else yield return StartCoroutine(Cor_PartMethod_SelectionFadeIn(sel_selectionFadeInAnimation, actions, selections)); //一般對話框模式時的選項淡入特效

        //淡入程序完成
        yield return StartCoroutine(Cor_PartMethod_SetInteractable());

        //等待玩家選擇
        yield return new WaitUntil(() => selected != -1);

        //選項事件
        StartCoroutine(Cor_PartMethod_SelectionEvent(selected, sel_selectionFadeOutAnimation));

        //布幕淡出
        if (sel_curtainFadeIn != Enum_curtainFadeIn.Off) StartCoroutine(Cor_PartMethod_CurtainFadeOut(sel_curtainFadeIn));
        else
        {
            //Debug.Log("無布幕淡出 [" + timer + "]");
            fadeOutOver[0] = true;
        }

        //對話框淡出
        StartCoroutine(Cor_PartMethod_SpeechBalloonFadeOut(sel_balloonFadeIn));

        //等待布幕淡出&對話框淡出&選項淡出&選項事件發生
        yield return new WaitUntil(() => (fadeOutOver[0] && fadeOutOver[1] && fadeOutOver[2] && fadeOutOver[3]));

        //後置事件
        if (sel_preEvent == Enum_preEvent.On)
        {
            //Debug.Log("後置事件 [" + timer + "]");
            endingEvent.Invoke();
        }
        //else Debug.Log("無後置事件 [" + timer + "]");

        //控件封鎖解除
        if (sel_blockade != Enum_blockade.Off)
        {
            //Debug.Log("控件封鎖解除 [" + timer + "]");
            PartMethod_Blockade(sel_blockade, true);
        }
        //else Debug.Log("無控件封鎖解除 [" + timer + "]");

        Initialize(); //初始化參數
        isTimerStart = false; //計時器結束

        yield return new WaitForEndOfFrame(); //暫停1幀(安全性等待)

        PlayArrangeSound(Enum_soundTiming.結束時); //撥放音效
        //Debug.Log("[對話框結束, 允許再次呼叫對話框]");
        s_speechBalloonShowing = false; //允許再次呼叫對話框
    }

    //模組化方法
    //布幕淡入協程
    private IEnumerator Cor_PartMethod_curtainFadeIn(Enum_curtainFadeIn sel)
    {
        yield return new WaitUntil(() => timer >= curtainFadeIn_trasDuration.x); //等待布幕淡入
        //Debug.Log("布幕淡入開始 [" + timer + "]");

        PlayArrangeSound(Enum_soundTiming.布幕淡入時); //撥放音效
        if (curtain_cg != null && !curtain_cg.gameObject.activeSelf) curtain_cg.gameObject.SetActive(true);
        if (curtain_img != null && !curtain_img.gameObject.activeSelf) curtain_img.gameObject.SetActive(true);

        if (sel_mainType == Enum_mainType.SystemMessage) //訊息視窗模式>>省略特效, 直接使布幕淡入完成
        {
            switch (sel) //顯示布幕
            {
                case Enum_curtainFadeIn.CanvasGroup_alpha:
                    curtain_cg.alpha = curtainFadeIn_tranStates.y;
                    break;

                case Enum_curtainFadeIn.Image_Color:
                    curtain_img.color = curtainFadeIn_transColors[1];
                    break;
            }

            fadeInOver[0] = true;
            yield break;
        }

        //設定計算用參數
        float deltaTimer = curtainFadeIn_trasDuration.y - curtainFadeIn_trasDuration.x;
        float deltaState = curtainFadeIn_tranStates.y - curtainFadeIn_tranStates.x;
        float nowAlpha = 0;
        Color deltaColor = curtainFadeIn_transColors[1] - curtainFadeIn_transColors[0];
        Color nowColor;

        bool startFadeIn = true;
        while (startFadeIn) //實作淡入特效
        {
            if (timer >= curtainFadeIn_trasDuration.y) //結束淡入特效
            {
                startFadeIn = false;
            }

            switch (sel)
            {
                case Enum_curtainFadeIn.CanvasGroup_alpha:
                    nowAlpha = (deltaState / deltaTimer) * (timer - curtainFadeIn_trasDuration.x) + curtainFadeIn_tranStates.x; // nowAlpha = Δt/Δs * (now - t1) + s1
                    curtain_cg.alpha = startFadeIn ? nowAlpha : curtainFadeIn_tranStates.y;
                    break;

                case Enum_curtainFadeIn.Image_Color:
                    nowColor = (deltaColor / deltaTimer) * (timer - curtainFadeIn_trasDuration.x) + curtainFadeIn_transColors[0];
                    curtain_img.color = startFadeIn ? nowColor : curtainFadeIn_transColors[1];
                    break;
            }

            yield return null;
        }

        //Debug.Log("布幕淡入結束 [" + timer + "]");
        fadeInOver[0] = true;
    }

    //模組化方法
    //對話框淡入協程
    private IEnumerator Cor_PartMethod_SpeechBalloonFadeIn(Enum_balloonFadeIn sel, Enum_textView textView, params string[] content)
    {
        textView_text.text = null; //清空對話框文字
        Color originTextColor = textView_text.color; //儲存文字原本的顏色
        Color limpidTextColor = new Color(textView_text.color.r, textView_text.color.g, textView_text.color.b, 0); //文字透明色

        if (sel == Enum_balloonFadeIn.On) yield return new WaitUntil(() => timer >= balloon_trasDuration.x); //等待對話框淡入
        if (sel == Enum_balloonFadeIn.Off) yield return new WaitUntil(() => fadeInOver[0]); //若無對話框, 則等待布幕淡入完成
        //Debug.Log("對話框淡入開始 [" + timer + "]");

        PlayArrangeSound(Enum_soundTiming.對話框淡入時); //撥放音效
        if (!textView_text.gameObject.activeSelf) textView_text.gameObject.SetActive(true); //若文字顯示區域(textView_text)不是active狀態, 啟動之
        if (!textView_text.transform.parent.gameObject.activeSelf) textView_text.transform.parent.gameObject.SetActive(true); //若父物件不是active狀態, 啟動之

        if (textView == Enum_textView.張貼) //張貼的場合, 文字與對話框一起淡入
        {
            string contentStr = null;

            if (sel_mainType == Enum_mainType.SystemMessage) contentStr = content[0]; //訊息視窗模式, 只允許顯示一句文字
            else
            {
                for (int i = 0; i < content.Length; i++)
                {
                    contentStr += content[i];
                }
            }

            textView_text.text = contentStr;
            if (sel == Enum_balloonFadeIn.On) textView_text.color = limpidTextColor; //暫時設為透明, 透明度跟隨對話框一起變化
        }

        if (sel == Enum_balloonFadeIn.On)
        {
            if (!balloon_img.gameObject.activeSelf) balloon_img.gameObject.SetActive(true); //若對話框Image不是active狀態, 啟動之
            if (textView_cursor != null && textView_cursor.activeSelf) textView_cursor.SetActive(false); //若文字閃爍標(textView_cursor)為active狀態, 關閉之

            if (sel_mainType == Enum_mainType.SystemMessage) //訊息視窗模式, 省略動畫直接完成對話框淡入
            {
                balloon_img.color = balloon_transColors[1]; //完成對話框Image狀態
                textView_text.color = originTextColor; //完成對話文字的顏色狀態
            }
            else
            {
                //設定計算用參數
                float deltaTimer = balloon_trasDuration.y - balloon_trasDuration.x;
                Color deltaColor = balloon_transColors[1] - balloon_transColors[0];
                Color deltaTextColor = originTextColor - limpidTextColor;

                bool startFadeIn = true;
                while (startFadeIn)
                {
                    if (timer >= balloon_trasDuration.y)
                    {
                        startFadeIn = false;
                    }

                    Color nowColor = (deltaColor / deltaTimer) * (timer - balloon_trasDuration.x) + balloon_transColors[0];
                    balloon_img.color = startFadeIn ? nowColor : balloon_transColors[1];
                    if (textView == Enum_textView.張貼)
                    {
                        Color nowTextColor = (deltaTextColor / deltaTimer) * (timer - balloon_trasDuration.x) + limpidTextColor;
                        textView_text.color = startFadeIn ? nowTextColor : originTextColor;
                    }

                    yield return null;
                }
            }

            //yield return new WaitForSeconds(0.5f);
        }

        if (textView == Enum_textView.讀字)
        {
            for (int i = 0; i < content.Length; i++) //第N句
            {
                textView_text.text = null; //清空對話框文字

                for (int j = 0; j < content[i].Length; j++) //逐字讀出第N句的每一個字
                {
                    textView_text.text += content[i].Substring(j, 1);
                    PlayArrangeSound(Enum_soundTiming.每次讀字時); //撥放音效
                    yield return new WaitForSeconds(textView_readSpeed);
                }

                if (sel_mainType == Enum_mainType.SystemMessage) yield break; //視窗訊息模式時, 只允許顯示一句文字
                textView_cursor.SetActive(true); //呼叫閃爍標

                if (i < content.Length - 1)
                {
                    switch (sel_continueSpeechKey)
                    {
                        case Enum_continueSpeechKey.任意:
                            yield return new WaitUntil(() => Input.anyKeyDown);
                            break;

                        case Enum_continueSpeechKey.指定:
                            yield return new WaitUntil(() => Input.GetKeyDown(sel_key));
                            break;
                    }
                }

                textView_cursor.SetActive(false); //按下繼續對話按鍵後, 關閉閃爍標
                PlayArrangeSound(Enum_soundTiming.閃爍標鍵入時); //撥放音效
            }
        }

        //Debug.Log("對話框淡入結束 [" + timer + "]");
    }

    //模組化方法
    //選項淡入協程
    private IEnumerator Cor_PartMethod_SelectionFadeIn(Enum_selectionFadeInAnimation sel, UnityAction[] actions, string[] selections)
    {
        //Debug.Log("選項淡入開始 [" + timer + "]");
        int selectionCount = 0;
        if (actions.Length == selections.Length) selectionCount = actions.Length; //校驗actions長度和selections長度是否相同
        PlayArrangeSound(Enum_soundTiming.選項淡入時); //撥放音效

        List<Vector2[]> posLocusPack = new List<Vector2[]>(); //儲存各選項移動軌跡的List
        List<float[]> alphaLocusPack = new List<float[]>(); //儲存各選項透明度變化

        //計算選項左標的演算法
        GetSeletionPos_X GetX = (balloon, selection, length, index, padding) => { return (balloon.localPosition.x + ((((1 - length) / 2f) + index) * (selection.sizeDelta.x + padding))); };
        GetSeletionPos_Y GetY = (balloon, selection, padding) => { return (balloon.localPosition.y - (balloon.sizeDelta.y / 2f) - (selection.sizeDelta.y / 2f) - padding); };

        for (int i = 0; i < selectionCount; i++) //逐個產生選項
        {
            GameObject go = Instantiate(seletion_prefab, selection_holder); //產生物件
            go.name = "[Selection " + i + "]";

            CanvasGroup cg; //取得物件上的CanvasGroup
            if (go.GetComponent<CanvasGroup>() == null) cg = go.AddComponent<CanvasGroup>();
            else cg = go.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false; //暫時設為不可按

            Text sel_text = null; //取得物件上的Text
            if (go.GetComponent<Text>() == null && go.GetComponentInChildren<Text>() == null) sel_text = go.AddComponent<Text>();
            else if (go.GetComponentInChildren<Text>() != null) sel_text = go.GetComponentInChildren<Text>();
            else if (go.GetComponent<Text>() != null && go.GetComponentInChildren<Text>() == null) sel_text = go.GetComponent<Text>();
            sel_text.text = selections[i]; //設定選項字串

            Button sel_btn = null; //取得物件上的Button
            if (go.GetComponent<Button>() == null) sel_btn = go.AddComponent<Button>();
            else sel_btn = go.GetComponent<Button>();
            go.GetComponent<Button>().onClick.AddListener(SelectFunction); //設定選項按下後的執行方法

            selectionPack.Add(go); //將實例化出來的選項物件加進List

            if (sel_mainType == Enum_mainType.SystemMessage) //訊息視窗模式, 省略動畫直接完成選項淡入
            {
                go.name = "[Message Window Button]";
                cg.alpha = 0.5f; //按鈕暫時先設為不激活
                if (sel_waitingButtonActiveMode == Enum_waitingButtonActiveMode.隱藏) go.gameObject.SetActive(false);
                yield break;
            }

            Vector2[] locus_pos = new Vector2[2]; //選項位置變化
            float[] locus_alpha = new float[2]; //透明度變化

            //演算法參數
            RectTransform rt_b = balloon_img == null ? textView_text.GetComponent<RectTransform>() : balloon_img.rectTransform;
            RectTransform rt_s = go.GetComponent<RectTransform>();
            switch (sel)
            {
                case Enum_selectionFadeInAnimation.淡入:
                    locus_pos[0] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(初始位置)
                    locus_pos[1] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(最終位置)
                    locus_alpha[0] = 0; //透明度變化(初始值)
                    locus_alpha[1] = 1; //透明度變化(最終值)
                    break;

                case Enum_selectionFadeInAnimation.橫向擴散:
                    locus_pos[0] = new Vector2(rt_b.localPosition.x, GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(初始位置)
                    locus_pos[1] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(最終位置)
                    locus_alpha[0] = 0; //透明度變化(初始值)
                    locus_alpha[1] = 1; //透明度變化(最終值)
                    break;

                case Enum_selectionFadeInAnimation.浮現:
                    locus_pos[0] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y) - 20); //位置移動軌跡(初始位置)
                    locus_pos[1] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(最終位置)
                    locus_alpha[0] = 0; //透明度變化(初始值)
                    locus_alpha[1] = 1; //透明度變化(最終值)
                    break;

                case Enum_selectionFadeInAnimation.掉落:
                    locus_pos[0] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y) + 80); //位置移動軌跡(初始位置)
                    locus_pos[1] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(最終位置)
                    locus_alpha[0] = 0; //透明度變化(初始值)
                    locus_alpha[1] = 1; //透明度變化(最終值)
                    break;

                case Enum_selectionFadeInAnimation.無動畫:
                    locus_pos[0] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(初始位置)
                    locus_pos[1] = new Vector2(GetX(rt_b, rt_s, selectionCount, i, selection_padding.x), GetY(rt_b, rt_s, selection_padding.y)); //位置移動軌跡(最終位置)
                    locus_alpha[0] = 1; //透明度變化(初始值)
                    locus_alpha[1] = 1; //透明度變化(最終值)
                    break;
            }
            posLocusPack.Add(locus_pos);
            alphaLocusPack.Add(locus_alpha);
        }

        float timerTag = timer; //標記現在時間
        bool startFadeIn = true;
        while (startFadeIn)
        {
            if (timer >= selectionFadeIn_costTime + timerTag) //結束淡入特效
            {
                startFadeIn = false;
            }

            for (int i = 0; i < selectionPack.Count; i++)
            {
                Transform t = selectionPack[i].transform;
                Vector2 deltaPos = posLocusPack[i][1] - posLocusPack[i][0];
                Vector2 nowPos = (deltaPos / selectionFadeIn_costTime) * (timer - timerTag) + posLocusPack[i][0];
                t.localPosition = startFadeIn ? nowPos : posLocusPack[i][1]; //位置變化

                CanvasGroup cg;
                if (selectionPack[i].GetComponent<CanvasGroup>() == null) cg = selectionPack[i].AddComponent<CanvasGroup>();
                else cg = selectionPack[i].GetComponent<CanvasGroup>();
                float deltaAlpha = alphaLocusPack[i][1] - alphaLocusPack[i][0];
                float nowAlpha = (deltaAlpha / selectionFadeIn_costTime) * (timer - timerTag) + alphaLocusPack[i][0];
                cg.alpha = startFadeIn ? nowAlpha : alphaLocusPack[i][1]; //透明度變化
            }

            yield return null;
        }

        //Debug.Log("選項淡入結束 [" + timer + "]");
        fadeInOver[1] = true;
    }

    //模組化方法
    //偵測淡入程序完成後, 設選項為可按
    private IEnumerator Cor_PartMethod_SetInteractable()
    {
        yield return new WaitUntil(() => (fadeInOver[0] && fadeInOver[1])); //淡入程序完成(布幕淡入&選項淡入)

        for (int i = 0; i < selectionPack.Count; i++)
        {
            CanvasGroup cg = selectionPack[i].GetComponent<CanvasGroup>();
            if (!cg.blocksRaycasts) cg.blocksRaycasts = true;
            if (!cg.interactable) cg.interactable = true;
        }

        isTimerStart = false; //計時器結束

        yield return null;
    }

    //模組化方法
    //選項事件
    private IEnumerator Cor_PartMethod_SelectionEvent(int index, Enum_selectionFadeOutAnimation sel)
    {
        //Debug.Log("玩家選擇<" + index + "> [" + timer + "]");
        for (int i = 0; i < selectionPack.Count; i++) //所有選項按鈕設為不可按
        {
            CanvasGroup cg = selectionPack[i].GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        }

        StartCoroutine(ClockTimer()); //開始計時器
        StartCoroutine(Cor_PartMethod_SelectionFadeOutComputer(index, sel)); //選項淡出動畫

        yield return new WaitForSeconds(afterSelect_delay);

        if (selectionEvents[index] != null) selectionEvents[index].Invoke();
        PlayArrangeSound(Enum_soundTiming.選項事件發生時); //撥放音效
        //Debug.Log("發生選項事件 [" + timer + "]");
        fadeOutOver[3] = true;
    }

    //模組化方法
    //選項淡出控制協程
    private IEnumerator Cor_PartMethod_SelectionFadeOutComputer(int index, Enum_selectionFadeOutAnimation sel)
    {
        //Debug.Log("選項淡出開始 [" + timer + "]");
        PlayArrangeSound(Enum_soundTiming.選項淡出時); //撥放音效
        float[] trans_selectedAlpha = new float[] { };
        switch (sel)
        {
            case Enum_selectionFadeOutAnimation.變色後延遲淡出:
                for (int i = 0; i < selectionPack.Count; i++) //淡出動畫>>未被選擇的選項
                {
                    if (i != index)
                    {
                        float[] trans_alpha = new float[] { 1, 0 };
                        StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[i].GetComponent<CanvasGroup>(), null, null, null, trans_alpha, 0.2f));
                    }
                }
                //淡出動畫>>選擇的選項
                Color[] trans_Textcolor = new Color[] { new Color(1, 1, 1, 1), new Color(1, 0.95f, 0.65f, 1) };
                yield return StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, selectionPack[index].GetComponentInChildren<Text>(), null, null, null, trans_Textcolor, null, 0.2f));

                yield return new WaitForSeconds(0.35f);

                trans_selectedAlpha = new float[] { 1, 0 };
                yield return StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[index].GetComponent<CanvasGroup>(), null, null, null, trans_selectedAlpha, 0.15f));

                break;

            case Enum_selectionFadeOutAnimation.向中央移動後淡出:
                for (int i = 0; i < selectionPack.Count; i++) //淡出動畫>>未被選擇的選項
                {
                    if (i != index)
                    {
                        float[] trans_alpha = new float[] { 1, 0 };
                        StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[i].GetComponent<CanvasGroup>(), null, null, null, trans_alpha, 0.15f));
                    }
                }
                //淡出動畫>>選擇的選項
                Vector2[] trans_selectedPos = new Vector2[] { selectionPack[index].transform.localPosition, new Vector2(textView_text.transform.localPosition.x, selectionPack[index].transform.localPosition.y) };
                yield return StartCoroutine(Cor_PartMethod_SelectionFadeOut(selectionPack[index].transform, null, null, trans_selectedPos, null, null, null, 0.2f));

                yield return new WaitForSeconds(0.3f);

                trans_selectedAlpha = new float[] { 1, 0 };
                yield return StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[index].GetComponent<CanvasGroup>(), null, null, null, trans_selectedAlpha, 0.2f));

                break;

            case Enum_selectionFadeOutAnimation.放大後淡出:
                for (int i = 0; i < selectionPack.Count; i++) //淡出動畫>>未被選擇的選項
                {
                    if (i != index)
                    {
                        float[] trans_alpha = new float[] { 1, 0 };
                        StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[i].GetComponent<CanvasGroup>(), null, null, null, trans_alpha, 0.3f));
                    }
                }
                //淡出動畫>>選擇的選項
                Vector2[] trans_scale = new Vector2[] { selectionPack[index].transform.localScale, new Vector2(1.2f, 1.2f) };
                StartCoroutine(Cor_PartMethod_SelectionFadeOut(selectionPack[index].transform, null, null, null, trans_scale, null, null, 0.7f));

                yield return new WaitForSeconds(0.4f);

                trans_selectedAlpha = new float[] { 1, 0 };
                yield return StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[index].GetComponent<CanvasGroup>(), null, null, null, trans_selectedAlpha, 0.3f));

                break;

            case Enum_selectionFadeOutAnimation.無動畫:
                for (int i = 0; i < selectionPack.Count; i++) //淡出動畫
                {
                    float[] trans_alpha = new float[] { 1, 0 };
                    StartCoroutine(Cor_PartMethod_SelectionFadeOut(null, null, selectionPack[i].GetComponent<CanvasGroup>(), null, null, null, trans_alpha, 0));
                }
                break;
        }

        //Debug.Log("選項淡出結束 [" + timer + "]");
        fadeOutOver[1] = true;
    }

    //模組化方法
    //選項淡出實作協程
    private IEnumerator Cor_PartMethod_SelectionFadeOut(Transform tra, Component colorObj, CanvasGroup cg, Vector2[] trans_pos, Vector2[] trans_scale, Color[] trans_color, float[] trans_alpha, float costTime)
    {
        float timerTag = timer; //標記現在時間

        //偵測改變顏色之對象類型
        Image img = null;
        Text text = null;
        bool imgOrText = false;
        if (colorObj != null)
        {
            if (colorObj.GetType() == typeof(Image))
            {
                img = (Image)colorObj;
                imgOrText = true;
            }
            else if (colorObj.GetType() == typeof(Text))
            {
                text = (Text)colorObj;
                imgOrText = false;
            }
        }

        bool startFadeIn = true;
        while (startFadeIn)
        {
            if (timer >= costTime + timerTag) //結束淡入特效
            {
                startFadeIn = false;
            }

            if (tra != null)
            {
                if (trans_pos != null)
                {
                    Vector2 deltaPos = trans_pos[1] - trans_pos[0];
                    Vector2 nowPos = (deltaPos / costTime) * (timer - timerTag) + trans_pos[0];
                    tra.localPosition = startFadeIn ? nowPos : trans_pos[1]; //位置變化
                }

                if (trans_scale != null)
                {
                    Vector2 deltaScale = trans_scale[1] - trans_scale[0];
                    Vector2 nowScale = (deltaScale / costTime) * (timer - timerTag) + trans_scale[0];
                    tra.localScale = startFadeIn ? nowScale : trans_scale[1]; //尺寸變化
                }
            }

            if (colorObj != null && trans_color != null)
            {
                Color deltaColor = trans_color[1] - trans_color[0];
                Color nowColor = (deltaColor / costTime) * (timer - timerTag) + trans_color[0];
                if (imgOrText) img.color = startFadeIn ? nowColor : trans_color[1]; //顏色變化(Image)
                else text.color = startFadeIn ? nowColor : trans_color[1]; //顏色變化(Text)
            }

            if (cg != null && trans_alpha != null)
            {
                float deltaAlpha = trans_alpha[1] - trans_alpha[0];
                float nowAlpha = (deltaAlpha / costTime) * (timer - timerTag) + trans_alpha[0];
                cg.alpha = startFadeIn ? nowAlpha : trans_alpha[1]; //透明度變化
            }

            yield return null;
        }
    }

    //模組化方法
    //布幕淡出協程
    private IEnumerator Cor_PartMethod_CurtainFadeOut(Enum_curtainFadeIn sel)
    {
        yield return new WaitUntil(() => timer >= curtainFadeOut_trasDuration.x); //等待布幕淡出
        //Debug.Log("布幕淡出開始 [" + timer + "]");

        PlayArrangeSound(Enum_soundTiming.布幕淡出時); //撥放音效
        if (curtain_cg != null && !curtain_cg.gameObject.activeSelf) curtain_cg.gameObject.SetActive(true);
        if (curtain_img != null && !curtain_img.gameObject.activeSelf) curtain_img.gameObject.SetActive(true);

        if (sel_mainType == Enum_mainType.SystemMessage) //視窗訊息模式, 省略動畫直接完成布幕淡出
        {
            switch (sel)
            {
                case Enum_curtainFadeIn.CanvasGroup_alpha:
                    curtain_cg.alpha = curtainFadeOut_tranStates.y;
                    break;

                case Enum_curtainFadeIn.Image_Color:
                    curtain_img.color = curtainFadeOut_transColors[1];
                    break;
            }

            if (curtain_cg != null)
            {
                curtain_cg.gameObject.SetActive(false);
                curtain_cg.alpha = curtainFadeOut_tranStates.x; //隱藏後, 初始化透明度
            }
            if (curtain_img != null)
            {
                curtain_img.gameObject.SetActive(false);
                curtain_img.color = curtainFadeOut_transColors[0]; //隱藏後, 初始化顏色
            }

            fadeOutOver[0] = true;
            yield break;
        }

        float deltaTimer = curtainFadeOut_trasDuration.y - curtainFadeOut_trasDuration.x;
        float deltaState = curtainFadeOut_tranStates.y - curtainFadeOut_tranStates.x;
        float nowAlpha = 0;
        Color deltaColor = curtainFadeOut_transColors[1] - curtainFadeOut_transColors[0];
        Color nowColor;

        bool startFadeIn = true;
        while (startFadeIn) //實作淡出特效
        {
            if (timer >= curtainFadeOut_trasDuration.y) //結束淡出特效
            {
                startFadeIn = false;
            }

            switch (sel)
            {
                case Enum_curtainFadeIn.CanvasGroup_alpha:
                    nowAlpha = (deltaState / deltaTimer) * (timer - curtainFadeOut_trasDuration.x) + curtainFadeOut_tranStates.x;
                    curtain_cg.alpha = startFadeIn ? nowAlpha : curtainFadeOut_tranStates.y;
                    break;

                case Enum_curtainFadeIn.Image_Color:
                    nowColor = (deltaColor / deltaTimer) * (timer - curtainFadeOut_trasDuration.x) + curtainFadeOut_transColors[0];
                    curtain_img.color = startFadeIn ? nowColor : curtainFadeOut_transColors[1];
                    break;
            }

            yield return null;
        }

        //布幕淡出結束後, 隱藏布幕
        if (curtain_cg != null)
        {
            curtain_cg.gameObject.SetActive(false);
            curtain_cg.alpha = curtainFadeOut_tranStates.x; //隱藏後, 初始化透明度
        }
        if (curtain_img != null)
        {
            curtain_img.gameObject.SetActive(false);
            curtain_img.color = curtainFadeOut_transColors[0]; //隱藏後, 初始化顏色
        }

        //Debug.Log("布幕淡出結束 [" + timer + "]");
        fadeOutOver[0] = true;
    }

    //模組化方法
    //對話框淡出協程
    private IEnumerator Cor_PartMethod_SpeechBalloonFadeOut(Enum_balloonFadeIn sel)
    {
        yield return new WaitUntil(() => timer >= balloonFadeOut_trasDuration.x); //等待對話框淡出
        //Debug.Log("對話框淡出開始 [" + timer + "]");

        PlayArrangeSound(Enum_soundTiming.對話框淡出時); //撥放音效
        if (balloon_img != null && !balloon_img.gameObject.activeSelf) balloon_img.gameObject.SetActive(true);
        if (textView_text != null && !textView_text.gameObject.activeSelf) textView_text.gameObject.SetActive(true);

        Color textOriginColor = textView_text.color; //文字原色
        Color textTargetColor = new Color(textOriginColor.r, textOriginColor.g, textOriginColor.b, 0); //文字目標顏色(淡出>>透明度0)

        if (sel_mainType == Enum_mainType.SystemMessage) //訊息視窗模式, 省略動畫直接完成對話框淡出
        {
            switch (sel)
            {
                case Enum_balloonFadeIn.On:
                    balloon_img.color = balloonFadeOut_transColors[1];
                    textView_text.color = textTargetColor;
                    break;

                case Enum_balloonFadeIn.Off:
                    textView_text.color = textTargetColor;
                    break;
            }

            //隱藏對話框
            if (balloon_img != null)
            {
                balloon_img.gameObject.SetActive(false);
                balloon_img.color = balloonFadeOut_transColors[0]; //隱藏後, 初始化對話框顏色
                if (textView_text != null) textView_text.color = textOriginColor; //初始化文字顏色
            }
            else if (balloon_img == null && textView_text != null)
            {
                textView_text.gameObject.SetActive(false);
                textView_text.color = textOriginColor; //隱藏後, 初始化文字顏色
            }

            fadeOutOver[2] = true;
            yield break;
        }

        //設定計算用參數
        float timerTag = timer;
        float until = sel == Enum_balloonFadeIn.On ? balloonFadeOut_trasDuration.y : timerTag + 0.3f;
        float deltaTimer = balloonFadeOut_trasDuration.y - balloonFadeOut_trasDuration.x;
        Color deltaColor = balloonFadeOut_transColors[1] - balloonFadeOut_transColors[0];
        Color nowColor;
        Color nowTextColor;
        Color deltaTextColor = textTargetColor - textOriginColor;

        bool startFadeIn = true;
        while (startFadeIn) //實作淡出特效
        {
            if (timer >= until) //結束淡出特效
            {
                startFadeIn = false;
            }

            switch (sel)
            {
                case Enum_balloonFadeIn.On:
                    nowColor = (deltaColor / deltaTimer) * (timer - balloonFadeOut_trasDuration.x) + balloonFadeOut_transColors[0];
                    balloon_img.color = startFadeIn ? nowColor : balloonFadeOut_transColors[1];

                    nowTextColor = (deltaTextColor / 0.3f) * (timer - timerTag) + textOriginColor;
                    textView_text.color = startFadeIn ? nowTextColor : textTargetColor;
                    break;

                case Enum_balloonFadeIn.Off:
                    nowTextColor = (deltaTextColor / 0.3f) * (timer - timerTag) + textOriginColor;
                    textView_text.color = startFadeIn ? nowTextColor : textTargetColor;
                    break;
            }

            yield return null;
        }

        //對話框淡出結束後, 隱藏對話框
        if (balloon_img != null)
        {
            balloon_img.gameObject.SetActive(false);
            balloon_img.color = balloonFadeOut_transColors[0]; //隱藏後, 初始化對話框顏色
            if (textView_text != null) textView_text.color = textOriginColor; //初始化文字顏色
        }
        else if (balloon_img == null && textView_text != null)
        {
            textView_text.gameObject.SetActive(false);
            textView_text.color = textOriginColor; //隱藏後, 初始化文字顏色
        }

        //Debug.Log("對話框淡出結束 [" + timer + "]");
        fadeOutOver[2] = true;
    }

    //模組化方法
    //等待視窗特效
    private IEnumerator Cor_PartMethod_MessageWindowWaiting(string circleTxt)
    {
        float circleTxtTimer = 0; //循環讀字計時器
        int circleTimes = 1; //循環到第幾字
        string originalText = textView_text.text; //原對話框字串
        bool btnState = false;

        while (isWaiting && selected == -1)
        {
            switch (sel_waitingType)
            {
                case Enum_waitingType.圖片旋轉:
                    if (!wait_rotateImg.gameObject.activeSelf) wait_rotateImg.gameObject.SetActive(true);
                    wait_rotateImg.transform.Rotate(Time.deltaTime * Vector3.forward * wait_circleSpeed);
                    break;

                case Enum_waitingType.進度條循環:
                    if (!wait_circulateSlider.gameObject.activeSelf) wait_circulateSlider.gameObject.SetActive(true);
                    wait_circulateSlider.value = (wait_circulateSlider.value + (Time.deltaTime * 0.01f * wait_circleSpeed)) % 1;
                    break;

                case Enum_waitingType.激活物件:
                    if (!wait_activeGameobject.gameObject.activeSelf) wait_activeGameobject.gameObject.SetActive(true);
                    break;
            }

            if (isTxtCircle && circleTxtTimer >= wait_textCircleSpeed) //循環讀字特效
            {
                if (sel_waitingButtonActiveMode == Enum_waitingButtonActiveMode.顯示且激活 && !btnState)
                {
                    btnState = true;
                    selectionPack[0].GetComponent<CanvasGroup>().alpha = 1;
                    selectionPack[0].GetComponent<CanvasGroup>().blocksRaycasts = true;
                    selectionPack[0].GetComponent<CanvasGroup>().interactable = true;
                }

                circleTxtTimer -= wait_textCircleSpeed; //秒數重新計算
                textView_text.text = originalText + circleTxt.Substring(0, circleTimes); //插入文字
                circleTimes = (circleTimes + 1) % (circleTxt.Length + 1); //設定目前循環至第幾字
            }

            circleTxtTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitWhile(() => isWaiting && (selected == -1)); //當isWaiting被關閉時, 終止等待狀態
        isWaiting = false;

        //選項可正常操作
        selectionPack[0].gameObject.SetActive(true);
        selectionPack[0].GetComponent<CanvasGroup>().alpha = 1;
        selectionPack[0].GetComponent<CanvasGroup>().blocksRaycasts = true;
        selectionPack[0].GetComponent<CanvasGroup>().interactable = true;
        fadeInOver[1] = true;

        switch (sel_waitingType) //初始化並隱藏
        {
            case Enum_waitingType.圖片旋轉:
                wait_rotateImg.transform.rotation = Quaternion.identity;
                wait_rotateImg.gameObject.SetActive(false);
                break;

            case Enum_waitingType.進度條循環:
                wait_circulateSlider.value = 0;
                wait_circulateSlider.gameObject.SetActive(false);
                break;

            case Enum_waitingType.激活物件:
                wait_activeGameobject.gameObject.SetActive(false);
                break;
        }

        textView_text.text = originalText; //使對話框字串恢復原樣

    }

    #endregion
}

