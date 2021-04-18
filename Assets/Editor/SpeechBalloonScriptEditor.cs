using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;

[CustomEditor(typeof(SpeechBalloonScript))]
[CanEditMultipleObjects]
public class SpeechBalloonScriptEditor : Editor
{
    SpeechBalloonScript s_target; //目標腳本

    void OnEnable()
    {
        s_target = (SpeechBalloonScript)target;
    }

    public override void OnInspectorGUI()
    {
        int titleLabelPadding = 80; //標題間隔
        int popupLength = 150; //選項條長度
        int subTitleLabelPadding = 65; //次標題間隔
        int propertyFieldLength = 240; //物件代入框長度
        GUIStyle sty_paramPlus = new GUIStyle("OL Plus") { margin = new RectOffset(20, 0, 1, -1) }; //面板開關(展開
        GUIStyle sty_paramMinus = new GUIStyle("OL Minus") { margin = new RectOffset(20, 0, 1, -1) }; //面板開關(縮回
        GUIStyle sty_insidePanel = new GUIStyle("ButtonRight") { overflow = new RectOffset(5, 0, 0, 0), margin = new RectOffset(0, 0, 5, 15), fixedWidth = 263 }; //詳細參數面板
        GUIStyle sty_miniLabel = new GUIStyle() { alignment = TextAnchor.MiddleLeft, fontSize = 10 }; //文字敘述(小)
        sty_miniLabel.normal.textColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        GUIStyle sty_focusLabel = new GUIStyle() { alignment = TextAnchor.MiddleLeft, fontSize = 11 }; //文字敘述(顏色改變)
        sty_focusLabel.normal.textColor = new Color(0.25f, 0.45f, 0.55f, 1f);

        if (s_target.foldStates.Length < 11) s_target.foldStates = new bool[11];
        this.serializedObject.Update();
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //對話框類型
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("對話框種類", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_mainType = (SpeechBalloonScript.Enum_mainType)EditorGUILayout.EnumPopup(s_target.sel_mainType, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    //等待視窗選項的初始化
                    s_target.isTxtCircle = false; //預設不啟用循環讀字
                    s_target.wait_textCircleSpeed = 0;
                    s_target.sel_waitingType = SpeechBalloonScript.Enum_waitingType.Off;
                    s_target.wait_rotateImg = null; //清除旋轉圖片
                    s_target.wait_circulateSlider = null; //清除循環進度條
                    s_target.wait_activeGameobject = null; //清除激活之物件
                    s_target.wait_circleSpeed = 0;
                    //選項設定的初始化
                    s_target.sel_waitingButtonActiveMode = SpeechBalloonScript.Enum_waitingButtonActiveMode.隱藏;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //前置事件
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("前置事件", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_preEvent = (SpeechBalloonScript.Enum_preEvent)EditorGUILayout.EnumPopup(s_target.sel_preEvent, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.preEvent = new UnityEvent();
                }
            }
            EditorGUI.BeginDisabledGroup(s_target.sel_preEvent == SpeechBalloonScript.Enum_preEvent.Off);
            {
                s_target.foldStates[0] = EditorGUILayout.Foldout(s_target.foldStates[0], "", s_target.foldStates[0] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[0])
        {
            switch (s_target.sel_preEvent)
            {
                case SpeechBalloonScript.Enum_preEvent.On:
                    EditorGUILayout.PropertyField(this.serializedObject.FindProperty("preEvent"), new GUIContent("Event"));
                    break;

                case SpeechBalloonScript.Enum_preEvent.Off:
                    s_target.foldStates[0] = false;
                    break;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //控件封鎖
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("控件封鎖", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_blockade = (SpeechBalloonScript.Enum_blockade)EditorGUILayout.EnumPopup(s_target.sel_blockade, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.blockade_img = new List<Image>();
                    s_target.blockade_cg = new CanvasGroup();
                }
            }
            EditorGUI.BeginDisabledGroup(s_target.sel_blockade == SpeechBalloonScript.Enum_blockade.Off);
            {
                s_target.foldStates[1] = EditorGUILayout.Foldout(s_target.foldStates[1], "", s_target.foldStates[1] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[1])
        {
            switch (s_target.sel_blockade)
            {
                case SpeechBalloonScript.Enum_blockade.CanvasGroup_blocksRaycasts:
                    EditorGUILayout.BeginVertical(sty_insidePanel);
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("blockade_cg"), new GUIContent(""), GUILayout.Width(propertyFieldLength));
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case SpeechBalloonScript.Enum_blockade.Image_raycastTarget:
                    EditorGUILayout.BeginVertical(sty_insidePanel);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("數量", GUILayout.Width(45));
                            EditorGUILayout.LabelField(( s_target.blockade_img.Count ).ToString(), new GUIStyle("AssetLabel") { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(60));
                            if (GUILayout.Button("", new GUIStyle("OL Plus") { margin = new RectOffset(15, 0, 1, -1) }))
                            {
                                s_target.blockade_img.Add(null);
                            }
                            if (GUILayout.Button("", new GUIStyle("OL Minus") { padding = new RectOffset(200, 0, 0, 0), margin = new RectOffset(0, 0, 1, -1) }))
                            {
                                if (s_target.blockade_img.Count > 0) s_target.blockade_img.RemoveAt(s_target.blockade_img.Count - 1);
                            }
                            this.serializedObject.ApplyModifiedProperties();
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < s_target.blockade_img.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Image ", GUILayout.Width(subTitleLabelPadding));
                                this.serializedObject.Update();
                                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("blockade_img").GetArrayElementAtIndex(i), new GUIContent(""), GUILayout.Width(170));
                                this.serializedObject.ApplyModifiedProperties();

                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case SpeechBalloonScript.Enum_blockade.Off:
                    s_target.foldStates[1] = false;
                    break;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //布幕淡入
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("布幕淡入", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_curtainFadeIn = (SpeechBalloonScript.Enum_curtainFadeIn)EditorGUILayout.EnumPopup(s_target.sel_curtainFadeIn, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.curtain_img = null;
                    s_target.curtainFadeIn_transColors = new Color[2];
                    s_target.curtainFadeIn_trasDuration = new Vector2(0, 0);
                    s_target.curtain_cg = null;
                    s_target.curtainFadeIn_tranStates = new Vector2(0, 0);

                    s_target.curtainFadeOut_trasDuration = new Vector2(0, 0);
                    s_target.curtainFadeOut_tranStates = new Vector2(0, 0);
                    s_target.curtainFadeOut_transColors = new Color[2];
                }
            }
            EditorGUI.BeginDisabledGroup(s_target.sel_curtainFadeIn == SpeechBalloonScript.Enum_curtainFadeIn.Off);
            {
                s_target.foldStates[2] = EditorGUILayout.Foldout(s_target.foldStates[2], "", s_target.foldStates[2] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[2])
        {
            switch (s_target.sel_curtainFadeIn)
            {
                case SpeechBalloonScript.Enum_curtainFadeIn.CanvasGroup_alpha:
                    EditorGUILayout.BeginVertical(sty_insidePanel);
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("curtain_cg"), new GUIContent(""), GUILayout.Width(propertyFieldLength));

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("透明度變化", GUILayout.Width(subTitleLabelPadding));
                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeIn_tranStates.x = EditorGUILayout.FloatField(s_target.curtainFadeIn_tranStates.x, GUILayout.Width(35));
                            EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeIn_tranStates.y = EditorGUILayout.FloatField(s_target.curtainFadeIn_tranStates.y, GUILayout.Width(35));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            //類型為SystemMessage時, 所影響的標題字色會改變
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeIn_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeIn_trasDuration.x = EditorGUILayout.FloatField(s_target.curtainFadeIn_trasDuration.x, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeIn_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeIn_trasDuration.y = EditorGUILayout.FloatField(s_target.curtainFadeIn_trasDuration.y, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                            if (s_target.curtainFadeIn_trasDuration.y < s_target.curtainFadeIn_trasDuration.x) s_target.curtainFadeIn_trasDuration.y = s_target.curtainFadeIn_trasDuration.x; //作動期間校驗
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case SpeechBalloonScript.Enum_curtainFadeIn.Image_Color:
                    EditorGUILayout.BeginVertical(sty_insidePanel);
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("curtain_img"), new GUIContent(""), GUILayout.Width(propertyFieldLength));

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("色彩變化", GUILayout.Width(subTitleLabelPadding));
                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeIn_transColors[0] = EditorGUILayout.ColorField(s_target.curtainFadeIn_transColors[0], GUILayout.Width(54));
                            EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(1));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeIn_transColors[1] = EditorGUILayout.ColorField(s_target.curtainFadeIn_transColors[1], GUILayout.Width(54));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            //類型為SystemMessage時, 所影響的標題字色會改變
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeIn_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeIn_trasDuration.x = EditorGUILayout.FloatField(s_target.curtainFadeIn_trasDuration.x, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeIn_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeIn_trasDuration.y = EditorGUILayout.FloatField(s_target.curtainFadeIn_trasDuration.y, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                            if (s_target.curtainFadeIn_trasDuration.y < s_target.curtainFadeIn_trasDuration.x) s_target.curtainFadeIn_trasDuration.y = s_target.curtainFadeIn_trasDuration.x; //作動期間校驗
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case SpeechBalloonScript.Enum_curtainFadeIn.Off:
                    s_target.foldStates[2] = false;
                    break;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //對話框淡入
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("對話框淡入", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_balloonFadeIn = (SpeechBalloonScript.Enum_balloonFadeIn)EditorGUILayout.EnumPopup(s_target.sel_balloonFadeIn, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.balloon_img = null;
                    s_target.balloon_transColors = new Color[2];
                    s_target.balloon_trasDuration = new Vector2(0, 0);
                }
            }
            EditorGUI.BeginDisabledGroup(s_target.sel_balloonFadeIn == SpeechBalloonScript.Enum_balloonFadeIn.Off);
            {
                s_target.foldStates[3] = EditorGUILayout.Foldout(s_target.foldStates[3], "", s_target.foldStates[3] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[3])
        {
            switch (s_target.sel_balloonFadeIn)
            {
                case SpeechBalloonScript.Enum_balloonFadeIn.On:
                    EditorGUILayout.BeginVertical(sty_insidePanel);
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("balloon_img"), new GUIContent(""), GUILayout.Width(propertyFieldLength));

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("色彩變化", GUILayout.Width(subTitleLabelPadding));
                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                            s_target.balloon_transColors[0] = EditorGUILayout.ColorField(s_target.balloon_transColors[0], GUILayout.Width(54));
                            EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(1));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                            s_target.balloon_transColors[1] = EditorGUILayout.ColorField(s_target.balloon_transColors[1], GUILayout.Width(54));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            //類型為SystemMessage時, 所影響的標題字色會改變
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.balloon_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.balloon_trasDuration.x = EditorGUILayout.FloatField(s_target.balloon_trasDuration.x, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.balloon_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.balloon_trasDuration.y = EditorGUILayout.FloatField(s_target.balloon_trasDuration.y, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                            if (s_target.balloon_trasDuration.y < s_target.balloon_trasDuration.x) s_target.balloon_trasDuration.y = s_target.balloon_trasDuration.x; //作動期間校驗
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case SpeechBalloonScript.Enum_balloonFadeIn.Off:
                    s_target.foldStates[3] = false;
                    break;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //等待視窗選項
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("等待視窗選項", sty_focusLabel, GUILayout.Width(titleLabelPadding));
                EditorGUI.BeginChangeCheck();
                {
                    s_target.sel_waitingType = (SpeechBalloonScript.Enum_waitingType)EditorGUILayout.EnumPopup(s_target.sel_waitingType, GUILayout.Width(popupLength));
                    if (EditorGUI.EndChangeCheck())
                    {
                        //初始化參數
                        s_target.wait_rotateImg = null; //清除旋轉圖片
                        s_target.wait_circulateSlider = null; //清除循環進度條
                        s_target.wait_activeGameobject = null; //清除激活之物件
                    }
                }
                s_target.foldStates[10] = EditorGUILayout.Foldout(s_target.foldStates[10], "", s_target.foldStates[10] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUILayout.EndHorizontal();
            if (s_target.foldStates[10])
            {
                EditorGUILayout.BeginVertical(sty_insidePanel);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("循環讀字", GUILayout.Width(subTitleLabelPadding));
                        s_target.isTxtCircle = EditorGUILayout.Toggle(s_target.isTxtCircle, GUILayout.Width(30));
                        if (s_target.isTxtCircle)
                        {
                            EditorGUILayout.LabelField("循環速度速度", GUILayout.Width(subTitleLabelPadding + 5));
                            s_target.wait_textCircleSpeed = EditorGUILayout.FloatField(s_target.wait_textCircleSpeed, GUILayout.Width(45));
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (s_target.sel_waitingType != SpeechBalloonScript.Enum_waitingType.Off && s_target.sel_waitingType != SpeechBalloonScript.Enum_waitingType.激活物件)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string lb = "";
                            if (s_target.sel_waitingType == SpeechBalloonScript.Enum_waitingType.圖片旋轉) lb = "旋轉速度";
                            if (s_target.sel_waitingType == SpeechBalloonScript.Enum_waitingType.進度條循環) lb = "進度條速度";
                            EditorGUILayout.LabelField(lb, GUILayout.Width(subTitleLabelPadding));
                            s_target.wait_circleSpeed = EditorGUILayout.FloatField(s_target.wait_circleSpeed, GUILayout.Width(45));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    switch (s_target.sel_waitingType)
                    {
                        case SpeechBalloonScript.Enum_waitingType.圖片旋轉:
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("圖片物件", GUILayout.Width(subTitleLabelPadding));
                                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("wait_rotateImg"), new GUIContent(""), GUILayout.Width(170));
                            }
                            EditorGUILayout.EndHorizontal();
                            break;

                        case SpeechBalloonScript.Enum_waitingType.進度條循環:
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("進度條物件", GUILayout.Width(subTitleLabelPadding));
                                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("wait_circulateSlider"), new GUIContent(""), GUILayout.Width(170));
                            }
                            EditorGUILayout.EndHorizontal();
                            break;

                        case SpeechBalloonScript.Enum_waitingType.激活物件:
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("目標物件", GUILayout.Width(subTitleLabelPadding));
                                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("wait_activeGameobject"), new GUIContent(""), GUILayout.Width(170));
                            }
                            EditorGUILayout.EndHorizontal();
                            break;
                    }

                }
                EditorGUILayout.EndVertical();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //對話框文字顯示
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("對話框文字顯示", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_textView = (SpeechBalloonScript.Enum_textView)EditorGUILayout.EnumPopup(s_target.sel_textView, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.sel_continueSpeechKey = SpeechBalloonScript.Enum_continueSpeechKey.任意;
                    s_target.sel_key = KeyCode.None;
                    s_target.textView_cursor = null;
                    s_target.textView_readSpeed = 0;
                }
            }
            s_target.foldStates[4] = EditorGUILayout.Foldout(s_target.foldStates[4], "", s_target.foldStates[4] ? sty_paramMinus : sty_paramPlus);
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[4])
        {
            EditorGUILayout.BeginVertical(sty_insidePanel);
            {
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("textView_text"), new GUIContent(""), GUILayout.Width(propertyFieldLength));

                if (s_target.sel_textView == SpeechBalloonScript.Enum_textView.讀字)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("閃爍標", GUILayout.Width(subTitleLabelPadding));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("textView_cursor"), new GUIContent(""), GUILayout.Width(170));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("讀字速度", GUILayout.Width(subTitleLabelPadding));
                        s_target.textView_readSpeed = EditorGUILayout.FloatField(s_target.textView_readSpeed, GUILayout.Width(45));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("繼續對話鍵", GUILayout.Width(subTitleLabelPadding));
                        s_target.sel_continueSpeechKey = (SpeechBalloonScript.Enum_continueSpeechKey)EditorGUILayout.EnumPopup(s_target.sel_continueSpeechKey, GUILayout.Width(45));
                        if (s_target.sel_continueSpeechKey == SpeechBalloonScript.Enum_continueSpeechKey.指定)
                        {
                            s_target.sel_key = (KeyCode)EditorGUILayout.EnumPopup(s_target.sel_key, GUILayout.Width(45));
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

            }
            EditorGUILayout.EndVertical();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //選項淡入動畫
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            //類型為SystemMessage時, 所影響的標題字色會改變
            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("選項淡入動畫", GUILayout.Width(titleLabelPadding));
            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("選項淡入動畫", sty_focusLabel, GUILayout.Width(titleLabelPadding));

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                {
                    if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.sel_selectionFadeInAnimation = SpeechBalloonScript.Enum_selectionFadeInAnimation.無動畫;
                    s_target.sel_selectionFadeInAnimation = (SpeechBalloonScript.Enum_selectionFadeInAnimation)EditorGUILayout.EnumPopup(s_target.sel_selectionFadeInAnimation, GUILayout.Width(popupLength));
                }
                EditorGUI.EndDisabledGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數

                }
            }
            s_target.foldStates[8] = EditorGUILayout.Foldout(s_target.foldStates[8], "", s_target.foldStates[8] ? sty_paramMinus : sty_paramPlus);
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[8])
        {
            EditorGUILayout.BeginVertical(sty_insidePanel);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("選項預置體", GUILayout.Width(subTitleLabelPadding));
                    EditorGUILayout.PropertyField(this.serializedObject.FindProperty("seletion_prefab"), new GUIContent(""), GUILayout.Width(170));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("選項父區域", GUILayout.Width(subTitleLabelPadding));
                    EditorGUILayout.PropertyField(this.serializedObject.FindProperty("selection_holder"), new GUIContent(""), GUILayout.Width(170));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("間隔", GUILayout.Width(subTitleLabelPadding));
                    EditorGUILayout.LabelField("X軸", sty_miniLabel, GUILayout.Width(28));
                    s_target.selection_padding.x = EditorGUILayout.FloatField(s_target.selection_padding.x, GUILayout.Width(35));
                    EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Y軸", sty_miniLabel, GUILayout.Width(28));
                    s_target.selection_padding.y = EditorGUILayout.FloatField(s_target.selection_padding.y, GUILayout.Width(35));
                }
                EditorGUILayout.EndHorizontal();

                if (s_target.sel_selectionFadeInAnimation != SpeechBalloonScript.Enum_selectionFadeInAnimation.無動畫)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("特效時間", GUILayout.Width(subTitleLabelPadding));
                        s_target.selectionFadeIn_costTime = EditorGUILayout.FloatField(s_target.selectionFadeIn_costTime, GUILayout.Width(45));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (s_target.selectionFadeIn_costTime != 0) s_target.selectionFadeIn_costTime = 0;

                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("按鈕模式", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));
                        s_target.sel_waitingButtonActiveMode = (SpeechBalloonScript.Enum_waitingButtonActiveMode)EditorGUILayout.EnumPopup(s_target.sel_waitingButtonActiveMode, GUILayout.Width(170));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //選項選擇後
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("事件發生延遲", GUILayout.Width(titleLabelPadding));
            EditorGUILayout.LabelField("選擇選項後", sty_miniLabel, GUILayout.Width(55));
            s_target.afterSelect_delay = EditorGUILayout.FloatField(s_target.afterSelect_delay, GUILayout.Width(40));
            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(10));
        }
        EditorGUILayout.EndHorizontal();

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //選項淡出動畫
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            //類型為SystemMessage時, 所影響的標題字色會改變
            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("選項淡出動畫", GUILayout.Width(titleLabelPadding));
            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("選項淡出動畫", sty_focusLabel, GUILayout.Width(titleLabelPadding));

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                {
                    if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.sel_selectionFadeOutAnimation = SpeechBalloonScript.Enum_selectionFadeOutAnimation.無動畫;
                    s_target.sel_selectionFadeOutAnimation = (SpeechBalloonScript.Enum_selectionFadeOutAnimation)EditorGUILayout.EnumPopup(s_target.sel_selectionFadeOutAnimation, GUILayout.Width(popupLength));
                }
                EditorGUI.EndDisabledGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數

                }
            }
        }
        EditorGUILayout.EndHorizontal();

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //對話框淡出
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginDisabledGroup(s_target.sel_balloonFadeIn != SpeechBalloonScript.Enum_balloonFadeIn.On);
            {
                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue)
                {
                    EditorGUILayout.LabelField("對話框淡出", GUILayout.Width(titleLabelPadding));
                    EditorGUILayout.LabelField("選項選擇後的 " + s_target.balloonFadeOut_trasDuration.x + " 秒 ~ " + s_target.balloonFadeOut_trasDuration.y + " 秒", sty_miniLabel, GUILayout.Width(popupLength));
                }

                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage)
                {
                    EditorGUILayout.LabelField("對話框淡出", sty_focusLabel, GUILayout.Width(titleLabelPadding));
                    EditorGUILayout.LabelField("訊息視窗對話框固定為瞬間淡出", sty_miniLabel, GUILayout.Width(popupLength));
                }

