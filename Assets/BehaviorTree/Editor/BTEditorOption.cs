using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BT
{
	public class BTEditorOption : EditorWindow
	{
		[MenuItem("Tools/Behavior Editor Option")]
		public static void ShowWindow()
		{
			mWindow = GetWindow<BTEditorOption>("节点默认配置");
		}

		private static BTEditorOption mWindow = null;
		private static Dictionary<string, Dictionary<string, string>> mOptions;

		private const int SPACE_VALUE = 6;
		private Vector2 scrollPos = Vector2.zero;

		private string mKey = "";
		private string mValue = "";
		private string mDelKey = null;
		private Dictionary<string, string> mChangeDict = new Dictionary<string, string>();

		private string mAddNode = null;
		private string mDelNode = null;
		private string mSelectNode = "";
		private Dictionary<string, string> mSelectDict;

		void OnGUI()
		{
			GUILayout.Space(SPACE_VALUE);
			if (GUILayout.Button("刷新路径"))
			{
				BtHelper.CleanPath();
			}

			GUILayout.Space(SPACE_VALUE);
			if (GUILayout.Button("读取配置"))
			{
				mOptions = BtHelper.ReadBTNodeOption();
			}

			GUILayout.Space(SPACE_VALUE);
			GUI.color = Color.green;
			if (GUILayout.Button("保存配置"))
			{
				if (mOptions != null)
					BtHelper.WriteBtNodeOption(mOptions);
			}

			GUI.color = Color.white;

			if (!string.IsNullOrEmpty(mDelNode))
			{
				mOptions.Remove(mDelNode);
				mDelNode = null;
			}

			if (mSelectDict != null && !string.IsNullOrEmpty(mDelKey))
			{
				mSelectDict.Remove(mDelKey);
				if (mChangeDict.ContainsKey(mDelKey))
					mChangeDict.Remove(mDelKey);
				mDelKey = null;
			}

			foreach (var key in mChangeDict.Keys)
			{
				if (mSelectDict.ContainsKey(key))
					mSelectDict[key] = mChangeDict[key];
				else
					mSelectDict.Add(key, mChangeDict[key]);
			}

			mChangeDict.Clear();

			if (mSelectDict != null)
			{
				EditorGUIUtility.labelWidth = 70;
				EditorGUILayout.BeginVertical("Box");
				{
					EditorGUILayout.TextField("修改配置:", mSelectNode);

					EditorGUIUtility.labelWidth = 24;
					foreach (var kv in mSelectDict)
					{
						DrawItemInspector(kv);
					}

					EditorGUILayout.BeginHorizontal();
					{
						mKey = EditorGUILayout.TextField("key:", mKey);
						mValue = EditorGUILayout.TextField("val:", mValue);
						if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
						{
							if (!string.IsNullOrEmpty(mKey))
							{
								mSelectDict.Add(mKey, mValue);
								mKey = "";
								mValue = "";
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(SPACE_VALUE);
			}

			if (mOptions != null)
			{
				EditorGUILayout.LabelField("所有配置:");
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				EditorGUIUtility.labelWidth = 35;
				foreach (var key in mOptions.Keys)
				{
					DrawOptionInspector(key);
				}

				EditorGUILayout.EndScrollView();

				EditorGUIUtility.labelWidth = 60;
				EditorGUILayout.BeginHorizontal();
				{
					mAddNode = EditorGUILayout.TextField("新增节点:", mAddNode);
					if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
					{
						if (!string.IsNullOrEmpty(mAddNode))
						{
							var data = new Dictionary<string, string>();
							data.Add("displayName", "");
							mOptions.Add(mAddNode, data);
							mAddNode = null;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}

		}

		private void DrawItemInspector(KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("key:", kv.Key);
				mChangeDict[kv.Key] = EditorGUILayout.TextField("val:", kv.Value);
				if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
				{
					mDelKey = kv.Key;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawOptionInspector(string key)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("节点:", key);
				if (GUILayout.Button("修改配置", GUILayout.MaxWidth(70)))
				{
					mChangeDict.Clear();
					mSelectNode = key;
					mSelectDict = mOptions[key];
				}

				if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
				{
					mDelNode = key;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

	}
}