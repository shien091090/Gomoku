//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

////[CustomEditor(typeof(AudiosPack))]
////[CanEditMultipleObjects]
//public class AudiosPackEditor : Editor
//{
//    AudiosPack a_target; //目標腳本

//    void OnEnable()
//    {
//        a_target = (AudiosPack)target;
//    }

//    public override void OnInspectorGUI()
//    {
//        int titleLabelPadding = 80; //標題間隔
//        int subTitleLabelPadding = 65; //次標題間隔
//        GUIStyle sty_insidePanel = new GUIStyle("ButtonRight") { overflow = new RectOffset(5, 0, 0, 0), margin = new RectOffset(0, 0, 5, 15), fixedWidth = 263 }; //詳細參數面板

//        //ClipsPack
//        EditorGUILayout.BeginVertical(sty_insidePanel);
//        {
//            EditorGUILayout.BeginHorizontal();
//            {
//                EditorGUILayout.LabelField("數量", GUILayout.Width(45));
//                EditorGUILayout.LabelField((a_target.GetClipsPack.Count).ToString(), new GUIStyle("AssetLabel") { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(60));
//                if (GUILayout.Button("", new GUIStyle("OL Plus") { margin = new RectOffset(15, 0, 1, -1) }))
//                {
//                    a_target.GetClipsPack.Add(new ClipUnit());
//                }
//                if (GUILayout.Button("", new GUIStyle("OL Minus") { padding = new RectOffset(200, 0, 0, 0), margin = new RectOffset(0, 0, 1, -1) }))
//                {
//                    if (a_target.GetClipsPack.Count > 0) a_target.GetClipsPack.RemoveAt(a_target.GetClipsPack.Count - 1);
//                }
//                this.serializedObject.ApplyModifiedProperties();
//            }
//            EditorGUILayout.EndHorizontal();

//            for (int i = 0; i < a_target.GetClipsPack.Count; i++)
//            {
//                EditorGUILayout.BeginHorizontal();
//                {
//                    EditorGUILayout.LabelField("AudioClip", GUILayout.Width(subTitleLabelPadding));
//                    this.serializedObject.Update();

//                    //var clipunit = this.serializedObject.FindProperty("clipsPack").GetArrayElementAtIndex(i);
//                    //Debug.Log("GetElement:" + (ClipUnit)clipunit);
//                    EditorGUILayout.PropertyField(this.serializedObject.FindProperty("clipsPack").GetArrayElementAtIndex(i).FindPropertyRelative("Clips"), new GUIContent(""), GUILayout.Width(170));
//                    this.serializedObject.ApplyModifiedProperties();

//                }
//                EditorGUILayout.EndHorizontal();
//            }
//        }
//        EditorGUILayout.EndVertical();
//    }
//}
