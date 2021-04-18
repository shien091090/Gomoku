using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[CustomEditor(typeof(AnimationScript))]
[CanEditMultipleObjects]
public class AnimationScriptEditor : Editor
{
    AnimationScript a_target;
    bool[] foldoutState; //摺疊夾收放狀態
    int[] indexArray; //索引數組
    string[] indexContents; //索引字串集合

    private void OnEnable()
    {
        a_target = (AnimationScript)target;
        if (foldoutState == null) SetIndexContent(a_target.eventStateCount);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(new GUIContent("IsPlaying : " + (a_target.animationPlaying ? "Playing" : "Standby"))); //是否撥放中

        a_target.playingLock = EditorGUILayout.Toggle("PlayingLock", a_target.playingLock); //動畫鎖(是否允許在動畫撥放期間呼叫撥放動畫方法)

        a_target.eventStateCount = EditorGUILayout.IntField("Event State Count", a_target.eventStateCount); //事件組數量
        if (a_target.eventStateCount != a_target.eventFlowFactory_before.Length || a_target.eventStateCount != a_target.eventFlowFactory_after.Length) SetIndexContent(a_target.eventStateCount);
        if (a_target.eventStateCount > 0) a_target.defaultEventIndex = EditorGUILayout.IntPopup("Default Index", Mathf.Clamp(a_target.defaultEventIndex, 1, indexArray.Length), indexContents, indexArray); //預設索引

        for (int i = 0; i < a_target.eventStateCount; i++)
        {
            foldoutState[i] = EditorGUILayout.Foldout(foldoutState[i], "State " + (i + 1)); //事件組摺疊夾
            if (foldoutState[i])
            {
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("eventFlowFactory_before").GetArrayElementAtIndex(i), new GUIContent("Before ")); //撥放前事件
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("eventFlowFactory_after").GetArrayElementAtIndex(i), new GUIContent("After ")); //撥放後事件
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    //設定索引內容
    public void SetIndexContent(int count)
    {
        indexArray = new int[count];
        indexContents = new string[count];
        if (foldoutState == null) foldoutState = new bool[0];
        bool[] _foldoutState = new bool[count]; //設定暫時變數, 避免直接new新陣列導致原本變數狀態遭到破壞
        PassAnimation[] _beforeEvents = new PassAnimation[count]; //同上
        UnityEvent[] _afterEvents = new UnityEvent[count]; //同上

        for (byte i = 0; i < count; i++)
        {
            indexArray[i] = i + 1;
            indexContents[i] = "State " + (i + 1).ToString();
            if (foldoutState.Length > i) _foldoutState[i] = _foldoutState[i] = foldoutState[i];
            if (a_target.eventFlowFactory_before.Length > i) _beforeEvents[i] = a_target.eventFlowFactory_before[i];
            if (a_target.eventFlowFactory_after.Length > i) _afterEvents[i] = a_target.eventFlowFactory_after[i];
        }

        foldoutState = _foldoutState;
        a_target.eventFlowFactory_before = _beforeEvents;
        a_target.eventFlowFactory_after = _afterEvents;
    }

}
