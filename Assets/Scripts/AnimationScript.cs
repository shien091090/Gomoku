//播放動畫、控制動畫播放模式以及結束後行為的腳本

//※適用於所有專案的功能性獨立腳本※
//※搭配Editor腳本"AnimationScriptEditor"※

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#region ScriptMovement

//動畫控制由事件輸入的類別
[System.Serializable]
public class PassAnimation : UnityEvent<AnimationClip, AnimationBehavior, bool> { }

//腳本控制物件動畫的類別
public abstract class ScriptMovement
{
    public bool isPlaying = false; //撥放中與否
    protected object variation; //時間變化線類型(float或AnimationCurve)
    protected string IEnumeratorName; //動畫實作協程的名稱
    protected List<System.Type> necessaryComponents = new List<System.Type>(); //必要組件名稱

    //返回動畫進度分子
    public float GetNumerator(float time)
    {
        if (variation.GetType() == typeof(float)) //直線性動畫
        {
            return time;
        }
        else if (variation.GetType() == typeof(AnimationCurve)) //曲線性動畫
        {
            AnimationCurve curve = (AnimationCurve)variation;
            return curve.Evaluate(time);
        }
        else
        {
            return 0;
        }
    }

    //返回動畫進度分母
    public float GetDenominator()
    {
        if (variation.GetType() == typeof(float)) //直線性動畫
        {
            return (float)variation;
        }
        else if (variation.GetType() == typeof(AnimationCurve)) //曲線性動畫
        {
            AnimationCurve curve = (AnimationCurve)variation;
            return curve.keys[curve.keys.Length - 1].value;
            //float max = 0;
            //for (int i = 0; i < curve.keys.Length; i++)
            //{
            //    if (curve.keys[i].value > max) max = curve.keys[i].value;
            //}
            //return max;
        }
        else
        {
            return 0;
        }
    }

    //返回動畫撥放時間
    public float GetEndTime()
    {
        if (variation.GetType() == typeof(float)) //直線性動畫
        {
            return (float)variation;
        }
        else if (variation.GetType() == typeof(AnimationCurve)) //曲線性動畫
        {
            AnimationCurve curve = (AnimationCurve)variation;
            return curve.keys[curve.keys.Length - 1].time;
        }
        else
        {
            return 0;
        }
    }

    //返回動畫實作協程的名稱
    public string GetIEnumeratorName() { return IEnumeratorName; }

    //返回必要組件(Component)列表
    public List<System.Type> GetNecessaryList() { return necessaryComponents; }

    //返回初始狀態
    public virtual void ReturnOriginState(GameObject go) { }
}

//腳本控制顏色
public class ColorScript : ScriptMovement
{
    public Color32[] insOutsColors; //起始與目標顏色

    # region 建構子
    public ColorScript(float costTime, Color32 color)
    {
        _Register();
        variation = costTime;
        insOutsColors = new Color32[] { color };
    }

    public ColorScript(float costTime, Color32 color1, Color32 color2)
    {
        _Register();
        variation = costTime;
        insOutsColors = new Color32[] { color1, color2 };
    }

    public ColorScript(AnimationCurve animCurve, Color32 color)
    {
        _Register();
        variation = animCurve;
        insOutsColors = new Color32[] { color };
    }

    public ColorScript(AnimationCurve animCurve, Color32 color1, Color32 color2)
    {
        _Register();
        variation = animCurve;
        insOutsColors = new Color32[] { color1, color2 };
    }

    //參數註冊
    private void _Register()
    {
        IEnumeratorName = "ChangeColorEnumerator"; //協程方法名
        necessaryComponents.Add(typeof(Image)); //必要組件
    }
    #endregion

    //返回初始狀態
    public override void ReturnOriginState(GameObject go)
    {
        Image img = go.GetComponent<Image>();
        img.color = insOutsColors[0];
    }
}

