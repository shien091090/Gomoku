using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AudioEffectMode //特殊音訊效果
{
    無, 音量淡化, 音調漸快, 音調漸慢, 音調漸快且淡化, 音調漸慢且淡化
}

public enum OutputMethod //輸出方法
{
    Play, OneShot
}

public class AudioManagerScript : MonoBehaviour
{
    public AudioListener audioListener; //聆聽器

    private static AudioManagerScript _instance; //單例模式
    public static AudioManagerScript Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例
    }

    //簡易撥放
    //(多載1/2) clip:音訊剪輯
    //(多載2/2) clip:音訊剪輯 / v:音量 [強制指定音量]
    public void SimplePlay(AudioClip clip) { _simplePlay(clip, 1); }
    public void SimplePlay(AudioClip clip, float v) { _simplePlay(clip, v); }
    private void _simplePlay(AudioClip clip, float v)
    {
        AudioSource.PlayClipAtPoint(clip, audioListener.transform.localPosition, v);
    }

    //從AudiosPack抓取對應音訊剪輯並撥放
    //(多載1/2) name:名稱
    //(多載2/2) tag:標籤
    public void PlayAudioClip(string name) { _playAudioClip(name, null, false); }
    public void PlayAudioClip(string[] tag) { _playAudioClip(null, tag, false); }
    private void _playAudioClip(string name, string[] tag, bool canCover)
    {
        if (name == null && tag.Length == 0) return; //無指定直接返回

        List<ClipUnit> clipsQueue = new List<ClipUnit>(); //待撥放佇列

        for (int i = 0; i < AudiosPack.Instance.clipsPack.Count; i++) //符合條件的ClipUnit加入待撥放佇列
        {
            if (name == AudiosPack.Instance.clipsPack[i].GetName) //從名字搜尋
            {
                clipsQueue.Add(AudiosPack.Instance.clipsPack[i]);
                continue;
            }

            if (tag == null) continue; //無tag時跳過
            for (int j = 0; j < tag.Length; j++) //從Tag搜尋
            {
                for (int k = 0; k < AudiosPack.Instance.clipsPack[i].GetTag.Count; k++)
                {
                    if (tag[j] == AudiosPack.Instance.clipsPack[i].GetTag[k])
                    {
                        clipsQueue.Add(AudiosPack.Instance.clipsPack[i]);
                    }
                }
            }
        }

        //for (int i = 0; i < clipsQueue.Count; i++) //檢查音源衝突
        //{
        //    for (int j = i + 1; j < clipsQueue.Count; j++)
        //    {
        //        if(AudiosPack.Instance.di)
        //    }
        //}

        List<AudioSource> doneSourceList = new List<AudioSource>(); //經手音源(防止音源衝突)
        for (int i = 0; i < clipsQueue.Count; i++) //逐一撥放
        {
            if (clipsQueue[i].GetClips == null || clipsQueue[i].GetClips.Count == 0) //音訊剪輯為空或是無指定的狀況時跳過
            {
                Debug.Log("[ERROR]未指定Clip");
                continue;
            }

            if (clipsQueue[i].GetClips == null || clipsQueue[i].GetClips.Count == 0) //音訊剪輯為空或是無指定的狀況時跳過
            {
                Debug.Log("[ERROR]未指定Clip");
                continue;
            }

            if (!AudiosPack.Instance.audioSourceDict.ContainsKey(clipsQueue[i].GetOutputSourceNum)) //若音源編號找不到則跳過
            {
                Debug.Log("[ERROR]未指定音源");
                continue;
            }

            List<AudioSourceUnit> audioSourceQueue = AudiosPack.Instance.audioSourceDict[clipsQueue[i].GetOutputSourceNum]; //音源佇列
            for (int j = 0; j < audioSourceQueue.Count; j++)
            {
                if (doneSourceList.Count == 0 || !doneSourceList.Contains(audioSourceQueue[j].source))
                {
                    doneSourceList.Add(audioSourceQueue[j].source);
                    StartCoroutine(Corou_PlayAudioClip(clipsQueue[i].GetClips, audioSourceQueue[j], clipsQueue[i].GetVolumeScale, canCover));
                }
                else
                {
                    Debug.Log("[ERROR]音源名稱 " + audioSourceQueue[j].source.name + " 產生衝突");
                }
            }
        }
    }

    //覆蓋撥放
    //(多載1/2) name:名稱
    //(多載2/2) tag:標籤
    public void CoverPlayAudioClip(string name) { _playAudioClip(name, null, true); }
    public void CoverPlayAudioClip(string[] tag) { _playAudioClip(null, tag, true); }

    //停止撥放
    //(多載1/2) sourceNum:音源編號
    //(多載2/2) tag:音源標籤
    public void Stop(byte sourceNum) { _Stop(sourceNum, null); }
    public void Stop(string[] tag) { _Stop(255, tag); }
    private void _Stop(byte sourceNum, string[] tag)
    {
        List<AudioSourceUnit> sourceQueue = new List<AudioSourceUnit>();

        if (AudiosPack.Instance.audioSourceDict.ContainsKey(sourceNum) && sourceNum != 255) //從音源編號搜尋
        {
            for (int i = 0; i < AudiosPack.Instance.audioSourceDict[sourceNum].Count; i++)
            {
                sourceQueue.Add(AudiosPack.Instance.audioSourceDict[sourceNum][i]);
            }
        }

        if (tag != null && tag.Length > 0)
        {
            for (int i = 0; i < AudiosPack.Instance.audioSourcesPack.Count; i++) //從Tag搜尋
            {
                for (int j = 0; j < tag.Length; i++)
                {
                    for (int k = 0; k < AudiosPack.Instance.audioSourcesPack[i].GetTag.Count; k++)
                    {
                        if (tag[j] == AudiosPack.Instance.audioSourcesPack[i].GetTag[k])
                        {
                            sourceQueue.Add(AudiosPack.Instance.audioSourcesPack[i]);
                        }
                    }
                }
            }
        }

        //挑調重複的音源
        for (int i = 0; i < sourceQueue.Count; i++)
        {
            for (int j = i + 1; j < sourceQueue.Count; j++)
            {
                if (sourceQueue[i].source == sourceQueue[j].source) sourceQueue[j].source = null;
            }
        }
        sourceQueue.RemoveAll((AudioSourceUnit u) => { return u.source == null; });

        for (int i = 0; i < sourceQueue.Count; i++) //對音源佇列逐一發出停止命令
        {
            StartCoroutine(Corou_StopPlaying(sourceQueue[i]));
        }
    }

    //撥放音訊
    private IEnumerator Corou_PlayAudioClip(List<AudioClip> clips, AudioSourceUnit audioSourceUnit, float volumeScale, bool canCover)
    {
        if (audioSourceUnit.source == null)
        {
            Debug.Log("[ERROR]空的AudioSource");
            yield break;
        }

        if (audioSourceUnit.GetOutputMethod == OutputMethod.OneShot) //OneShot模式可以同時撥放複數Clip, 也不會有因為指定同個AudioSource而導致撥放衝突的問題
        {
            if (AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] == 2) yield break; //即使是OneShot, 若AudioSource狀態為2(停止中)則必須等完全停止才可撥放
            audioSourceUnit.source.pitch = 1.0f; //音調初始化
            audioSourceUnit.source.volume = AudiosPack.Instance.initialVolumeDict[audioSourceUnit.source]; //音量初始化
            for (int i = 0; i < clips.Count; i++)
            {
                audioSourceUnit.source.PlayOneShot(clips[i], volumeScale);
            }
            yield break;
        }

        if (clips.Count > 1) //Play模式不可撥放複數Clip
        {
            Debug.Log("[ERROR]不可使用複數Clip");
            yield break;
        }

        if (!canCover && AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] != 0) yield break; //音源尚未撥放完畢, 無法再度撥放
        if (canCover && AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] != 0) //覆蓋撥放
        {
            audioSourceUnit.source.Stop();
            yield return new WaitUntil(() => AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] == 0);
        }

        AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] = 1; //狀態=撥放中
        audioSourceUnit.source.pitch = 1.0f; //音調初始化
        audioSourceUnit.source.volume = AudiosPack.Instance.initialVolumeDict[audioSourceUnit.source]; //音量初始化
        audioSourceUnit.source.clip = clips[0];
        audioSourceUnit.source.volume = Mathf.Clamp(audioSourceUnit.source.volume * volumeScale, 0.0f, 1.0f); //音量調整

        audioSourceUnit.source.Play();

        float t = 0;
        float volumeLimit = audioSourceUnit.source.volume; //音量淡入時的目標音量

        float basePitch = new float(); //音調基數
        if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化) basePitch = 1.0f * (1.0f - audioSourceUnit.GetFastRate);
        if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化) basePitch = 1.0f * (1.0f + audioSourceUnit.GetLowerRate);

        while (t < audioSourceUnit.GetFadeDuration[0] && audioSourceUnit.source.isPlaying && audioSourceUnit.GetFadeInEffectMode != AudioEffectMode.無 && AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] != 2)
        {
            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音量淡化 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.volume = volumeLimit * (t / audioSourceUnit.GetFadeDuration[0]);
            }

            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化)
            {
                audioSourceUnit.source.pitch = basePitch + ((1.0f - basePitch) * (t / audioSourceUnit.GetFadeDuration[0]));
            }

            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.pitch = basePitch - ((basePitch - 1.0f) * (t / audioSourceUnit.GetFadeDuration[0]));
            }

            t += Time.deltaTime;
            if (t >= audioSourceUnit.GetFadeDuration[0]) //效果結束, 設定為目標值
            {
                audioSourceUnit.source.volume = volumeLimit;
                audioSourceUnit.source.pitch = 1.0f;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
        AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全停止
    }

    //停止撥放的特殊效果
    private IEnumerator Corou_StopPlaying(AudioSourceUnit audioSourceUnit)
    {
        if (!audioSourceUnit.source.isPlaying || AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] == 2) yield break; //若已經停止, 或是正在停止中, 則中止程序
        AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] = 2; //狀態=停止中

        AudioSource source = audioSourceUnit.source;

        if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.無) //無特殊效果則直接停止
        {
            source.Stop();
            yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
            AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全靜止
            yield break;
        }

        float t = 0;
        float iniVolume = audioSourceUnit.source.volume;
        float iniPitch = audioSourceUnit.source.pitch;
        while (t < audioSourceUnit.GetFadeDuration[1] && audioSourceUnit.source.isPlaying)
        {
            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音量淡化 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快且淡化 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.volume = iniVolume - (iniVolume * (t / audioSourceUnit.GetFadeDuration[1]));
            }

            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快且淡化)
            {
                audioSourceUnit.source.pitch = iniPitch + (((audioSourceUnit.GetFastRate + 1.0f) - iniPitch) * (t / audioSourceUnit.GetFadeDuration[1]));
            }

            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.pitch = iniPitch - ((iniPitch - (1.0f - (audioSourceUnit.GetLowerRate))) * (t / audioSourceUnit.GetFadeDuration[1]));
            }

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        source.Stop();
        yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
        AudiosPack.Instance.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全靜止
    }
}
