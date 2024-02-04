using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace BT
{
	public class BtEditorWindow : EditorWindow
	{
		// [MenuItem("Tools/Behavior Editor %&d")]
		[MenuItem("Tools/Behavior Editor %t")]
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
		public static Vector2 Position { get; set; } = Vector2.zero;

		public static GUIStyle LabelStyle { get; } = new GUIStyle("flow node 3");
		private const int DefaultFontSize = 12;
		private static float mScale = 1f;

		public static float Scale
		{
			get => mScale;
			set
			{
				if (value > 1f) mScale = 1f;
				else if (value < 0.5f) mScale = 0.5f;
				else
				{
					mScale = value;
					LabelStyle.fontSize = Mathf.RoundToInt(DefaultFontSize * mScale);
				}
			}
		}

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
				{
					Tab = 0;
					mCurChangeDict.Clear();
				}
				mCurSelectNode = value;
			}
		}

		private BtNode mCurSelectNode = null;

		private BtGrid mBtGrid;

		public BehaviourTree CurBehaviourTree
		{
			get => mCurBehaviourTree;
			set
			{
				if (value != mCurBehaviourTree)
				{
					mCurSharedChangeDict.Clear();
					CurSelectNode = null;
				}
				mCurBehaviourTree = value;
			}
		}

		private BehaviourTree mCurBehaviourTree;

		private Rect mNodeInspectorRect;
		private Rect mTreeInspectorRect;

		public void Initialize()
		{
			PLUS_ICON = EditorGUIUtility.IconContent("Toolbar Plus");
			MINUS_ICON = EditorGUIUtility.IconContent("Toolbar Minus");
			defaultLabelWidth = EditorGUIUtility.labelWidth;

			if (CurBehaviourTree == null) CurBehaviourTree = new BehaviourTree(this);
			if (mBtGrid == null) mBtGrid = new BtGrid();
			BtHelper.LoadNodeFile();
			LoadBehaviorTree();
		}

		private void OnGUI()
		{
			mBtGrid.DrawGrid(position.size);
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				CurBehaviourTree.Update(position);
				GUILayout.EndVertical();

				if (mAllShowJson != null && mAllShowJson.Length > 0 && mLastSelectJson != mCurSelectJson)
				{
					mLastSelectJson = mCurSelectJson;
					var fileName = mAllShowJson[mCurSelectJson];
					var file = Path.Combine(BtHelper.JsonPath, $"{fileName}.json");
					CurBehaviourTree = BtHelper.LoadBehaviorTree(this, file);
					if (CurBehaviourTree == null)
					{
						Debug.LogErrorFormat("读取行为树失败, {0}", file);
						CurBehaviourTree = new BehaviourTree(this, DEFAULE_BT_NAME);
					}
				}

				BeginWindows();
				{
					mNodeInspectorRect = new Rect(position.width - BtConst.InspectWidth, 0,
						BtConst.InspectWidth, BtConst.NodeInspectHeight);
					GUILayout.Window(0, mNodeInspectorRect, NodeInspectorWindow, "Node Inspector");

					mTreeInspectorRect = new Rect(position.width - BtConst.InspectWidth, BtConst.NodeInspectHeight,
						BtConst.InspectWidth, BtConst.TreeInspectHeight);
					GUILayout.Window(1, mTreeInspectorRect, TreeInspectorWindow, "Tree Inspector");
				}
				EndWindows();
			}
			GUILayout.EndHorizontal();

			if (Event.current.rawType == EventType.KeyDown) EventCallBack(Event.current);
		}

		private void EventCallBack(Event e)
		{
			//单个按键
			if (e.keyCode == KeyCode.Delete)
			{
				if (CurSelectNode != null && !CurSelectNode.IsRoot)
				{
					Event.Use();
					BtHelper.RemoveChild(CurSelectNode);
					CurSelectNode = null;
				}
			}
			//LeftControl的组合键
			if ((e.modifiers & EventModifiers.Control) != 0)
			{
				switch (e.keyCode)
				{
					case KeyCode.C:
						CopyNode = Window.CurSelectNode;
						break;
					case KeyCode.V:
						if (CopyNode == null) break;
						var tree = Window.CurBehaviourTree;
						var pos = e.mousePosition - Position;
						var clone = BtHelper.PasteChild(tree, null, pos);
						Window.CurSelectNode = clone;
						tree.AddBrokenNode(clone);
						break;
				}
			}
		}

		public BtNode GetMouseTriggerDownPoint(Vector2 mousePos)
		{
			foreach (var node in CurBehaviourTree.NodeDict.Values)
			{
				if (node.Graph.DownPointRect.Contains(mousePos))
					return node;
			}
			return null;
		}

		#endregion

		#region NodeInspector

		private const int SPACE_VALUE = 10;
		private const int BTN_ICON_WIDTH = 28;
		private const string DEFAULE_BT_NAME = "behavior_tree";
		private const string KEY_NAME = "key:";
		private const string VAL_NAME = "val:";

		private static GUIContent PLUS_ICON;
		private static GUIContent MINUS_ICON;

		private static readonly string[] TAB =
		{
			"Behaviour Tree",
			"Common Option",
		};

		private int Tab { get; set; }
		private float defaultLabelWidth;

		private int mCurSelectJson = 0;
		private int mLastSelectJson = 0;
		private string[] mAllShowJson;

		private bool mIsSettingNode = false;

		private string mCurKey = string.Empty;
		private string mCurValue = string.Empty;
		private string mCurDelKey = null;
		private readonly Dictionary<string, string> mCurChangeDict = new Dictionary<string, string>();

		private string mCurSharedKey = string.Empty;
		private string mCurSharedValue = string.Empty;
		private string mCurSharedDelKey = null;
		private readonly Dictionary<string, string> mCurSharedChangeDict = new Dictionary<string, string>();

		public static bool IsAutoAlign = true;
		public static bool IsShowPos = false;
		public static bool IsShowIndex = false;
		public static bool IsLockAxisY = false;
		public static bool IsDebug = false;

		public static BtNode CopyNode = null;

		private void NodeInspectorWindow(int winId)
		{
			DrawNodeInspector();
		}

		private void TreeInspectorWindow(int winId)
		{
			Tab = GUILayout.Toolbar(Tab, TAB);
			GUILayout.Space(SPACE_VALUE);
			switch (Tab)
			{
				case 0:
					DrawTreeInspector();
					DrawSharedInspector();
					break;
				case 1:
					DrawCommonOption();
					break;
			}
		}

		private void DrawTreeInspector()
		{
			EditorGUILayout.BeginHorizontal();
			{
				IsDebug = GUILayout.Toggle(IsDebug, "调试");
				// GUI.enabled = IsDebug;
				IsAutoAlign = GUILayout.Toggle(IsAutoAlign, "自动对齐");
				IsShowPos = GUILayout.Toggle(IsShowPos, "坐标");
				IsShowIndex = GUILayout.Toggle(IsShowIndex, "序号");
				// IsLockAxisY = GUILayout.Toggle(IsLockAxisY, "锁定Y轴");
				// GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical("Box");
			{
				EditorGUILayout.LabelField("当前行为树");
				if (mAllShowJson != null && mAllShowJson.Length > 0)
				{
					// GUILayout.Space(SPACE_VALUE);
					EditorGUIUtility.labelWidth = 40;
					EditorGUILayout.BeginHorizontal();
					{
						mCurSelectJson = EditorGUILayout.Popup(mCurSelectJson, mAllShowJson);
						if (GUILayout.Button(MINUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
						{
							var fileName = mAllShowJson[mCurSelectJson];
							var filePath = Path.Combine(BtHelper.JsonPath, $"{fileName}.json");
							File.Delete(filePath);
							LoadBehaviorTree();
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUIUtility.labelWidth = 60;
				}
				if (CurBehaviourTree != null)
					CurBehaviourTree.Name = EditorGUILayout.TextField("行为树名:", CurBehaviourTree.Name);

				GUI.color = Color.green;
				if (GUILayout.Button("保存行为树"))
				{
					if (CurBehaviourTree != null && CurBehaviourTree.BrokenNodeDict.Count > 0)
						EditorUtility.DisplayDialog("提示", "有节点未连上", "确定");
					else
						BtHelper.SaveBtData(CurBehaviourTree);
				}
				GUI.color = Color.white;
			}
			EditorGUILayout.EndVertical();

			if (GUILayout.Button("新建行为树"))
				CurBehaviourTree = new BehaviourTree(this, DEFAULE_BT_NAME);
			if (GUILayout.Button("加载行为树"))
				LoadBehaviorTree();
		}

		private void DrawSharedInspector()
		{
			var data = CurBehaviourTree.Root.Data;
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("共享数据", GUILayout.MaxWidth(50));
			EditorGUIUtility.labelWidth = 25;
			mTreeScrollPos = EditorGUILayout.BeginScrollView(mTreeScrollPos);
			{
				if (!string.IsNullOrEmpty(mCurSharedDelKey))
				{
					data.RemoveSharedData(mCurSharedDelKey);
					if (mCurSharedChangeDict.ContainsKey(mCurSharedDelKey))
						mCurSharedChangeDict.Remove(mCurSharedDelKey);
					mCurSharedDelKey = null;
				}

				foreach (var key in mCurSharedChangeDict.Keys)
					data.AddSharedData(key, mCurSharedChangeDict[key]);

				mCurSharedChangeDict.Clear();

				if (data.sharedData != null)
					DrawSharedItemInspector(data.sharedData);
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.BeginHorizontal();
			{
				mCurSharedKey = EditorGUILayout.TextField(KEY_NAME, mCurSharedKey);
				mCurSharedValue = EditorGUILayout.TextField(VAL_NAME, mCurSharedValue);
				if (GUILayout.Button(PLUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
				{
					if (BtHelper.CheckKey(mCurSharedKey))
					{
						data.AddSharedData(mCurSharedKey, mCurSharedValue);
						mCurSharedKey = "";
						mCurSharedValue = "";
						GUIUtility.keyboardControl = 0;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		private void DrawNodeInspector()
		{
			EditorGUIUtility.labelWidth = 60;
			var node = Window.CurSelectNode;
			if (node != null)
			{
				var data = node.Data;
				GUI.enabled = data.enabled;
				EditorGUILayout.BeginHorizontal();

				if (mIsSettingNode)
				{
					EditorGUILayout.LabelField("节点说明", GUILayout.MaxWidth(50));
					data.desc = EditorGUILayout.TextArea(data.desc, GUILayout.MaxWidth(BtConst.InspectWidth));
				}
				else EditorGUILayout.HelpBox(data.desc, MessageType.Info);

				if (GUILayout.Button(mIsSettingNode ? "ok" : "set", GUILayout.MaxWidth(30)))
					mIsSettingNode = !mIsSettingNode;
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginVertical("Box");
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("节点数据", GUILayout.MaxWidth(50));
					// GUILayout.FlexibleSpace();
					if (IsDebug && !string.IsNullOrEmpty(data.type))
					{
						if (GUILayout.Button("选中脚本"))
						{
							var file = FileUtil.GetProjectRelativePath(Path.Combine(BtHelper.NodePath,
								data.type + ".lua"));
							var lua = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
							Selection.activeObject = lua;
						}

						if (GUILayout.Button("编辑脚本"))
							System.Diagnostics.Process.Start(Path.Combine(BtHelper.NodePath, data.type + ".lua"));
						// BtHelper.OpenFile(Path.Combine(BtHelper.nodePath, data.type + ".lua"));
					}
					EditorGUILayout.EndHorizontal();

					data.name = EditorGUILayout.TextField("显示名:", data.name);
					if (IsDebug) EditorGUILayout.LabelField("节点名:", data.file);

					mDataScrollPos = EditorGUILayout.BeginScrollView(mDataScrollPos);
					switch (node.NodeType.Type)
					{
						case TaskType.Composite:
							break;
						case TaskType.Root:
							DrawRootInspector(data);
							break;
						case TaskType.Selector:
						case TaskType.Sequence:
							DrawAbortInspector(data);
							break;
						case TaskType.Trigger:
						case TaskType.IsTrigger:
							DrawTriggerInspector(data);
							break;
						default:
							DrawDataInspector(data);
							break;
					}
					EditorGUILayout.EndScrollView();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUIUtility.labelWidth = 25;
						mCurKey = EditorGUILayout.TextField(KEY_NAME, mCurKey);
						mCurValue = EditorGUILayout.TextField(VAL_NAME, mCurValue);
						if (GUILayout.Button(PLUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
						{
							if (BtHelper.CheckKey(mCurKey))
							{
								data.AddData(mCurKey, mCurValue);
								mCurKey = "";
								mCurValue = "";
								GUIUtility.keyboardControl = 0;
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				GUI.enabled = true;
			}
		}

		private void LoadBehaviorTree()
		{
			var files = Directory.GetFiles(BtHelper.JsonPath, "*.json", SearchOption.AllDirectories);
			mAllShowJson = files.Select(Path.GetFileNameWithoutExtension).ToArray();
			if (mCurSelectJson >= mAllShowJson.Length)
				mCurSelectJson = mAllShowJson.Length - 1;
			mLastSelectJson = -1;
		}

		private void DrawRootInspector(BtNodeData data)
		{
			var reStart = false;
			if (data.data != null && data.data.TryGetValue(BtConst.Restart, out var value))
				reStart = value == "1";
			reStart = EditorGUILayout.Toggle("是否循环", reStart);
			data.AddData(BtConst.Restart, reStart ? "1" : "");
		}

		private void DrawAbortInspector(BtNodeData data)
		{
			var abortType = AbortType.None;
			if (data.data != null && data.data.TryGetValue(BtConst.AbortType, out var value))
				Enum.TryParse(value, out abortType);
			abortType = (AbortType) EditorGUILayout.EnumPopup("打断类型", abortType);
			data.AddData(BtConst.AbortType, abortType.ToString());
		}

		private void DrawTriggerInspector(BtNodeData data)
		{
			var type = TriggerType.Equals;
			if (data.data != null && data.data.TryGetValue(BtConst.TriggerType, out var typeStr))
				Enum.TryParse(typeStr, out type);
			type = (TriggerType) EditorGUILayout.EnumPopup("判断类型", type);
			data.AddData(BtConst.TriggerType, type.ToString());

			var value = 0;
			if (data.data != null && data.data.TryGetValue(BtConst.TriggerValue, out var valueStr))
				int.TryParse(valueStr, out value);
			value = EditorGUILayout.IntField("判断数值", value);
			data.AddData(BtConst.TriggerValue, value.ToString());
		}

		private void DrawDataInspector(BtNodeData data)
		{
			EditorGUIUtility.labelWidth = 25;
			if (!string.IsNullOrEmpty(mCurDelKey))
			{
				data.RemoveData(mCurDelKey);
				if (mCurChangeDict.ContainsKey(mCurDelKey))
					mCurChangeDict.Remove(mCurDelKey);
				mCurDelKey = null;
			}

			foreach (var key in mCurChangeDict.Keys)
				data.AddData(key, mCurChangeDict[key]);

			mCurChangeDict.Clear();

			if (data.data != null)
				DrawItemInspector(data.data);
		}

		private void DrawItemInspector(IDictionary<string, string> data)
		{
			foreach (var kv in data)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(KEY_NAME, kv.Key);
				mCurChangeDict[kv.Key] = EditorGUILayout.TextField(VAL_NAME, kv.Value, GUILayout.MaxWidth(80));
				if (GUILayout.Button(MINUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mCurDelKey = kv.Key;
					GUIUtility.keyboardControl = 0;
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawSharedItemInspector(IDictionary<string, string> data)
		{
			foreach (var kv in data)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(KEY_NAME, kv.Key);
				mCurSharedChangeDict[kv.Key] = EditorGUILayout.TextField(VAL_NAME, kv.Value, GUILayout.MaxWidth(80));
				if (GUILayout.Button(MINUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mCurSharedDelKey = kv.Key;
					GUIUtility.keyboardControl = 0;
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		#endregion

		#region NodeOption

		private static Dictionary<string, Dictionary<string, string>> mOptions;

		private Vector2 mDefScrollPos = Vector2.zero;
		private Vector2 mDataScrollPos = Vector2.zero;
		private Vector2 mTreeScrollPos = Vector2.zero;

		private string mDefKey = "";
		private string mDefValue = "";
		private string mDefDelKey = null;
		private Dictionary<string, string> mDefChangeDict = new Dictionary<string, string>();

		private string mAddNode = null;
		private string mDelNode = null;
		private string mSelectNode = "";
		private Dictionary<string, string> mSelectDict;

		private void DrawCommonOption()
		{
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("JsonBT目录"))
					System.Diagnostics.Process.Start(BtHelper.JsonPath);
				if (GUILayout.Button("LuaBT目录"))
					System.Diagnostics.Process.Start(BtHelper.BehaviorPath);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(SPACE_VALUE);
			if (IsDebug && GUILayout.Button("刷新路径")) BtHelper.CleanPath();

			if (GUILayout.Button("读取配置")) mOptions = BtHelper.ReadBtNodeOption();

			GUI.color = Color.green;
			if (GUILayout.Button("保存配置"))
			{
				if (mOptions != null)
					BtHelper.WriteBtNodeOption(mOptions);
			}
			GUI.color = Color.white;

			if (!string.IsNullOrEmpty(mDelNode))
			{
				mOptions?.Remove(mDelNode);
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
				EditorGUILayout.LabelField("修改配置: " + mSelectNode);

				EditorGUIUtility.labelWidth = 25;
				foreach (var kv in mSelectDict)
					DrawItemOptionInspector(kv);

				EditorGUILayout.BeginHorizontal();
				{
					mDefKey = EditorGUILayout.TextField(KEY_NAME, mDefKey);
					mDefValue = EditorGUILayout.TextField(VAL_NAME, mDefValue);
					if (GUILayout.Button(PLUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
					{
						if (BtHelper.CheckKey(mDefKey))
						{
							mSelectDict.Add(mDefKey, mDefValue);
							mDefKey = "";
							mDefValue = "";
							GUIUtility.keyboardControl = 0;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}

			if (mOptions != null)
			{
				EditorGUIUtility.labelWidth = 50;
				EditorGUILayout.LabelField("所有配置");
				mDefScrollPos = EditorGUILayout.BeginScrollView(mDefScrollPos);
				{
					foreach (var key in mOptions.Keys)
						DrawOptionInspector(key);
				}
				EditorGUILayout.EndScrollView();

				EditorGUILayout.BeginHorizontal();
				mAddNode = EditorGUILayout.TextField("新增节点", mAddNode);
				if (GUILayout.Button(PLUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
				{
					if (!string.IsNullOrEmpty(mAddNode))
					{
						var data = new Dictionary<string, string> {{"name", ""}};
						mOptions.Add(mAddNode, data);
						mAddNode = null;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawItemOptionInspector(KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(KEY_NAME, kv.Key);
			mDefChangeDict[kv.Key] = EditorGUILayout.TextField(VAL_NAME, kv.Value);
			if (GUILayout.Button(MINUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
			{
				mDefDelKey = kv.Key;
				GUIUtility.keyboardControl = 0;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawOptionInspector(string key)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(key);
			if (GUILayout.Button("修改配置", GUILayout.MaxWidth(60)))
			{
				mDefChangeDict.Clear();
				mSelectNode = key;
				mSelectDict = mOptions[key];
			}

			if (GUILayout.Button(MINUS_ICON, GUILayout.Width(BTN_ICON_WIDTH)))
				mDelNode = key;
			EditorGUILayout.EndHorizontal();
		}

		#endregion
	}

	public class BtGrid
	{
		private readonly Texture background;

		public BtGrid()
		{
			var path = BtHelper.ToolPath + "/GUI/background.png";
			path = FileUtil.GetProjectRelativePath(path);
			background = AssetDatabase.LoadAssetAtPath<Texture>(path);
		}

		/// <summary>
		/// 绘制背景格子
		/// </summary>
		/// <param name="windowSize"></param>
		public void DrawGrid(Vector2 windowSize)
		{
			Handles();
			DrawBackground(windowSize);
		}

		/// <summary>
		/// 拖拽背景
		/// </summary>
		private void Handles()
		{
			var currentEvent = BtEditorWindow.Window.Event;
			if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 1)
			{
				currentEvent.Use();
				BtEditorWindow.Position += currentEvent.delta / BtEditorWindow.Scale;
			}
		}

		private void DrawBackground(Vector2 windowSize)
		{
			var scale = BtEditorWindow.Scale;
			var position = BtEditorWindow.Position;
			var rect = new Rect(0, 0, windowSize.x, windowSize.y);
			var texCoords = new Rect(-position.x / background.width / scale,
				1.0f - windowSize.y / background.height / scale + position.y / background.height / scale,
				windowSize.x / background.width / scale,
				windowSize.y / background.height / scale);
			GUI.DrawTextureWithTexCoords(rect, background, texCoords);
		}
	}

	public class BtNode
	{
		/// <summary>
		/// 编辑化节点
		/// </summary>
		public BtNodeType NodeType { get; }

		/// <summary>
		/// 唯一识别符
		/// </summary>
		public string Guid { get; }

		public string NodeName { get; }

		public BtNodeData Data { get; }

		public BehaviourTree Owner { get; }

		public BtNode Parent { set; get; }

		public BtNodeGraph Graph { get; }

		public List<BtNode> ChildNodeList { get; }

		public bool IsHaveParent => Parent != null;

		public bool IsHaveChild => ChildNodeList.Count > 0;

		public bool IsRoot => NodeName == BtConst.RootName;

		public bool IsSelected => BtEditorWindow.Window.CurSelectNode == this;

		private bool mCanDragMove = false;
		private bool mIsDragging = false;
		private bool mIsLinkParent = false;

		private Vector3 mBzStartPos;
		private Vector3 mBzEndPos;

		public BtNode(BehaviourTree owner, BtNode parent, BtNodeData data)
		{
			Owner = owner;
			Parent = parent;
			Data = data;
			Graph = new BtNodeGraph();
			NodeName = data.file;
			Graph.RealRect = new Rect(data.posX, data.posY, BtConst.DefaultWidth, BtConst.DefaultHeight);
			ChildNodeList = new List<BtNode>();
			Guid = BtHelper.GenerateUniqueStringId();
			NodeType = BtHelper.CreateNodeType(this);
		}

		public void Update(Rect canvas)
		{
			DrawNode();
			DealHandles(canvas);
		}

		private void DrawNode()
		{
			if (!Data.visable) return;
			if (IsHaveChild)
			{
				mBzStartPos = Graph.DownPointRect.center;
				foreach (var node in ChildNodeList)
				{
					if (!node.Data.visable) continue;
					mBzEndPos = node.Graph.UpPointRect.center;
					var center = mBzStartPos.x + (mBzEndPos.x - mBzStartPos.x) / 2;
					Handles.DrawBezier(mBzStartPos, mBzEndPos, new Vector3(center, mBzStartPos.y),
						new Vector3(center, mBzEndPos.y), BtConst.LineColor, Texture2D.whiteTexture,
						BtConst.BezierSize);
					GUI.Label(node.Graph.UpPointRect, BtNodeStyle.LinePoint);
				}
			}

			if (mIsLinkParent)
			{
				var startPos = Graph.UpPointRect.center;
				var endPos = BtEditorWindow.Window.Event.mousePosition;
				var center = startPos.x + (endPos.x - startPos.x) / 2;
				Handles.DrawBezier(startPos, endPos,
					new Vector3(center, startPos.y), new Vector3(center, endPos.y),
					BtConst.LineColor, Texture2D.whiteTexture, BtConst.BezierSize * BtEditorWindow.Scale);
				//Handles.DrawLine (startPos, endPos);
			}

			if (!IsRoot && !IsHaveParent)
				GUI.Label(Graph.UpPointRect, BtNodeStyle.ErrorPoint);

			if (NodeType.CanAddNodeCount > 0)
			{
				GUI.Label(Graph.DownPointRect,
					NodeType.IsValid == ErrorType.Error ? BtNodeStyle.ErrorPoint : BtNodeStyle.LinePoint);
				if (Data.fold) GUI.Label(Graph.DownPlusRect, BtNodeStyle.FoldoutPlus);
			}

			GUIStyle style;
			if (Data.enabled) style = IsSelected ? NodeType.SelectStyle : NodeType.NormalStyle;
			else style = IsSelected ? BtNodeStyle.SelectRootStyle : BtNodeStyle.RootStyle;
			var showLabel = Data.name;
			if (!BtEditorWindow.IsDebug) showLabel = $"\n{showLabel}";
			else
			{
				if (Data.data == null) showLabel = $"\n{showLabel}";
				else if (Data.data.Count == 1)
				{
					var first = Data.data.First();
					showLabel = $"{showLabel}\n{first.Key}:{first.Value}";
				}
				else if (Data.data.Count >= 2)
				{
					var i = 0;
					foreach (var data in Data.data)
					{
						if (i < 2) showLabel = $"{showLabel}\n{data.Key}:{data.Value}";
						else break;
						i++;
					}
				}
			}

			GUI.Box(Graph.NodeRect, "", style);
			var icon = NodeType.GetIcon();
			if (icon == null) GUI.Label(Graph.NodeRect, showLabel, BtEditorWindow.LabelStyle);
			else GUI.DrawTexture(Graph.IconRect, icon);

			if (NodeType.Type == TaskType.Selector || NodeType.Type == TaskType.Sequence)
			{
				if (Data.data != null && Data.data.TryGetValue(BtConst.AbortType, out var value))
				{
					Enum.TryParse<AbortType>(value, out var abortType);
					switch (abortType)
					{
						case AbortType.Lower:
							GUI.DrawTexture(Graph.AbortTypeRect, BtNodeStyle.AbortLowerLogo);
							break;
						case AbortType.Self:
							GUI.DrawTexture(Graph.AbortTypeRect, BtNodeStyle.AbortSelfLogo);
							break;
						case AbortType.Both:
							GUI.DrawTexture(Graph.AbortTypeRect, BtNodeStyle.AbortBothLogo);
							break;
					}
				}
			}

			if (BtEditorWindow.IsShowPos)
				GUI.Label(Graph.PosRect, new GUIContent($"{Graph.RealRect.x},{Graph.RealRect.y}"));
			if (BtEditorWindow.IsShowIndex)
				GUI.Label(Graph.IndexRect, Data.index.ToString(), BtNodeStyle.IndexStyle);


			if (!IsRoot && BtEditorWindow.IsDebug || !Data.isOn)
			{
				Data.isOn = GUI.Toggle(Graph.EnabledRect, Data.isOn, "");
				if (Data.IsChangeToggle(Data.isOn))
					SetNodeEnabled(this, Data.isOn && Parent.Data.enabled);
			}
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles(Rect canvas)
		{
			var window = BtEditorWindow.Window;
			var curEvent = window.Event;
			if (curEvent.mousePosition.x >= canvas.width - BtConst.InspectWidth) { }
			//拖拽
			else if (curEvent.type == EventType.MouseDrag && curEvent.button == 0)
			{
				if (Graph.NodeRect.Contains(curEvent.mousePosition) && mCanDragMove)
				{
					curEvent.Use();
					mIsDragging = true;
					window.CurSelectNode = this;
					GUIUtility.keyboardControl = 0;
					var scale = BtEditorWindow.Scale;
					var delta = BtEditorWindow.IsLockAxisY
						? new Vector2(curEvent.delta.x / scale, 0)
						: curEvent.delta * scale;
					UpdateNodePosition(this, delta);
				}
			}
			//点击
			else if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
			{
				if (Graph.NodeRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					window.CurSelectNode = this;
					GUIUtility.keyboardControl = 0;
					mCanDragMove = true;
				}
				else if (Graph.UpPointRect.Contains(curEvent.mousePosition))
				{
					if (IsRoot) return;
					curEvent.Use();
					if (IsHaveParent)
					{
						Parent.ChildNodeList.Remove(this);
						Parent.Data.children.Remove(Data);
						Owner.AddBrokenNode(this);
						Parent = null;
					}
					else
					{
						window.CurSelectNode = this;
						mIsLinkParent = true;
					}
				}
				else if (Graph.DownPointRect.Contains(curEvent.mousePosition))
				{
					if (!IsHaveChild) return;
					curEvent.Use();
					Data.fold = !Data.fold;
					SetNodeVisible(this, !Data.fold);
					Data.visable = true; //自己还是要显示
				}
				// else window.CurSelectNode = null;
			}
			//松开鼠标
			else if (curEvent.type == EventType.MouseUp && curEvent.button == 0)
			{
				if (mIsDragging)
				{
					curEvent.Use();
					mIsDragging = false;
					if (BtEditorWindow.IsAutoAlign)
						SetNodePosition(this);
				}

				if (mIsLinkParent)
				{
					mIsLinkParent = false;
					var parent = window.GetMouseTriggerDownPoint(curEvent.mousePosition);
					if (parent != null && parent != this &&
					    parent.ChildNodeList.Count < parent.NodeType.CanAddNodeCount)
					{
						parent.ChildNodeList.Add(this);
						parent.Data.AddChild(Data);
						Owner.RemoveBrokenNode(this);
						Parent = parent;
					}
				}

				mCanDragMove = false;
			}
			else if (curEvent.type == EventType.ScrollWheel)
			{
				var y = curEvent.delta.y;
				if (y != 0)
				{
					BtEditorWindow.Scale = BtEditorWindow.Scale -= y / 100f;
					curEvent.Use();
				}
			}
			else if (curEvent.type == EventType.ContextClick)
			{
				if (Graph.NodeRect.Contains(curEvent.mousePosition))
				{
					//显示右键菜单
					ShowMenu();
				}
			}
		}

		private void SetNodeEnabled(BtNode node, bool enabled)
		{
			node.Data.enabled = enabled && node.Data.isOn;
			if (!node.IsHaveChild) return;
			foreach (var child in node.ChildNodeList)
				SetNodeEnabled(child, enabled && node.Data.isOn);
		}

		private void SetNodeVisible(BtNode node, bool visible)
		{
			node.Data.visable = visible;
			if (!node.IsHaveChild) return;
			foreach (var child in node.ChildNodeList)
				SetNodeVisible(child, visible && !node.Data.fold);
		}

		private void UpdateNodePosition(BtNode node, Vector2 delta)
		{
			node.Graph.RealRect.position += delta;
			if (!node.IsHaveChild) return;
			foreach (var child in node.ChildNodeList)
				UpdateNodePosition(child, delta);
		}

		private void SetNodePosition(BtNode node)
		{
			BtHelper.AutoAlignPosition(node);
			if (!node.IsHaveChild) return;
			foreach (var child in node.ChildNodeList)
				SetNodePosition(child);
		}

		private void Callback(object obj)
		{
			var name = obj.ToString();
			switch (name)
			{
				case "Delete":
					BtHelper.RemoveChild(this);
					break;
				case "Copy":
					BtEditorWindow.CopyNode = this;
					break;
				case "Paste":
					var clone = BtHelper.PasteChild(Owner, this, Data.posX, Data.posY + BtConst.DefaultHeight);
					BtEditorWindow.Window.CurSelectNode = clone;
					Owner.AddBrokenNode(clone);
					break;
				default:
					var node = BtHelper.AddChildNode(Owner, this, name);
					BtHelper.SetNodeDefaultData(node, name);
					break;
			}
		}

		private void ShowMenu()
		{
			var menu = BtHelper.GetGenericMenu(this, Callback);
			menu.ShowAsContext();
		}
	}

	public class BehaviourTree
	{
		public Dictionary<string, BtNode> NodeDict;
		public Dictionary<string, BtNode> BrokenNodeDict;
		public Dictionary<string, int> NodePosDict;

		public string Name { get; set; }

		public BtNode Root { get; }

		private EditorWindow WinHandle { get; }

		public BehaviourTree(EditorWindow win)
		{
			WinHandle = win;
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
		}

		public BehaviourTree(EditorWindow win, string name, BtNodeData data = null)
		{
			WinHandle = win;
			Name = name;
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
			BrokenNodeDict = new Dictionary<string, BtNode>();
			if (data == null)
			{
				data = new BtNodeData(BtConst.RootName, string.Empty,
					(BtEditorWindow.Window.position.width - BtConst.InspectWidth) / 2, 50);
				data.AddData(BtConst.Restart, "1");
			}

			Root = new BtNode(this, null, data);
			AddNode(Root);
			BtHelper.AutoAlignPosition(Root);
		}

		public void Update(Rect canvas)
		{
			if (NodeDict == null) return;
			foreach (var node in NodeDict.Values)
			{
				node.Update(canvas);
			}
		}

		public void AddNode(BtNode node)
		{
			NodeDict.Add(node.Guid, node);
			//用于判断是否重叠
			var key = node.Data.GetPosition().ToString();
			if (NodePosDict.TryGetValue(key, out var count))
				NodePosDict[key] = count + 1;
			else
				NodePosDict.Add(key, 1);
			// WinHandle.Repaint();
		}

		public void RemoveNode(BtNode node)
		{
			NodeDict.Remove(node.Guid);
			BrokenNodeDict.Remove(node.Guid);
			NodePosDict.Remove(node.Data.GetPosition().ToString());
			// WinHandle.Repaint();
		}

		public void AddBrokenNode(BtNode node)
		{
			BrokenNodeDict.Add(node.Guid, node);
			WinHandle.Repaint();
		}

		public void RemoveBrokenNode(BtNode node)
		{
			BrokenNodeDict.Remove(node.Guid);
			// WinHandle.Repaint();
		}

		public Vector2 GenNodePos(Vector2 pos)
		{
			while (true)
			{
				if (NodePosDict.ContainsKey(pos.ToString()))
				{
					pos.y += (BtConst.DefaultHeight + BtConst.DefaultSpacingY) / 2f;
					continue;
				}

				NodePosDict.Add(pos.ToString(), 1);
				return pos;
			}
		}
	}

	public class BtNodeGraph
	{
		/// <summary>
		/// 实际节点
		/// </summary>
		public Rect RealRect;

		/// <summary>
		/// 节点范围
		/// </summary>
		public Rect NodeRect
		{
			get
			{
				var ret = RealRect;
				ret.position += BtEditorWindow.Position;
				ret.position *= BtEditorWindow.Scale;
				ret.size *= BtEditorWindow.Scale;
				return ret;
			}
		}

		/// <summary>
		/// 图标位置
		/// </summary>
		public Rect IconRect
		{
			get
			{
				var rect = NodeRect;
				var w = BtConst.IconSize * BtEditorWindow.Scale;
				var x = rect.x + (rect.width - w) / 2;
				var y = rect.y + (rect.height - w) / 2;
				return new Rect(x, y, w, w);
			}
		}

		/// <summary>
		/// 下部连接点
		/// </summary>
		public Rect DownPointRect
		{
			get
			{
				var w = BtConst.LinePointLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.center.x - w / 2, NodeRect.yMax, w, w);
			}
		}

		/// <summary>
		/// 下部连接点
		/// </summary>
		public Rect DownPlusRect
		{
			get
			{
				var w = BtConst.LinePlusLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.center.x - w / 2, NodeRect.yMax + 4, w, w);
			}
		}

		/// <summary>
		/// 上部连接点
		/// </summary>
		public Rect UpPointRect
		{
			get
			{
				var w = BtConst.LinePointLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.center.x - w / 2, NodeRect.yMin - w, w, w);
			}
		}

		/// <summary>
		/// 错误节点范围
		/// </summary>
		public Rect ErrorRect
		{
			get
			{
				var w = BtConst.LinePointLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.x + 5, NodeRect.y + 5, w, w);
			}
		}

		/// <summary>
		/// 左上显示区
		/// </summary>
		public Rect PosRect
		{
			get
			{
				var scale = BtEditorWindow.Scale;
				var x = BtConst.DefaultSpacingX * scale;
				var w = BtConst.DefaultWidth * scale;
				var h = BtConst.DefaultHeight * scale;
				return new Rect(NodeRect.xMin + x, NodeRect.yMin - h / 2f, w, h);
			}
		}


		/// <summary>
		/// 索引显示区
		/// </summary>
		public Rect IndexRect
		{
			get
			{
				var w = BtConst.LinePointLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.x - w / 2, NodeRect.y - 8, w, w);
			}
		}

		/// <summary>
		/// 符合节点打断类型显示区
		/// </summary>
		public Rect AbortTypeRect
		{
			get
			{
				var w = BtConst.LinePointLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.xMin, NodeRect.yMin, w, w);
			}
		}

		/// <summary>
		/// 启用节点显示区
		/// </summary>
		public Rect EnabledRect
		{
			get
			{
				var w = BtConst.ToggleLength * BtEditorWindow.Scale;
				return new Rect(NodeRect.xMax - w + 2, NodeRect.yMin, w, w);
			}
		}
	}
}