//腳本控制位置
public class PositionScript : ScriptMovement
{
    public Vector3[] insOutsPositions; //起始與目標位置
    public readonly bool worldOrLocal; //世界座標or在地坐標

    # region 建構子
    public PositionScript(float costTime, Vector3 pos, bool posType)
    {
        _Register();
        variation = costTime;
        worldOrLocal = posType;
        insOutsPositions = new Vector3[] { pos };
    }

    public PositionScript(float costTime, Vector3 pos1, Vector3 pos2, bool posType)
    {
        _Register();
        variation = costTime;
        worldOrLocal = posType;
        insOutsPositions = new Vector3[] { pos1, pos2 };
    }

    public PositionScript(AnimationCurve animCurve, Vector3 pos, bool posType)
    {
        _Register();
        variation = animCurve;
        worldOrLocal = posType;
        insOutsPositions = new Vector3[] { pos };
    }

    public PositionScript(AnimationCurve animCurve, Vector3 pos1, Vector3 pos2, bool posType)
    {
        _Register();
        variation = animCurve;
        worldOrLocal = posType;
        insOutsPositions = new Vector3[] { pos1, pos2 };
    }

    //參數註冊
    private void _Register()
    {
        IEnumeratorName = "ChangePositionEnumerator"; //協程方法名
    }
    #endregion

    //返回初始狀態
    public override void ReturnOriginState(GameObject go)
    {
        Transform tra = go.transform;

        if (worldOrLocal) tra.position = insOutsPositions[0];
        else tra.localPosition = insOutsPositions[0];
    }
}

//腳本控制透明度
public class AlphaScript : ScriptMovement
{
    public float[] insOutsAlpha; //起始與目標位置

    # region 建構子
    public AlphaScript(float costTime, float a)
    {
        _Register();
        variation = costTime;
        insOutsAlpha = new float[] { a };
    }

    public AlphaScript(float costTime, float a1, float a2)
    {
        _Register();
        variation = costTime;
        insOutsAlpha = new float[] { a1, a2 };
    }

    public AlphaScript(AnimationCurve animCurve, float a)
    {
        _Register();
        variation = animCurve;
        insOutsAlpha = new float[] { a };
    }

    public AlphaScript(AnimationCurve animCurve, float a1, float a2)
    {
        _Register();
        variation = animCurve;
        insOutsAlpha = new float[] { a1, a2 };
    }

    //參數註冊
    private void _Register()
    {
        IEnumeratorName = "ChangeAlphaEnumerator"; //協程方法名
        necessaryComponents.Add(typeof(CanvasGroup)); //必要組件
    }
    #endregion

    //返回初始狀態
    public override void ReturnOriginState(GameObject go)
    {
        CanvasGroup c = go.GetComponent<CanvasGroup>();

        c.alpha = insOutsAlpha[0];
    }
}

//腳本控制尺寸
public class ScaleScript : ScriptMovement
{
    public Vector3[] insOutsScales; //起始與目標位置

    # region 建構子
    public ScaleScript(float costTime, Vector3 s)
    {
        _Register();
        variation = costTime;
        insOutsScales = new Vector3[] { s };
    }

    public ScaleScript(float costTime, Vector3 s1, Vector3 s2)
    {
        _Register();
        variation = costTime;
        insOutsScales = new Vector3[] { s1, s2 };
    }

    public ScaleScript(AnimationCurve animCurve, Vector3 s)
    {
        _Register();
        variation = animCurve;
        insOutsScales = new Vector3[] { s };
    }

    public ScaleScript(AnimationCurve animCurve, Vector3 s1, Vector3 s2)
    {
        _Register();
        variation = animCurve;
        insOutsScales = new Vector3[] { s1, s2 };
    }

    //參數註冊
    private void _Register()
    {
        IEnumeratorName = "ChangeScaleEnumerator"; //協程方法名
    }
    #endregion

