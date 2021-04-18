using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipUnit //管理音訊剪輯的列表
{
    [SerializeField]
    private List<AudioClip> clips = new List<AudioClip>(); //撥放剪輯
    [SerializeField]
    private string name; //名稱
    [SerializeField]
    private List<string> tag = new List<string>(); //標籤(可複數)
    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float volumeScale = 1.0f; //音量輸出率, 用以微調音效的各自音量(會再和AudioManagerCore.setVolume加算)
    [SerializeField]
    private byte outputSourceNum; //使用的音源標號

    public List<AudioClip> GetClips { get { return clips; } } //取得撥放剪輯
    public string GetName { get { return name; } } //取得名稱
    public List<string> GetTag { get { return tag; } } //取得標籤
    public void SetTag(string[] tagStr) //設定標籤
    {
        string[] tmp_tag = tagStr;

        if (tmp_tag == null) tmp_tag = new string[] { "" }; //以空字串代替null
        if (tmp_tag.Length == 0) tmp_tag = new string[] { "" }; //以空字串代替空內容
        tag = new List<string>();
        tag.AddRange(tmp_tag);
    }
    public float GetVolumeScale { get { return volumeScale; } } //取得音量輸出率
    public byte GetOutputSourceNum { get { return outputSourceNum; } } //取得音源編號
    public byte SetOutputSourceNum { set { outputSourceNum = (byte)Mathf.Clamp(value, 0, 254); } } //設定音源編號(不可超過254)
}

[System.Serializable]
public class AudioSourceUnit //管理音源的列表
{
    public AudioSource source;
    [SerializeField]
    private byte sourceNum; //編號
    [SerializeField]
    private string name; //名稱
    [SerializeField]
    private List<string> tag = new List<string>(); //標籤(可複數)
    [SerializeField]
    private AudioEffectMode fadeInEffectMode = AudioEffectMode.無; //淡入效果:預設為一般
    [SerializeField]
    private AudioEffectMode fadeOutEffectMode = AudioEffectMode.無; //淡出效果:預設為一般
    [SerializeField]
    private OutputMethod outputMethod = OutputMethod.Play; //撥放方法:預設為Play(不支援複數Clip)
    [SerializeField]
    private float iniParam_becomeFastRate = 0.5f; //(淡入參數)聲調漸快幅度百分比 Ex:0.2f(20%) > Pitch基值1.0淡入時0.8→1.0;淡出時1.0→1.2 或是Pitch基值1.2淡入時0.96→1.2;淡出時1.2→1.44
    [SerializeField]
    private float iniParam_becomeLowerRate = 0.35f; //(淡入參數)聲調漸慢幅度百分比
    [SerializeField]
    private float duration_fadeIn = 0.5f; //(淡入參數)淡入作動時間
    [SerializeField]
    private float duration_fadeOut = 0.5f; //(淡入參數)淡出作動時間

    public byte GetSourceNum { get { return sourceNum; } } //取得音源編號
    public string GetName { get { return name; } } //取得名稱
    public List<string> GetTag { get { return tag; } } //取得標籤

    //設定淡入參數
    //(多載1/2) fadeInMode:淡入模式 / fadeOutMode:淡出模式
    public void SetFadeParam(AudioEffectMode fadeInMode, AudioEffectMode fadeOutMode)
    {
        fadeInEffectMode = fadeInMode;
        fadeOutEffectMode = fadeOutMode;
    }
    //(多載2/2) becomeFastRate:聲調漸快幅度百分比 / becomeLowerRate:聲調漸慢幅度百分比
    public void SetFadeParam(float becomeFastRate, float becomeLowerRate)
    {
        iniParam_becomeFastRate = Mathf.Clamp(becomeFastRate, 0.0f, 1.0f);
        iniParam_becomeLowerRate = Mathf.Clamp(becomeLowerRate, 0.0f, 1.0f);
    }
    public AudioEffectMode GetFadeInEffectMode { get { return fadeInEffectMode; } } //取得淡入模式
    public AudioEffectMode GetFadeOutEffectMode { get { return fadeOutEffectMode; } } //取得淡出模式
    public OutputMethod GetOutputMethod { get { return outputMethod; } } //取得撥放方法
    public OutputMethod SetOutputMethod //設定撥放方法
    {
        set
        {
            if (value == OutputMethod.OneShot) fadeInEffectMode = AudioEffectMode.無;
            outputMethod = value;
        }
    }
    public float GetFastRate { get { return iniParam_becomeFastRate; } } //取得聲調漸快幅度百分比
    public float GetLowerRate { get { return iniParam_becomeLowerRate; } } //取得聲調漸慢幅度百分比
    public float[] GetFadeDuration { get { return new float[] { duration_fadeIn, duration_fadeOut }; } } //取得淡入淡出作動時間([0]=淡入作動時間/[1]=淡出作動時間)
    public float SetFadeInDuration //設定淡入作動時間
    {
        set
        {
            duration_fadeIn = value < 0.0f ? 0.0f : value;
        }
    }
    public float SetFadeOutDuration //設定淡入作動時間
    {
        set
        {
            duration_fadeOut = value < 0.0f ? 0.0f : value;
        }
    }
}

