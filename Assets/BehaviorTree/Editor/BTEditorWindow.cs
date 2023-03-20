using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BT
{
	public class BtEditorWindow : EditorWindow
	{
		[MenuItem("Tools/Behavior Editor %&d")]
		public static void ShowWindow()
		{
			mWindow = GetWindow<BtEditorWindow>("行为树编辑器");
			mWindow.Initialize();
		}

		private static BtEditorWindow mWindow = null;

		public static BtEditorWindow Window
		{
			get
			{
				if (mWindow == null)
					ShowWindow();
				return mWindow;
			}
		}

		#region BehaviorTree

		/// <summary>
		/// 当前移动坐标 鼠标拖拽背景偏移
		/// </summary>
		public Vector2 Position { get; set; } = Vector2.zero;

		public Event Event
		{
			get
			{
				if (Event.current != null) return Event.current;
				var evt = new Event {type = EventType.Ignore};
				return evt;
			}
		}

		public BtNode CurSelectNode
		{
			get => mCurSelectNode;
			set
			{
				if (value != mCurSelectNode)
					Tab = 0;
				mCurSelectNode = value;
			}
		}
		private BtNode mCurSelectNode = null;

		[HideInInspector] public BtGrid BtGrid;

		private BehaviourTree mBehaviourTree;
		
		private Rect mNodeInspectorRect;

		public void Initialize()
		{
			if (mBehaviourTree == null)
				mBehaviourTree = new BehaviourTree();

			if (BtGrid == null)
				BtGrid = new BtGrid();

			mNodeInspectorRect = new Rect(position.width - BtConst.RightInspectWidth, 0,
				BtConst.RightInspectWidth, BtConst.RightInspectHeight);

			BtHelper.LoadNodeFile();

			LoadBehaviorTree();
		}

		void OnGUI()
		{
			BtGrid.DrawGrid(position.size);
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					mBehaviourTree.Update(position);
				}
				GUILayout.EndVertical();
				BeginWindows();
				GUILayout.Window(0, mNodeInspectorRect, NodeInspectorWindow, "Inspector");
				EndWindows();
			}
			GUILayout.EndHorizontal();

			if (Event.type == EventType.KeyUp)
			{
				if (Event.keyCode == KeyCode.Delete)
				{
					if (CurSelectNode != null && !CurSelectNode.IsRoot)
					{
						Event.Use();
						BtHelper.RemoveChild(CurSelectNode);
					}
				}
			}
		}

		public BtNode GetMouseTriggerDownPoint(Vector2 mousePos)
		{
			foreach (var node in mBehaviourTree.NodeDict.Values)
			{
				if (node.Graph.DownPointRect.Contains(mousePos))
					return node;
			}
			return null;
		}

		#endregion

		#region NodeEditor

		private const int SPACE_VALUE = 10;
		private const int BTN_ICON_WIDTH = 28;
		private const string DEFAULE_BT_NAME = "新建行为树";

		private static string[] TAB =
		{
			"Node Inspector",
			"Node Data Option",
		};
		public int Tab { get; set; }

		private int mCurSelectJson = 0;
		private int mLastSelectJson = 0;
		private string[] mAllShowJsons;

		private string mCurKey = string.Empty;
		private string mCurValue = string.Empty;
		private string mCurDelKey = null;
		private Dictionary<string, string> mCurChangeDict = new Dictionary<string, string>();

		private string mLastNodeGuid = string.Empty;
		private bool mIsSettingNode = false;

		public static bool IsAutoAlign = true;
		public static bool IsLockAxisY = false;
		public static bool IsDebug = false;

		public static BtNode CopyNode = null;

		private void NodeInspectorWindow(int win_id)
		{
			Tab = GUILayout.Toolbar(Tab, TAB);
			GUILayout.Space(SPACE_VALUE);
			switch (Tab)
			{
				case 0:
					DrawNodeInspector();
					break;
				case 1:
					DrawNodeOption();
					break;
			}
		}

		private void DrawNodeInspector()
		{
			EditorGUILayout.BeginHorizontal();
			{
				IsDebug = GUILayout.Toggle(IsDebug, "是否调试");
				GUI.enabled = IsDebug;
				IsAutoAlign = GUILayout.Toggle(IsAutoAlign, "自动对齐");
				IsLockAxisY = GUILayout.Toggle(IsLockAxisY, "锁定Y轴");
				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(SPACE_VALUE);
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("JsonBT目录"))
				{
					System.Diagnostics.Process.Start(BtHelper.jsonPath);
				}
				if (GUILayout.Button("LuaBT目录"))
				{
					System.Diagnostics.Process.Start(BtHelper.behaviorPath);
				}
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(SPACE_VALUE);
			if (GUILayout.Button("加载行为树"))
				LoadBehaviorTree();

			if (mAllShowJsons != null && mAllShowJsons.Length > 0 && mLastSelectJson != mCurSelectJson)
			{
				mLastSelectJson = mCurSelectJson;
				var fileName = mAllShowJsons[mCurSelectJson];
				var file = Path.Combine(BtHelper.jsonPath, $"{fileName}.json");
				mBehaviourTree = BtHelper.LoadBehaviorTree(file);
				if (mBehaviourTree == null)
				{
					Debug.LogErrorFormat("读取行为树失败, {0}", file);
					mBehaviourTree = new BehaviourTree(DEFAULE_BT_NAME);
				}
			}

			if (GUILayout.Button(DEFAULE_BT_NAME))
			{
				mBehaviourTree = new BehaviourTree(DEFAULE_BT_NAME);
			}

			if (mAllShowJsons != null && mAllShowJsons.Length > 0)
			{
				GUILayout.Space(SPACE_VALUE);
				EditorGUIUtility.labelWidth = 40;
				EditorGUILayout.BeginHorizontal();
				{
					mCurSelectJson = EditorGUILayout.Popup("行为树:", mCurSelectJson, mAllShowJsons);
					if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"),
						GUILayout.Width(BTN_ICON_WIDTH)))
					{
						var fileName = mAllShowJsons[mCurSelectJson];
						var filePath = Path.Combine(BtHelper.jsonPath, $"{fileName}.json");
						File.Delete(filePath);
						LoadBehaviorTree();
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 60;
			}

			GUILayout.Space(SPACE_VALUE);
			EditorGUILayout.BeginVertical("Box");
			{
				if (mBehaviourTree != null)
					mBehaviourTree.Name = EditorGUILayout.TextField("行为树名:", mBehaviourTree.Name);

				GUI.color = Color.green;
				if (GUILayout.Button("保存行为树"))
				{
					if (mBehaviourTree != null && mBehaviourTree.BrokenNodeDict.Count > 0)
						EditorUtility.DisplayDialog("提示", "有节点未连上", "确定");
					else
						BtHelper.SaveBTData(mBehaviourTree);
				}
				GUI.color = Color.white;
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(SPACE_VALUE);
			var node = Window.CurSelectNode;
			if (node != null)
			{
				var data = node.Data;
				EditorGUILayout.BeginHorizontal();

				if (mIsSettingNode)
				{
					EditorGUILayout.LabelField("节点说明", GUILayout.MaxWidth(50));
					data.desc = EditorGUILayout.TextArea(data.desc, GUILayout.MaxWidth(BtConst.RightInspectWidth));
				}
				else
				{
					EditorGUILayout.HelpBox(data.desc, MessageType.Info);
				}

				if (GUILayout.Button(mIsSettingNode ? "ok" : "set", GUILayout.MaxWidth(30)))
					mIsSettingNode = !mIsSettingNode;
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(SPACE_VALUE);
				EditorGUILayout.BeginVertical("Box");
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("节点数据");
					GUILayout.FlexibleSpace();
					if (data.type.StartsWith("tasks/"))
					{
						if (GUILayout.Button("编辑脚本"))
							System.Diagnostics.Process.Start(Path.Combine(BtHelper.nodePath, data.type + ".lua"));
					}

					EditorGUILayout.EndHorizontal();
					data.displayName = EditorGUILayout.TextField("显示名:", data.displayName);
					EditorGUILayout.LabelField("节点名:", data.name);
					if (node.Guid != mLastNodeGuid)
					{
						mLastNodeGuid = node.Guid;
						mCurChangeDict.Clear();
					}

					DrawDataInspector(data);
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void LoadBehaviorTree()
		{
			var files = Directory.GetFiles(BtHelper.jsonPath, "*.json", SearchOption.AllDirectories);
			var fileNames = new List<string>();
			foreach (var file in files)
				fileNames.Add(Path.GetFileNameWithoutExtension(file));
			mAllShowJsons = fileNames.ToArray();
			if (mCurSelectJson >= mAllShowJsons.Length)
				mCurSelectJson = mAllShowJsons.Length - 1;
			mLastSelectJson = -1;
		}

		private void DrawDataInspector(BtNodeData data)
		{
			EditorGUIUtility.labelWidth = 24;

			if (!string.IsNullOrEmpty(mCurDelKey))
			{
				data.RemoveData(mCurDelKey);
				if (mCurChangeDict.ContainsKey(mCurDelKey))
					mCurChangeDict.Remove(mCurDelKey);
				mCurDelKey = null;
			}

			foreach (var key in mCurChangeDict.Keys)
			{
				data.AddData(key, mCurChangeDict[key]);
			}

			mCurChangeDict.Clear();

			if (data.data != null)
			{
				foreach (var kv in data.data)
				{
					DrawItemInspector(kv);
				}
			}

			EditorGUILayout.BeginHorizontal();
			{
				mCurKey = EditorGUILayout.TextField("key:", mCurKey);
				mCurValue = EditorGUILayout.TextField("val:", mCurValue);
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(BTN_ICON_WIDTH)))
				{
					if (!string.IsNullOrEmpty(mCurKey))
					{
						data.AddData(mCurKey, mCurValue);
						mCurKey = "";
						mCurValue = "";
					}
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawItemInspector(KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("key:", kv.Key);
				mCurChangeDict[kv.Key] = EditorGUILayout.TextField("val:", kv.Value, GUILayout.MaxWidth(80));
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mCurDelKey = kv.Key;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region Node Default Option
		
		private static Dictionary<string, Dictionary<string, string>> mOptions;
		
		private Vector2 mDefScrollPos = Vector2.zero;
		
		private string mDefKey = "";
		private string mDefValue = "";
		private string mDefDelKey = null;
		private Dictionary<string, string> mDefChangeDict = new Dictionary<string, string>();
		
		private string mAddNode = null;
		private string mDelNode = null;
		private string mSelectNode = "";
		private Dictionary<string, string> mSelectDict;
		
		private void DrawNodeOption()
		{
			if (IsDebug && GUILayout.Button("刷新路径"))
				BtHelper.CleanPath();
			if (GUILayout.Button("读取配置"))
			{
				mOptions = BtHelper.ReadBTNodeOption();
			}
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
		
			if (mSelectDict != null && !string.IsNullOrEmpty(mDefDelKey))
			{
				mSelectDict.Remove(mDefDelKey);
				if (mDefChangeDict.ContainsKey(mDefDelKey))
					mDefChangeDict.Remove(mDefDelKey);
				mDefDelKey = null;
			}
		
			foreach (var key in mDefChangeDict.Keys)
			{
				if (mSelectDict.ContainsKey(key))
					mSelectDict[key] = mDefChangeDict[key];
				else
					mSelectDict.Add(key, mDefChangeDict[key]);
			}
		
			mDefChangeDict.Clear();
		
			if (mSelectDict != null)
			{
				GUILayout.Space(SPACE_VALUE);
				EditorGUILayout.BeginVertical("Box");
				{
					EditorGUILayout.TextField("修改配置: " + mSelectNode);
		
					EditorGUIUtility.labelWidth = 24;
					foreach (var kv in mSelectDict)
					{
						DrawItemOptionInspector(kv);
					}
		
					EditorGUILayout.BeginHorizontal();
					{
						mDefKey = EditorGUILayout.TextField("key:", mDefKey);
						mDefValue = EditorGUILayout.TextField("val:", mDefValue);
						if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"),
							GUILayout.Width(BTN_ICON_WIDTH)))
						{
							if (!string.IsNullOrEmpty(mDefKey))
							{
								mSelectDict.Add(mDefKey, mDefValue);
								mDefKey = "";
								mDefValue = "";
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}

			if (mOptions != null)
			{
				GUILayout.Space(SPACE_VALUE);
				EditorGUILayout.LabelField("所有配置:");
				mDefScrollPos = EditorGUILayout.BeginScrollView(mDefScrollPos);
				{
					EditorGUIUtility.labelWidth = 35;
					foreach (var key in mOptions.Keys)
						DrawOptionInspector(key);
				}
				EditorGUILayout.EndScrollView();

				EditorGUIUtility.labelWidth = 60;
				EditorGUILayout.BeginHorizontal();
				{
					mAddNode = EditorGUILayout.TextField("新增节点:", mAddNode);
					if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(BTN_ICON_WIDTH)))
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
		
		private void DrawItemOptionInspector(KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("key:", kv.Key);
				mDefChangeDict[kv.Key] = EditorGUILayout.TextField("val:", kv.Value);
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mDefDelKey = kv.Key;
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
					mDefChangeDict.Clear();
					mSelectNode = key;
					mSelectDict = mOptions[key];
				}
		
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mDelNode = key;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		
		#endregion
	}
}