    //返回初始狀態
    public override void ReturnOriginState(GameObject go)
    {
        go.transform.localScale = insOutsScales[0];
    }
}

#endregion

//播放動畫的執行方法資訊包
public class ActionSetting
{
    public readonly List<UnityAction> actionList_begin = new List<UnityAction>(); //開始時執行方法
    public readonly List<UnityAction> actionList_end = new List<UnityAction>(); //結束時執行方法

    # region 建構子
    public ActionSetting(UnityAction act, TaskMode mode)
    {
        if (mode == TaskMode.僅開始時) actionList_begin.Add(act);
        if (mode == TaskMode.僅結束時) actionList_end.Add(act);
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin.Add(act);
            actionList_end.Add(act);
        }
    }

    public ActionSetting(List<UnityAction> actList, TaskMode mode)
    {
        if (mode == TaskMode.僅開始時) actionList_begin = actList;
        if (mode == TaskMode.僅結束時) actionList_end = actList;
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin = actList;
            actionList_end = actList;
        }
    }

    public ActionSetting(UnityAction act1, UnityAction act2, TaskMode mode)
    {
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin.Add(act1);
            actionList_end.Add(act2);
        }
    }

    public ActionSetting(List<UnityAction> actList1, List<UnityAction> actList2, TaskMode mode)
    {
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin = actList1;
            actionList_end = actList2;
        }
    }

    public ActionSetting(List<UnityAction> actList1, UnityAction act2, TaskMode mode)
    {
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin = actList1;
            actionList_end.Add(act2);
        }
    }

    public ActionSetting(UnityAction act1, List<UnityAction> actList2, TaskMode mode)
    {
        if (mode == TaskMode.開始及結束時)
        {
            actionList_begin.Add(act1);
            actionList_end = actList2;
        }
    }
    # endregion
}

//用於控制動畫撥放行為的資訊包
public class AnimationBehavior
{
    public string playingClipName; //撥放中的剪輯字串
    public readonly PlayMode playMode; //動畫撥放模式
    public readonly ActionSetting actionInfo; //控制執行方法的資訊包
    //以下為便捷參數, 為節省程式碼長度以及避免頻繁使用GetComponent浪費資源, 直接在動畫結束後執行特定而常見的動作
    public bool disableObject = false; //停用物件
    public bool destroyThis = false; //銷毀此物件

    #region 建構子
    public AnimationBehavior(PlayMode m)
    {
        playMode = m;
    }

    public AnimationBehavior(PlayMode m, ActionSetting info)
    {
        playMode = m;
        actionInfo = info;
    }
    #endregion
}

public enum PlayMode //動畫撥放模式
{
    狀態延續, 初始化, 循環撥放
}

public enum TaskMode //事件執行時機
{
    僅開始時, 僅結束時, 開始及結束時
}

public class AnimationScript : MonoBehaviour
{
    #region 變數宣告 --------------------------------------------------------------------------------------------------------------

    public PassAnimation[] eventFlowFactory_before = new PassAnimation[0]; //動畫撥放前事件
    public UnityEvent[] eventFlowFactory_after = new UnityEvent[0]; //動畫撥放後事件
    public int eventStateCount; //事件組數量
    public int defaultEventIndex; //預設事件組索引
    public bool playingLock = true; //動畫撥放時禁止在結束前撥放別的動畫
    public bool animationPlaying = false; //動畫撥放中
    private AnimationBehavior i_behaviorInfo; //動畫撥放行為
    private Animation anim; //動畫播放器
    private Image image;
    private bool isStopLoop = false; //是否停止循環撥放

    #endregion
    #region 內建方法 --------------------------------------------------------------------------------------------------------------

    void Awake()
    {

    }

    #endregion
    #region 自訂方法 --------------------------------------------------------------------------------------------------------------