                s_target.foldStates[5] = EditorGUILayout.Foldout(s_target.foldStates[5], "", s_target.foldStates[5] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.sel_balloonFadeIn != SpeechBalloonScript.Enum_balloonFadeIn.On)
        {
            //初始化參數
            s_target.foldStates[5] = false;
            s_target.balloonFadeOut_transColors = new Color[2];
            s_target.balloonFadeOut_trasDuration = new Vector2(0, 0);
        }
        if (s_target.foldStates[5])
        {
            EditorGUILayout.BeginVertical(sty_insidePanel);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("色彩變化", GUILayout.Width(subTitleLabelPadding));
                    EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                    s_target.balloonFadeOut_transColors[0] = EditorGUILayout.ColorField(s_target.balloonFadeOut_transColors[0], GUILayout.Width(54));
                    EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(1));
                    EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                    s_target.balloonFadeOut_transColors[1] = EditorGUILayout.ColorField(s_target.balloonFadeOut_transColors[1], GUILayout.Width(54));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    //類型為SystemMessage時, 所影響的標題字色會改變
                    if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                    if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                    EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                    EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                    {
                        if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.balloonFadeOut_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                        s_target.balloonFadeOut_trasDuration.x = EditorGUILayout.FloatField(s_target.balloonFadeOut_trasDuration.x, GUILayout.Width(35));
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                    EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                    EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                    {
                        if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.balloonFadeOut_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                        s_target.balloonFadeOut_trasDuration.y = EditorGUILayout.FloatField(s_target.balloonFadeOut_trasDuration.y, GUILayout.Width(35));
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                    if (s_target.balloonFadeOut_trasDuration.y < s_target.balloonFadeOut_trasDuration.x) s_target.balloonFadeOut_trasDuration.y = s_target.balloonFadeOut_trasDuration.x; //作動期間校驗
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //布幕淡出
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginDisabledGroup(s_target.sel_curtainFadeIn == SpeechBalloonScript.Enum_curtainFadeIn.Off);
            {
                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue)
                {
                    EditorGUILayout.LabelField("布幕淡出", GUILayout.Width(titleLabelPadding));
                    EditorGUILayout.LabelField("選項選擇後的 " + s_target.curtainFadeOut_trasDuration.x + " 秒 ~ " + s_target.curtainFadeOut_trasDuration.y + " 秒", sty_miniLabel, GUILayout.Width(popupLength));
                }

                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage)
                {
                    EditorGUILayout.LabelField("布幕淡出", sty_focusLabel, GUILayout.Width(titleLabelPadding));
                    EditorGUILayout.LabelField("訊息視窗布幕固定為瞬間淡出", sty_miniLabel, GUILayout.Width(popupLength));
                }

                s_target.foldStates[6] = EditorGUILayout.Foldout(s_target.foldStates[6], "", s_target.foldStates[6] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.sel_curtainFadeIn == SpeechBalloonScript.Enum_curtainFadeIn.Off) { s_target.foldStates[6] = false; }
        if (s_target.foldStates[6])
        {
            EditorGUILayout.BeginVertical(sty_insidePanel);
            {
                switch (s_target.sel_curtainFadeIn)
                {
                    case SpeechBalloonScript.Enum_curtainFadeIn.CanvasGroup_alpha:
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("透明度變化", GUILayout.Width(subTitleLabelPadding));
                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeOut_tranStates.x = EditorGUILayout.FloatField(s_target.curtainFadeOut_tranStates.x, GUILayout.Width(35));
                            EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeOut_tranStates.y = EditorGUILayout.FloatField(s_target.curtainFadeOut_tranStates.y, GUILayout.Width(35));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            //類型為SystemMessage時, 所影響的標題字色會改變
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeOut_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeOut_trasDuration.x = EditorGUILayout.FloatField(s_target.curtainFadeOut_trasDuration.x, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeOut_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeOut_trasDuration.y = EditorGUILayout.FloatField(s_target.curtainFadeOut_trasDuration.y, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                            if (s_target.curtainFadeOut_trasDuration.y < s_target.curtainFadeOut_trasDuration.x) s_target.curtainFadeOut_trasDuration.y = s_target.curtainFadeOut_trasDuration.x; //作動期間校驗
                        }
                        EditorGUILayout.EndHorizontal();
                        break;

                    case SpeechBalloonScript.Enum_curtainFadeIn.Image_Color:
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("色彩變化", GUILayout.Width(subTitleLabelPadding));
                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeOut_transColors[0] = EditorGUILayout.ColorField(s_target.curtainFadeOut_transColors[0], GUILayout.Width(54));
                            EditorGUILayout.LabelField("", sty_miniLabel, GUILayout.Width(1));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));
                            s_target.curtainFadeOut_transColors[1] = EditorGUILayout.ColorField(s_target.curtainFadeOut_transColors[1], GUILayout.Width(54));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            //類型為SystemMessage時, 所影響的標題字色會改變
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.Dialogue) EditorGUILayout.LabelField("作動期間", GUILayout.Width(subTitleLabelPadding));
                            if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) EditorGUILayout.LabelField("作動期間", sty_focusLabel, GUILayout.Width(subTitleLabelPadding));

                            EditorGUILayout.LabelField("開始於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeOut_trasDuration.x = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeOut_trasDuration.x = EditorGUILayout.FloatField(s_target.curtainFadeOut_trasDuration.x, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));
                            EditorGUILayout.LabelField("結束於", sty_miniLabel, GUILayout.Width(28));

                            EditorGUI.BeginDisabledGroup(s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage);
                            {
                                if (s_target.sel_mainType == SpeechBalloonScript.Enum_mainType.SystemMessage) s_target.curtainFadeOut_trasDuration.y = 0; //若類型為SystemMessage時作動期間固定為0
                                s_target.curtainFadeOut_trasDuration.y = EditorGUILayout.FloatField(s_target.curtainFadeOut_trasDuration.y, GUILayout.Width(35));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField("秒", sty_miniLabel, GUILayout.Width(20));

                            if (s_target.curtainFadeOut_trasDuration.y < s_target.curtainFadeOut_trasDuration.x) s_target.curtainFadeOut_trasDuration.y = s_target.curtainFadeOut_trasDuration.x; //作動期間校驗
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                }
            }
            EditorGUILayout.EndVertical();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //後置事件
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("後置事件", GUILayout.Width(titleLabelPadding));
            EditorGUI.BeginChangeCheck();
            {
                s_target.sel_endingEvent = (SpeechBalloonScript.Enum_endingEvent)EditorGUILayout.EnumPopup(s_target.sel_endingEvent, GUILayout.Width(popupLength));
                if (EditorGUI.EndChangeCheck())
                {
                    //初始化參數
                    s_target.endingEvent = new UnityEvent();
                }
            }
            EditorGUI.BeginDisabledGroup(s_target.sel_endingEvent == SpeechBalloonScript.Enum_endingEvent.Off);
            {
                s_target.foldStates[7] = EditorGUILayout.Foldout(s_target.foldStates[7], "", s_target.foldStates[7] ? sty_paramMinus : sty_paramPlus);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[7])
        {
            switch (s_target.sel_endingEvent)
            {
                case SpeechBalloonScript.Enum_endingEvent.On:
                    EditorGUILayout.PropertyField(this.serializedObject.FindProperty("endingEvent"), new GUIContent("Event"));
                    break;

                case SpeechBalloonScript.Enum_endingEvent.Off:
                    s_target.foldStates[7] = false;
                    break;
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------
        //音效管理
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("音效管理", GUILayout.Width(titleLabelPadding));
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("audioListener"), new GUIContent(""), GUILayout.Width(popupLength));
            s_target.foldStates[9] = EditorGUILayout.Foldout(s_target.foldStates[9], "", s_target.foldStates[9] ? sty_paramMinus : sty_paramPlus);
        }
        EditorGUILayout.EndHorizontal();
        if (s_target.foldStates[9])
        {
            EditorGUILayout.BeginVertical(sty_insidePanel);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        s_target.se_UseCustomAudioSource = EditorGUILayout.Toggle(s_target.se_UseCustomAudioSource, GUILayout.Width(16));
                        if (EditorGUI.EndChangeCheck())
                        {
                            //初始化參數
                            s_target.se_AudioSoucre = null;
                        }
                    }

                    EditorGUILayout.LabelField("是否自訂音源", GUILayout.Width(titleLabelPadding));
                    if (s_target.se_UseCustomAudioSource) EditorGUILayout.PropertyField(this.serializedObject.FindProperty("se_AudioSoucre"), new GUIContent(""));

                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("數量", GUILayout.Width(45));
                    EditorGUILayout.LabelField(( s_target.se_SoundTimings.Count ).ToString(), new GUIStyle("AssetLabel") { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(60));
                    if (GUILayout.Button("", new GUIStyle("OL Plus") { margin = new RectOffset(15, 0, 1, -1) }))
                    {
                        s_target.se_SoundTimings.Add(SpeechBalloonScript.Enum_soundTiming.呼叫對話框時);
                        s_target.se_clips.Add(null);
                        s_target.se_volume.Add(1);
                    }
                    if (GUILayout.Button("", new GUIStyle("OL Minus") { padding = new RectOffset(200, 0, 0, 0), margin = new RectOffset(0, 0, 1, -1) }))
                    {
                        if (s_target.se_SoundTimings.Count > 0) s_target.se_SoundTimings.RemoveAt(s_target.se_SoundTimings.Count - 1);
                        if (s_target.se_clips.Count > 0) s_target.se_clips.RemoveAt(s_target.se_clips.Count - 1);
                        if (s_target.se_volume.Count > 0) s_target.se_volume.RemoveAt(s_target.se_volume.Count - 1);
                    }
                }
                this.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();

                this.serializedObject.Update();
                for (int i = 0; i < s_target.se_SoundTimings.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Sound " + i, GUILayout.Width(55));
                        s_target.se_SoundTimings[i] = (SpeechBalloonScript.Enum_soundTiming)EditorGUILayout.EnumPopup(s_target.se_SoundTimings[i], GUILayout.Width(85));
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("se_clips").GetArrayElementAtIndex(i), new GUIContent(""), GUILayout.Width(100));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("音量", GUILayout.Width(55));
                        s_target.se_volume[i] = EditorGUILayout.Slider(s_target.se_volume[i], 0, 1, GUILayout.Width(180));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                this.serializedObject.ApplyModifiedProperties();

            }
            EditorGUILayout.EndVertical();
        }

        Undo.RecordObject(s_target, "SpeechBalloonScript");
        this.serializedObject.ApplyModifiedProperties();
    }

}
