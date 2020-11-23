﻿using System.IO;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BTEditorWindow : EditorWindow
	{
		private const int SPACE_VALUE = 10;
		private const string DEFAULE_BT_NAME = "新建行为树";

		private int mCurSelectJson = 0;
		private int mLastSelectJson = 0;
		private string[] mAllShowJsons;

		private string mKey = string.Empty;
		private string mValue = string.Empty;
		private string mDelKey = null;
		private Dictionary<string, string> mChangeDict = new Dictionary<string, string> ();

		private string mLastNodeGuid = string.Empty;

		void DrawNodeInspector ()
		{
			GUILayout.Space (SPACE_VALUE);
			EditorGUILayout.HelpBox (string.Format ("编辑行为树:{0}", mBehaviourTree.Name), MessageType.Info, true);
			GUILayout.Space (SPACE_VALUE);
			if (GUILayout.Button ("加载行为树")) {
				LoadBehaviorTree ();
			}

			if (mAllShowJsons != null && mAllShowJsons.Length > 0 && mLastSelectJson != mCurSelectJson) {
				mLastSelectJson = mCurSelectJson;
				var fileName = mAllShowJsons [mCurSelectJson];
				var file = Path.Combine (BTHelper.jsonPath, string.Format ("{0}.json", fileName));
				mBehaviourTree = BTHelper.LoadBehaviorTree (file);
				if (mBehaviourTree == null) {
					Debug.LogErrorFormat ("读取行为树失败, {0}", file);
					mBehaviourTree = new BehaviourTree (DEFAULE_BT_NAME);
				}
			}

			if (GUILayout.Button (DEFAULE_BT_NAME)) {
				mBehaviourTree = new BehaviourTree (DEFAULE_BT_NAME);
			}

			if (GUILayout.Button ("保存行为树")) {
				BTHelper.SaveBTData (mBehaviourTree);
			}

			EditorGUIUtility.labelWidth = 40;
			if (mAllShowJsons != null && mAllShowJsons.Length > 0)
				mCurSelectJson = EditorGUILayout.Popup ("行为树:", mCurSelectJson, mAllShowJsons);
			EditorGUIUtility.labelWidth = 60;

			GUILayout.Space (SPACE_VALUE);

			EditorGUILayout.LabelField ("节点数据");
			var node = window.CurSelectNode;
			if (node != null) {
				var data = node.Data;
				EditorGUILayout.LabelField ("节点名:", data.name);
				if (node.Guid != mLastNodeGuid) {
					mLastNodeGuid = node.Guid;
					mChangeDict.Clear ();
				}

				DrawDataInspector (data);
			}
		}

		void LoadBehaviorTree ()
		{
			var files = FileHelper.GetAllFiles (BTHelper.jsonPath, "json");
			var fileNames = new List<string> ();
			foreach (var file in files) {
				fileNames.Add (Path.GetFileNameWithoutExtension (file));
			}

			mAllShowJsons = fileNames.ToArray ();
			mLastSelectJson = -1;
		}

		void DrawDataInspector (BTNodeData data)
		{
			EditorGUIUtility.labelWidth = 24;

			if (!string.IsNullOrEmpty (mDelKey)) {
				data.RemoveData (mDelKey);
				if (mChangeDict.ContainsKey (mDelKey))
					mChangeDict.Remove (mDelKey);
				mDelKey = null;
			}

			foreach (var key in mChangeDict.Keys) {
				data.AddData (key, mChangeDict [key]);
			}

			mChangeDict.Clear ();

			if (data.data != null) {
				foreach (var kv in data.data) {
					DrawItemInspector (kv);
				}
			}

			EditorGUILayout.BeginHorizontal ();
			{
				mKey = EditorGUILayout.TextField ("key:", mKey);
				mValue = EditorGUILayout.TextField ("value:", mValue, GUILayout.MaxWidth (80));
				if (GUILayout.Button ("+", GUILayout.MaxWidth (20))) {
					if (!string.IsNullOrEmpty (mKey)) {
						data.AddData (mKey, mValue);
						mKey = "";
						mValue = "";
					}
				}
			}
			EditorGUILayout.EndHorizontal ();
		}

		void DrawItemInspector (KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.LabelField ("key:", kv.Key);
				mChangeDict [kv.Key] = EditorGUILayout.TextField ("val:", kv.Value, GUILayout.MaxWidth (80));
				if (GUILayout.Button ("-", GUILayout.MaxWidth (20))) {
					mDelKey = kv.Key;
				}
			}
			EditorGUILayout.EndHorizontal ();
		}

	}
}