    //AnimationEvent系列接口為方便編輯器事件(ex:button)使用, 直接拖拉至EventField執行
    //播放動畫與執行可視化事件(初始化)
    public void AnimationEvent_Initial(AnimationClip clip) { Debug.Log(defaultEventIndex - 1); eventFlowFactory_before[defaultEventIndex - 1].Invoke(clip, new AnimationBehavior(PlayMode.初始化), true); }
    public void AnimationEvent_Initial(AnimationClip clip, int index)
    {
        defaultEventIndex = index;
        eventFlowFactory_before[index].Invoke(clip, new AnimationBehavior(PlayMode.初始化), true);
    }
    //播放動畫與執行可視化事件(循環播放)
    public void AnimationEvent_Loop(AnimationClip clip) { eventFlowFactory_before[defaultEventIndex - 1].Invoke(clip, new AnimationBehavior(PlayMode.循環撥放), true); }
    public void AnimationEvent_Loop(AnimationClip clip, int index)
    {
        defaultEventIndex = index;
        eventFlowFactory_before[index].Invoke(clip, new AnimationBehavior(PlayMode.循環撥放), true);
    }
    //播放動畫與執行可視化事件(狀態持續)
    public void AnimationEvent_Stay(AnimationClip clip) { eventFlowFactory_before[defaultEventIndex - 1].Invoke(clip, new AnimationBehavior(PlayMode.狀態延續), true); }
    public void AnimationEvent_Stay(AnimationClip clip, int index)
    {
        defaultEventIndex = index;
        eventFlowFactory_before[index].Invoke(clip, new AnimationBehavior(PlayMode.狀態延續), true);
    }

    //撥放動畫 input:動畫剪輯, 動畫撥放行為
    public void PlayAnimation(AnimationClip clip, AnimationBehavior info, bool eventFlow) { _PlayAnimation(clip, info, eventFlow); }
    public void PlayAnimation(string clipName, AnimationBehavior info, bool eventFlow) { _PlayAnimation(clipName, info, eventFlow); }

    //動畫撥放
    //內部執行方法
    private void _PlayAnimation(object input, AnimationBehavior info, bool eventFlow)
    {
        if (!animationPlaying || (animationPlaying && !playingLock)) //沒有動畫正在撥放 or 有動畫正在撥放但並不禁止重疊撥放
        {
            if (!this.gameObject.activeSelf) this.gameObject.SetActive(true);
            animationPlaying = true; //開始撥放動畫

            bool _eventFlow = eventFlow;
            string clipName = null;
            AnimationClip clip = new AnimationClip();

            if (this.GetComponent<Image>() != null) image = this.GetComponent<Image>();
            if (this.GetComponent<Animation>() == null) this.gameObject.AddComponent<Animation>(); //若物件中不包含Animation組件則掛載之
            if (anim == null) anim = this.GetComponent<Animation>();

            if (image != null && !image.enabled) image.enabled = true; //若image未激活則激活之
            if (!anim.enabled) anim.enabled = true; //若動畫機未激活則激活之

            if (input.GetType() == typeof(AnimationClip)) //取得剪輯名稱
            {
                clip = (AnimationClip)input;
                clipName = clip.name;
                anim.AddClip(clip, clip.name); //將剪輯加入動畫清單
            }
            else if (input.GetType() == typeof(string)) clipName = (string)input;

            i_behaviorInfo = info; //儲存撥放行為資訊
            i_behaviorInfo.playingClipName = clipName; //儲存剪輯名稱

            foreach (AnimationState state in anim) //喚醒剪輯至可撥放狀態
            {
                if (state.name == clipName)
                {
                    state.time = 0;
                    state.enabled = true;
                }
            }

            if (i_behaviorInfo.actionInfo != null && !_eventFlow) StartCoroutine(RunEndingMethod(i_behaviorInfo.actionInfo.actionList_begin)); //播放開始時執行方法

            anim.PlayQueued(clipName, QueueMode.PlayNow);
            StartCoroutine(EndingBehavior(_eventFlow)); //撥放結束後執行協程
        }
    }

