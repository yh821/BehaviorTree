using System.IO;
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
		private bool mIsSettingNode = false;

		//public static bool IsAutoAlign = true;
		//public static bool IsLockAxisY = false;

		public static BTNode CopyNode = null;

		void DrawNodeInspector ()
		{
			GUI.DrawTexture (new Rect (position.width - BTConst.RIGHT_INSPECT_WIDTH - 5, 0, 
				BTConst.RIGHT_INSPECT_WIDTH + 5, 500), BTNodeStyle.NodeEditorBG);

			GUILayout.Space (SPACE_VALUE);
			//EditorGUILayout.BeginHorizontal ();
			//{
			//	IsAutoAlign = GUILayout.Toggle (IsAutoAlign, "自动对齐", GUILayout.MaxWidth (80));
			//	IsLockAxisY = GUILayout.Toggle (IsLockAxisY, "锁定Y轴", GUILayout.MaxWidth (80));
			//}
			//EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			{
				if (GUILayout.Button ("JsonBT目录")) {
					System.Diagnostics.Process.Start (BTHelper.jsonPath);
				}
				if (GUILayout.Button ("LuaBT目录")) {
					System.Diagnostics.Process.Start (BTHelper.behaviorPath);
				}
				if (GUILayout.Button ("Node配置")) {
					BTEditorOption.ShowWindow ();
				}
			}
			EditorGUILayout.EndHorizontal ();

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

			GUILayout.Space (SPACE_VALUE);			
			EditorGUIUtility.labelWidth = 40;
			if (mAllShowJsons != null && mAllShowJsons.Length > 0)
				mCurSelectJson = EditorGUILayout.Popup ("行为树:", mCurSelectJson, mAllShowJsons);
			EditorGUIUtility.labelWidth = 60;

			GUILayout.Space (SPACE_VALUE);
			EditorGUILayout.BeginVertical ("Box");
			{
				EditorGUILayout.LabelField ("行为树数据");
				if (mBehaviourTree != null) {
					mBehaviourTree.Name = EditorGUILayout.TextField ("行为树名:", mBehaviourTree.Name);
				}

				GUI.color = Color.green;
				if (GUILayout.Button ("保存行为树")) {
					if (mBehaviourTree.OrphanNodeDict.Count > 0)
						EditorUtility.DisplayDialog ("提示", "有节点未连上", "确定");
					else
						BTHelper.SaveBTData (mBehaviourTree);
				}
				GUI.color = Color.white;
			}
			EditorGUILayout.EndVertical ();

			GUILayout.Space (SPACE_VALUE);
			var node = window.CurSelectNode;
			if (node != null) {
				var data = node.Data;
				EditorGUILayout.BeginHorizontal ();

				if (mIsSettingNode) {
					EditorGUILayout.LabelField ("节点说明", GUILayout.MaxWidth (50));
					data.desc = EditorGUILayout.TextArea (data.desc, GUILayout.MaxWidth (BTConst.RIGHT_INSPECT_WIDTH));
				} else {
					EditorGUILayout.HelpBox (data.desc, MessageType.Info);
				}

				if (GUILayout.Button (mIsSettingNode ? "ok" : "set", GUILayout.MaxWidth (30)))
					mIsSettingNode = !mIsSettingNode;
				EditorGUILayout.EndHorizontal ();

				GUILayout.Space (SPACE_VALUE);
				EditorGUILayout.BeginVertical ("Box");
				{
					EditorGUILayout.LabelField ("节点数据");
					data.displayName = EditorGUILayout.TextField ("显示名:", data.displayName);
					EditorGUILayout.LabelField ("节点名:", data.name);
					if (node.Guid != mLastNodeGuid) {
						mLastNodeGuid = node.Guid;
						mChangeDict.Clear ();
					}

					DrawDataInspector (data);
				}
				EditorGUILayout.EndVertical ();
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
				mValue = EditorGUILayout.TextField ("val:", mValue);
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