public class AudiosPack : MonoBehaviour
{
    private static AudiosPack _instance; //單例
    public static AudiosPack Instance
    {
        get { return _instance; }
    }

    public List<ClipUnit> clipsPack = new List<ClipUnit>(); //音訊卡
    public List<AudioSourceUnit> audioSourcesPack = new List<AudioSourceUnit>(); //音源卡

    public Dictionary<byte, List<AudioSourceUnit>> audioSourceDict = new Dictionary<byte, List<AudioSourceUnit>>(); //從SourceNum查找AudioSourceUnit清單的Dictionary
    public Dictionary<AudioSource, float> initialVolumeDict = new Dictionary<AudioSource, float>(); //從AudioSource查找初始音量的Dictionary
    public Dictionary<AudioSource, byte> audioSourceStateDict = new Dictionary<AudioSource, byte>(); //紀錄所有AudioSource的目前狀態(0 = 完全停止 / 1 = 撥放中 / 2 = 停止中)

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例
    }

    void Start()
    {
        SetAudioSourceDict();
    }

    //彙整audioSourceDict
    private void SetAudioSourceDict()
    {
        if (audioSourceDict.Count != 0) audioSourceDict = new Dictionary<byte, List<AudioSourceUnit>>(); //初始化audioSourceDict

        List<byte> numList = new List<byte>(); //儲存所有出現過的AudioSourceUnit的SourceNum
        List<List<AudioSourceUnit>> allotTable = new List<List<AudioSourceUnit>>(); //將每個對應SourceNum的AudioSourceUnit按照順序編入List

        for (int i = 0; i < audioSourcesPack.Count; i++) //遍歷所有AudioSourceUnit
        {
            if (!initialVolumeDict.ContainsKey(audioSourcesPack[i].source)) initialVolumeDict.Add(audioSourcesPack[i].source, audioSourcesPack[i].source.volume); //紀錄音源的初始音量
            if (!audioSourceStateDict.ContainsKey(audioSourcesPack[i].source)) audioSourceStateDict.Add(audioSourcesPack[i].source, 0); //紀錄音源狀態

            if (numList.Count == 0) //第一個元素
            {
                numList.Add(audioSourcesPack[i].GetSourceNum);
                allotTable.Add(new List<AudioSourceUnit>());
                allotTable[0].Add(audioSourcesPack[i]);
                continue;
            }

            for (int j = 0; j < numList.Count; j++) //第二個元素後, 遍歷現有SourceNum尋找是否有無重複
            {
                if (numList[j] == audioSourcesPack[i].GetSourceNum) //若找到相同者, 將此項AudioSourceUnit加入對應的List
                {
                    allotTable[j].Add(audioSourcesPack[i]);
                    break;
                }

                if (j == numList.Count - 1) //遍歷到最後一項時, 視為新的SourceNum並註冊之
                {
                    numList.Add(audioSourcesPack[i].GetSourceNum);
                    allotTable.Add(new List<AudioSourceUnit>());
                    allotTable[allotTable.Count - 1].Add(audioSourcesPack[i]);
                    break;
                }
            }
        }

        //Debug.Log("-----Test-----");
        //Debug.Log("numList.Count = " + numList.Count);
        //Debug.Log("allotTable.Count = " + allotTable.Count);
        //for (int i = 0; i < allotTable.Count; i++)
        //{
        //    Debug.Log("allotTable[" + i + "].count = " + allotTable[i].Count);
        //}
        //Debug.Log("-----Test2-----");
        //for (int i = 0; i < numList.Count; i++)
        //{
        //    Debug.Log("Num [" + numList[i] + "]");
        //    for (int j = 0; j < allotTable[i].Count; j++)
        //    {
        //        Debug.Log("(" + allotTable[i][j].sourceNum + ")<" + j + "> " + allotTable[i][j].source.name);
        //    }
        //}

        for (int i = 0; i < allotTable.Count; i++) //移除同個SourceNum中指定重複音源
        {
            for (int j = 0; j < allotTable[i].Count; j++)
            {
                for (int k = j + 1; k < allotTable[i].Count; k++)
                {
                    if (allotTable[i][j].source == allotTable[i][k].source)
                    {
                        allotTable[i][k].source = null;
                        Debug.Log("[ERROR]在同個SourceNum[" + allotTable[i][j].GetSourceNum + "]指定重複音源!!");
                    }
                }
            }
        }

        for (int i = 0; i < allotTable.Count; i++) //將彙整好的AudioSourceUnit導入Dictionary中
        {
            audioSourceDict.Add(numList[i], allotTable[i]);
        }

        //Debug.Log("audioSourceDict.Count = " + audioSourceDict.Count);
        //for (int i = 0; i < numList.Count; i++)
        //{
        //    Debug.Log("Dictionary內容測試 ------- Num:" + numList[i]);
        //    for (int j = 0; j < audioSourceDict[numList[i]].Count; j++)
        //    {
        //        Debug.Log("第" + j + "個 [" + audioSourceDict[numList[i]][j].source.name + "] num:" + audioSourceDict[numList[i]][j].sourceNum);
        //    }
        //}
    }

    //設定音量
    //(多載1/2) num:音源編號 / v:音量
    //(多載2/2) v:音量 / tag:音源標籤
    public void SetVolume(byte num, float v) { _setvolume(num, null, v); }
    public void SetVolume(string[] tag, float v) { _setvolume(255, tag, v); }
    private void _setvolume(byte num, string[] tag, float v)
    {
        List<AudioSource> SourceQueue = new List<AudioSource>(); //目標AudioSource佇列

        if (audioSourceDict.ContainsKey(num) && num != 255) //從音源編號搜尋
        {
            for (int i = 0; i < audioSourceDict[num].Count; i++)
            {
                if (!SourceQueue.Contains(audioSourceDict[num][i].source)) SourceQueue.Add(audioSourceDict[num][i].source);
            }
        }

        if (tag != null) //從Tag搜尋
        {
            for (int i = 0; i < audioSourcesPack.Count; i++)
            {
                for (int j = 0; j < audioSourcesPack[i].GetTag.Count; j++)
                {
                    for (int k = 0; k < tag.Length; k++)
                    {
                        if (tag[k] == audioSourcesPack[i].GetTag[j] && !SourceQueue.Contains(audioSourcesPack[i].source))
                        {
                            SourceQueue.Add(audioSourcesPack[i].source);
                            break;
                        }
                    }
                    if (SourceQueue.Contains(audioSourcesPack[i].source)) break;
                }
            }
        }

        for (int i = 0; i < SourceQueue.Count; i++)
        {
            initialVolumeDict[SourceQueue[i]] = v; //重新設定AudioSource的初始音量
            SourceQueue[i].volume = v; //設定AudioSource的現在音量
        }
    }

    //指定ClipUnit改變AudioSourceNum
    //(多載1/2) name:音訊名稱 / num:轉換編號
    //(多載2/2) tag:標籤 / num:轉換編號
    public void ChangeClipSource(string name, byte num) { _changeClipSource(name, null, num); }
    public void ChangeClipSource(string[] tag, byte num) { _changeClipSource(null, tag, num); }
    private void _changeClipSource(string name, string[] tag, byte num)
    {
        if (name == null && tag == null) return;

        List<ClipUnit> clipUnitQueue = new List<ClipUnit>(); //待轉換的ClipUnit佇列

        for (int i = 0; i < clipsPack.Count; i++)
        {
            if (clipsPack[i].GetName == name) //從名稱搜尋
            {
                clipUnitQueue.Add(clipsPack[i]);
                continue;
            }

            if (tag == null) continue;
            if (tag.Length == 0) continue;

            for (int j = 0; j < clipsPack[i].GetTag.Count; j++) //從標籤搜尋
            {
                if (clipUnitQueue.Contains(clipsPack[i])) break;
                for (int k = 0; k < tag.Length; k++)
                {
                    if (clipsPack[i].GetTag[j] == tag[k])
                    {
                        clipUnitQueue.Add(clipsPack[i]);
                        break;
                    }
                }
            }
        }

        if (clipUnitQueue.Count == 0) return; //查無結果時中止程序

        //Debug.Log("clipUnitQueue.Count = " + clipUnitQueue.Count);
        //for (int i = 0; i < clipUnitQueue.Count; i++)
        //{
        //    Debug.Log("名稱:" + clipUnitQueue[i].name);
        //    string tagLabel = "";
        //    for (int j = 0; j < clipUnitQueue[i].tag.Count; j++)
        //    {
        //        tagLabel += clipUnitQueue[i].tag[j] + "/";
        //    }
        //    Debug.Log("Tag:" + tagLabel);
        //    Debug.Log("音源編號:" + clipUnitQueue[i].outputSourceNum);
        //}

        for (int i = 0; i < clipUnitQueue.Count; i++) //轉換佇列中所有ClipUnit的音源編號
        {
            clipUnitQueue[i].SetOutputSourceNum = num;
        }
    }

    //設定ClipUnit的Tag
    public void SetClipTag(string name, string[] tag)
    {
        if (name == null) return;

        for (int i = 0; i < clipsPack.Count; i++)
        {
            if (clipsPack[i].GetName == name)
            {
                clipsPack[i].SetTag(tag);
            }
        }
    }

    //設定淡入淡出參數
    //多載(1/3) num:音源編號 / fadeInEffectMode:淡入模式 / fadeOutEffectMode:淡出模式
    //多載(2/3) num:音源編號 / fadeInEffectMode:淡入模式 / dur_fadeIn:淡入作動時間 / fadeOutEffectMode:淡出模式 / dur_fadeOut:淡出作動時間
    //多載(3/3) num:音源編號 / fastRate:聲調漸快幅度百分比 / lowerRate:聲調漸慢幅度百分比
    public void SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, AudioEffectMode fadeOutEffectMode) { _SetFadeParam(num, fadeInEffectMode, -1, fadeOutEffectMode, -1); }
    public void SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, float dur_fadeIn, AudioEffectMode fadeOutEffectMode, float dur_fadeOut) { _SetFadeParam(num, fadeInEffectMode, dur_fadeIn, fadeOutEffectMode, dur_fadeOut); }
    public void SetFadeParam(byte num, float fastRate, float lowerRate)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetFadeParam(fastRate, lowerRate);
            }
        }
    }
    private void _SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, float dur_fadeIn, AudioEffectMode fadeOutEffectMode, float dur_fadeOut)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetFadeParam(fadeInEffectMode, fadeOutEffectMode);
                if (dur_fadeIn != -1) audioSourcesPack[i].SetFadeInDuration = dur_fadeIn;
                if (dur_fadeOut != -1) audioSourcesPack[i].SetFadeOutDuration = dur_fadeOut;
            }
        }
    }

    //設定撥放方法
    public void SetSourceOutputMethod(byte num, OutputMethod method)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetOutputMethod = method;
            }
        }
    }

    //取得AudioSourceUnit
    public AudioSourceUnit GetAudioSourceUnit(string name)
    {
        List<AudioSourceUnit> unit = new List<AudioSourceUnit>();

        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetName == name) unit.Add(audioSourcesPack[i]);
        }

        if (unit.Count > 1) //若搜尋結果超過1個, 則返回錯誤
        {
            Debug.Log("[ERROR]返回結果不可為1個以上");
            return null;
        }

        return unit[0];
    }
}