    //腳本動畫(單一)
    public void AnimationCommand(ScriptMovement script, AnimationBehavior info)
    {
        AnimationCommand(new List<ScriptMovement>() { script }, info);
    }

    //腳本動畫(複數)
    public void AnimationCommand(List<ScriptMovement> scripts, AnimationBehavior info)
    {
        if (!animationPlaying || (animationPlaying && !playingLock)) //沒有動畫正在撥放 or 有動畫正在撥放但並不禁止重疊撥放
        {
            if (!this.gameObject.activeSelf) this.gameObject.SetActive(true);
            animationPlaying = true; //開始撥放動畫

            i_behaviorInfo = info; //儲存撥放行為資訊

            if (i_behaviorInfo.actionInfo != null) StartCoroutine(RunEndingMethod(i_behaviorInfo.actionInfo.actionList_begin)); //播放開始時執行方法

            float totalPlayTime = 0;
            for (int i = 0; i < scripts.Count; i++) //設定動畫作動時間
            {
                if (scripts[i].GetEndTime() > totalPlayTime)
                {
                    totalPlayTime = scripts[i].GetEndTime();
                }
            }

            for (int i = 0; i < scripts.Count; i++) //呼叫動畫實作協程
            {
                for (int j = 0; j < scripts[i].GetNecessaryList().Count; j++) //判斷並掛載必要方法
                {
                    if (this.GetComponent(scripts[i].GetNecessaryList()[j]) == null)
                    {
                        this.gameObject.AddComponent(scripts[i].GetNecessaryList()[j]);
                    }
                }
                scripts[i].isPlaying = true;
                StartCoroutine(scripts[i].GetIEnumeratorName(), scripts[i]); //動畫效果實作協程
            }
            StartCoroutine(ScriptMovementProcess(totalPlayTime, scripts, info)); //動畫播放行為控制協程
        }
    }

    //設定預設事件組索引
    public void SetDefaultIndex(int index)
    {
        defaultEventIndex = index;
    }

    //停止所有循環動畫
    public void StopAnyLoop()
    {
        if (!isStopLoop) StartCoroutine(StopLoopCoroutine());
    }

    #endregion
    #region 協同程序 --------------------------------------------------------------------------------------------------------------

    //動畫結束行為
    private IEnumerator EndingBehavior(bool eventFlow)
    {
        yield return new WaitWhile(() => anim.isPlaying); //等待撥放結束

        //確實將動畫撥放到結尾(防止BUG)
        anim.Play(i_behaviorInfo.playingClipName);
        anim[i_behaviorInfo.playingClipName].time = anim[i_behaviorInfo.playingClipName].length;
        anim[i_behaviorInfo.playingClipName].enabled = true;
        anim.Sample();
        anim[i_behaviorInfo.playingClipName].enabled = false;

        yield return null;

        if (i_behaviorInfo.disableObject) this.gameObject.SetActive(false);

        switch (i_behaviorInfo.playMode)
        {
            case PlayMode.狀態延續:

                if (eventFlow) eventFlowFactory_after[defaultEventIndex - 1].Invoke(); //執行可視化事件
                else if (!eventFlow && i_behaviorInfo.actionInfo != null) yield return StartCoroutine(RunEndingMethod(i_behaviorInfo.actionInfo.actionList_end)); //播放結束時執行方法

                anim.enabled = false;
                animationPlaying = false; //撥放動畫結束
                yield break;

            case PlayMode.初始化:
                anim.Play(i_behaviorInfo.playingClipName);
                anim[i_behaviorInfo.playingClipName].time = 0;
                anim[i_behaviorInfo.playingClipName].enabled = true;
                anim.Sample();
                anim[i_behaviorInfo.playingClipName].enabled = false;

                if (eventFlow) eventFlowFactory_after[defaultEventIndex - 1].Invoke(); //執行可視化事件
                else if (!eventFlow && i_behaviorInfo.actionInfo != null) yield return StartCoroutine(RunEndingMethod(i_behaviorInfo.actionInfo.actionList_end)); //播放結束時執行方法

                anim.enabled = false;
                animationPlaying = false; //撥放動畫結束
                yield break;

            case PlayMode.循環撥放:
                animationPlaying = false; //撥放動畫結束
                if (!isStopLoop) PlayAnimation(i_behaviorInfo.playingClipName, i_behaviorInfo, eventFlow);
                break;
        }

        if (i_behaviorInfo.destroyThis) Destroy(this.gameObject); //銷毀物件開關
    }

    //等待執行方法
    private IEnumerator RunEndingMethod(List<UnityAction> actionList)
    {
        for (int i = 0; i < actionList.Count; i++)
        {
            actionList[i].Invoke();
        }
        yield return null;
    }

    //動畫腳本進程(實作動畫撥放行為)
    private IEnumerator ScriptMovementProcess(float time, List<ScriptMovement> scripts, AnimationBehavior info)
    {
        //yield return new WaitUntil(() => ());

        float timer = 0; //計時器

        while (timer < time)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (info.actionInfo != null) //播放結束時執行方法
        {
            for (int i = 0; i < info.actionInfo.actionList_end.Count; i++)
            {
                info.actionInfo.actionList_end[i].Invoke();
            }
        }
        if (info.disableObject) this.gameObject.SetActive(false);

        animationPlaying = false; //撥放動畫結束

        if (info.playMode == PlayMode.循環撥放)
        {
            if (!isStopLoop) AnimationCommand(scripts, info);
        }
        else if (info.playMode == PlayMode.初始化)
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].ReturnOriginState(this.gameObject);
            }
        }

        if (info.destroyThis) Destroy(this.gameObject); //銷毀物件開關
    }

    //改變顏色
    private IEnumerator ChangeColorEnumerator(ColorScript info)
    {
        bool playing = true;
        float timer = 0; //計時器
        float endTime = info.GetEndTime(); //動畫結束時間
        float denominator = info.GetDenominator(); //動畫進度分母
        Image img = this.GetComponent<Image>();

        if (!img.enabled) img.enabled = true;

        Color32[] alteration = new Color32[2] { info.insOutsColors.Length == 2 ? info.insOutsColors[0] : (Color32)this.GetComponent<Image>().color, info.insOutsColors[info.insOutsColors.Length - 1] }; //重新定義起始與目標顏色
        info.insOutsColors = alteration;

        while (playing) //變化過程
        {
            timer = timer + Time.fixedDeltaTime > endTime ? endTime : timer + Time.fixedDeltaTime;

            float r = alteration[0].r + ((alteration[1].r - alteration[0].r) * (info.GetNumerator(timer) / denominator));
            float g = alteration[0].g + ((alteration[1].g - alteration[0].g) * (info.GetNumerator(timer) / denominator));
            float b = alteration[0].b + ((alteration[1].b - alteration[0].b) * (info.GetNumerator(timer) / denominator));
            float a = alteration[0].a + ((alteration[1].a - alteration[0].a) * (info.GetNumerator(timer) / denominator));
            img.color = new Color32((byte)Mathf.Clamp(r, 0, 255), (byte)Mathf.Clamp(g, 0, 255), (byte)Mathf.Clamp(b, 0, 255), (byte)Mathf.Clamp(a, 0, 255));

            if (timer == endTime) playing = false;
            yield return new WaitForFixedUpdate();
        }

        info.isPlaying = false;

        yield return null;
    }

    //改變位置
    private IEnumerator ChangePositionEnumerator(PositionScript info)
    {
        bool playing = true;
        float timer = 0; //計時器
        float endTime = info.GetEndTime(); //動畫結束時間
        float denominator = info.GetDenominator(); //動畫進度分母

        bool worldOrLocal = info.worldOrLocal; //世界座標or本地座標
        Vector3[] alteration = new Vector3[2] { info.insOutsPositions.Length == 2 ? info.insOutsPositions[0] : (worldOrLocal ? this.transform.position : this.transform.localPosition), info.insOutsPositions[info.insOutsPositions.Length - 1] };
        info.insOutsPositions = alteration;

        while (playing) //變化過程
        {
            timer = timer + Time.fixedDeltaTime > endTime ? endTime : timer + Time.fixedDeltaTime;

            float x = alteration[0].x + ((alteration[1].x - alteration[0].x) * (info.GetNumerator(timer) / denominator));
            float y = alteration[0].y + ((alteration[1].y - alteration[0].y) * (info.GetNumerator(timer) / denominator));
            float z = alteration[0].z + ((alteration[1].z - alteration[0].z) * (info.GetNumerator(timer) / denominator));
            if (worldOrLocal) this.transform.position = new Vector3(x, y, z);
            else this.transform.localPosition = new Vector3(x, y, z);

            if (timer == endTime) playing = false;
            yield return new WaitForFixedUpdate();
        }

        info.isPlaying = false;

        yield return null;
    }

    //改變透明度
    private IEnumerator ChangeAlphaEnumerator(AlphaScript info)
    {
        bool playing = true;
        float timer = 0; //計時器
        float endTime = info.GetEndTime(); //動畫結束時間
        float denominator = info.GetDenominator(); //動畫進度分母
        CanvasGroup canvasGroup = this.GetComponent<CanvasGroup>();

        float[] alteration = new float[2] { info.insOutsAlpha.Length == 2 ? info.insOutsAlpha[0] : this.GetComponent<CanvasGroup>().alpha, info.insOutsAlpha[info.insOutsAlpha.Length - 1] };
        info.insOutsAlpha = alteration;

        while (playing) //變化過程
        {
            timer = timer + Time.fixedDeltaTime > endTime ? endTime : timer + Time.fixedDeltaTime;

            float a = alteration[0] + ((alteration[1] - alteration[0]) * (info.GetNumerator(timer) / denominator));
            canvasGroup.alpha = a;

            if (timer == endTime) playing = false;
            yield return new WaitForFixedUpdate();
        }

        info.isPlaying = false;

        yield return null;
    }

    //改變尺寸
    private IEnumerator ChangeScaleEnumerator(ScaleScript info)
    {
        bool playing = true;
        float timer = 0; //計時器
        float endTime = info.GetEndTime(); //動畫結束時間
        float denominator = info.GetDenominator(); //動畫進度分母

        Vector3[] alteration = new Vector3[2] { info.insOutsScales.Length == 2 ? info.insOutsScales[0] : this.transform.localScale, info.insOutsScales[info.insOutsScales.Length - 1] };
        info.insOutsScales = alteration;

        while (playing) //變化過程
        {
            timer = timer + Time.fixedDeltaTime > endTime ? endTime : timer + Time.fixedDeltaTime;

            float x = alteration[0].x + ((alteration[1].x - alteration[0].x) * (info.GetNumerator(timer) / denominator));
            float y = alteration[0].y + ((alteration[1].y - alteration[0].y) * (info.GetNumerator(timer) / denominator));
            float z = alteration[0].z + ((alteration[1].z - alteration[0].z) * (info.GetNumerator(timer) / denominator));
            this.transform.localScale = new Vector3(x, y, z);

            if (timer == endTime) playing = false;
            yield return new WaitForFixedUpdate();
        }

        info.isPlaying = false;

        yield return null;
    }

    //停止循環撥放之協程
    private IEnumerator StopLoopCoroutine()
    {
        isStopLoop = true;

        yield return new WaitUntil(() => !animationPlaying); //等待動畫撥放完畢

        yield return new WaitForEndOfFrame(); //暫停1幀(安全性等待)

        isStopLoop = false;
    }

    #endregion

}
