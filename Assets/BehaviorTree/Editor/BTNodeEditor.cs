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

		private BtGrid BtGrid;

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

		private void OnGUI()
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
				{
					mNodeInspectorRect = new Rect(position.width - BtConst.RightInspectWidth, 0,
						BtConst.RightInspectWidth, BtConst.RightInspectHeight);
					GUILayout.Window(0, mNodeInspectorRect, NodeInspectorWindow, "Inspector");
				}
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

		#region NodeInspector

		private const int SPACE_VALUE = 10;
		private const int BTN_ICON_WIDTH = 28;
		private const string DEFAULE_BT_NAME = "新建行为树";

		private static string[] TAB =
		{
			"Node Inspector",
			"Node Option",
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
		public static bool IsShowPos = false;
		public static bool IsShowIndex = false;
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
				IsDebug = GUILayout.Toggle(IsDebug, "调试");
				// GUI.enabled = IsDebug;
				IsAutoAlign = GUILayout.Toggle(IsAutoAlign, "自动对齐");
				IsShowPos = GUILayout.Toggle(IsShowPos, "坐标");
				IsShowIndex = GUILayout.Toggle(IsShowIndex, "序号");
				// IsLockAxisY = GUILayout.Toggle(IsLockAxisY, "锁定Y轴");
				// GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(SPACE_VALUE);
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("JsonBT目录"))
				{
					System.Diagnostics.Process.Start(BtHelper.JsonPath);
				}

				if (GUILayout.Button("LuaBT目录"))
				{
					System.Diagnostics.Process.Start(BtHelper.BehaviorPath);
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
				var file = Path.Combine(BtHelper.JsonPath, $"{fileName}.json");
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
						var filePath = Path.Combine(BtHelper.JsonPath, $"{fileName}.json");
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
						BtHelper.SaveBtData(mBehaviourTree);
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
					EditorGUILayout.LabelField("节点数据", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
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
					if (IsDebug)
						EditorGUILayout.LabelField("节点名:", data.file);
					if (node.Guid != mLastNodeGuid)
					{
						mLastNodeGuid = node.Guid;
						mCurChangeDict.Clear();
					}

					switch (node.NodeType.Type)
					{
						case TaskType.Composite:
							break;
						case TaskType.Root:
							DrawRootInspector(data);
							break;
						case TaskType.Abort:
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
				}
				EditorGUILayout.EndVertical();
			}
		}

		private void LoadBehaviorTree()
		{
			var files = Directory.GetFiles(BtHelper.JsonPath, "*.json", SearchOption.AllDirectories);
			mAllShowJsons = files.Select(Path.GetFileNameWithoutExtension).ToArray();
			if (mCurSelectJson >= mAllShowJsons.Length)
				mCurSelectJson = mAllShowJsons.Length - 1;
			mLastSelectJson = -1;
		}

		private void DrawRootInspector(BtNodeData data)
		{
			var reStart = false;
			if (data.data != null && data.data.TryGetValue("restart", out var value))
				reStart = value == "1";
			reStart = EditorGUILayout.Toggle("restart", reStart);
			data.AddData("restart", reStart ? "1" : "");
		}

		private void DrawAbortInspector(BtNodeData data)
		{
			var abortType = AbortType.None;
			if (data.data != null && data.data.TryGetValue("abort", out var value))
				Enum.TryParse(value, out abortType);
			abortType = (AbortType) EditorGUILayout.EnumPopup("abortType", abortType);
			data.AddData("abort", abortType.ToString());
		}

		private void DrawTriggerInspector(BtNodeData data)
		{
			var type = TriggerType.Equals;
			if (data.data != null && data.data.TryGetValue("triggerType", out var typeStr))
				Enum.TryParse(typeStr, out type);
			type = (TriggerType) EditorGUILayout.EnumPopup("triggerType", type);
			data.AddData("triggerType", type.ToString());

			var value = 0;
			if (data.data != null && data.data.TryGetValue("triggerValue", out var valueStr))
				int.TryParse(valueStr, out value);
			value = EditorGUILayout.IntField("triggerValue", value);
			data.AddData("triggerValue", value.ToString());
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
				data.AddData(key, mCurChangeDict[key]);

			mCurChangeDict.Clear();

			if (data.data != null)
				foreach (var kv in data.data)
					DrawItemInspector(kv);

			EditorGUILayout.BeginHorizontal();
			{
				mCurKey = EditorGUILayout.TextField("key:", mCurKey);
				mCurValue = EditorGUILayout.TextField("val:", mCurValue);
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(BTN_ICON_WIDTH)))
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

		private void DrawItemInspector(KeyValuePair<string, string> kv)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("key:", kv.Key);
				mCurChangeDict[kv.Key] = EditorGUILayout.TextField("val:", kv.Value, GUILayout.MaxWidth(80));
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(BTN_ICON_WIDTH)))
				{
					mCurDelKey = kv.Key;
					GUIUtility.keyboardControl = 0;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region NodeOption

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
				mOptions = BtHelper.ReadBtNodeOption();
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
					EditorGUILayout.LabelField("修改配置: " + mSelectNode);

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
							data.Add("name", "");
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
					GUIUtility.keyboardControl = 0;
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
					mDelNode = key;
			}
			EditorGUILayout.EndHorizontal();
		}

		#endregion
	}

	public class BtGrid
	{
		private readonly Texture _background;

		public BtGrid()
		{
			var path = BtHelper.ToolPath + "/GUI/background.png";
			path = FileUtil.GetProjectRelativePath(path);
			_background = AssetDatabase.LoadAssetAtPath<Texture>(path);
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
				BtEditorWindow.Window.Position += currentEvent.delta;
			}
		}

		private void DrawBackground(Vector2 windowSize)
		{
			var position = BtEditorWindow.Window.Position;
			var rect = new Rect(0, 0, windowSize.x, windowSize.y);
			var texCoords = new Rect(-position.x / _background.width,
				(1.0f - windowSize.y / _background.height) + position.y / _background.height,
				windowSize.x / _background.width,
				windowSize.y / _background.height);
			GUI.DrawTextureWithTexCoords(rect, _background, texCoords);
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

		public List<BtNode> ChildNodeList;

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
			if (IsHaveChild && !Data.fold)
			{
				mBzStartPos = Graph.DownPointRect.center;
				foreach (var node in ChildNodeList)
				{
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
				Handles.DrawBezier(startPos, endPos, new Vector3(center, startPos.y),
					new Vector3(center, endPos.y), BtConst.LineColor, Texture2D.whiteTexture, BtConst.BezierSize);
				//Handles.DrawLine (startPos, endPos);
			}

			if (!IsRoot && !IsHaveParent)
				GUI.Label(Graph.UpPointRect, BtNodeStyle.ErrorPoint);

			if (NodeType.CanAddNodeCount > 0)
				GUI.Label(Graph.DownPointRect,
					NodeType.IsValid == ErrorType.Error ? BtNodeStyle.ErrorPoint : BtNodeStyle.LinePoint);

			GUIStyle style;
			if (IsSelected)
				style = Data.fold ? NodeType.FoldSelectStyle : NodeType.SelectStyle;
			else
				style = Data.fold ? NodeType.FoldNormalStyle : NodeType.NormalStyle;

			var showLabel = Data.name;
			if (!BtEditorWindow.IsDebug)
				showLabel = $"\n{showLabel}";
			else
			{
				if (Data.data == null)
				{
					showLabel = $"\n{showLabel}";
				}
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
						if (i < 2)
							showLabel = $"{showLabel}\n{data.Key}:{data.Value}";
						else
							break;
						i++;
					}
				}
			}

			var icon = NodeType.GetIcon();
			if (icon == null)
				GUI.Label(Graph.NodeRect, showLabel, style);
			else
			{
				GUI.Label(Graph.NodeRect, "", style);
				GUI.DrawTexture(Graph.IconRect, icon);
			}

			if (NodeType.Type == TaskType.Abort)
			{
				if (Data.data != null && Data.data.TryGetValue("abort", out var value))
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
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles(Rect canvas)
		{
			var window = BtEditorWindow.Window;
			var curEvent = window.Event;
			//拖拽
			if (curEvent.type == EventType.MouseDrag && curEvent.button == 0)
			{
				if (Graph.NodeRect.Contains(curEvent.mousePosition) && mCanDragMove)
				{
					curEvent.Use();
					mIsDragging = true;
					window.CurSelectNode = this;
					GUIUtility.keyboardControl = 0;
					var delta = BtEditorWindow.IsLockAxisY ? new Vector2(curEvent.delta.x, 0) : curEvent.delta;
					UpdateNodePosition(this, delta);
				}
			}
			//点击
			else if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
			{
				if (curEvent.mousePosition.x >= canvas.width - BtConst.RightInspectWidth)
				{
					//window.CurSelectNode = null;
				}
				else if (Graph.UpPointRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					if (!IsRoot)
					{
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
				}
				else if (Graph.DownPointRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					if (IsHaveChild)
						Data.fold = !Data.fold;
				}
				else if (Graph.NodeRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					window.CurSelectNode = this;
					GUIUtility.keyboardControl = 0;
					mCanDragMove = true;
				}
				else
				{
					window.CurSelectNode = null;
				}
			}
			//松开鼠标
			else if (curEvent.type == EventType.MouseUp && curEvent.button == 0)
			{
				if (mIsDragging)
				{
					curEvent.Use();
					mIsDragging = false;
					if (BtEditorWindow.IsAutoAlign)
					{
						SetNodePosition(this);
					}
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
			else if (curEvent.type == EventType.ContextClick)
			{
				if (Graph.NodeRect.Contains(curEvent.mousePosition))
				{
					//显示右键菜单
					ShowMenu();
				}
			}
		}

		private void UpdateNodePosition(BtNode parent, Vector2 delta)
		{
			parent.Graph.RealRect.position += delta;
			if (parent.IsHaveChild)
			{
				foreach (var node in parent.ChildNodeList)
				{
					UpdateNodePosition(node, delta);
				}
			}
		}

		private void SetNodePosition(BtNode parent)
		{
			BtHelper.AutoAlignPosition(parent);
			if (parent.IsHaveChild)
			{
				foreach (var node in parent.ChildNodeList)
				{
					SetNodePosition(node);
				}
			}
		}

		public void Callback(object obj)
		{
			var name = obj.ToString();
			if (name == "Delete")
				BtHelper.RemoveChild(this);
			else if (name == "Copy")
				BtEditorWindow.CopyNode = this;
			else if (name == "Paste")
				BtHelper.PasteChild(Owner, this, Data.posX, Data.posY + BtConst.DefaultHeight);
			else
			{
				var node = BtHelper.AddChildNode(Owner, this, name);
				BtHelper.SetNodeDefaultData(node, name);
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

		public BehaviourTree()
		{
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
		}

		public BehaviourTree(string name, BtNodeData data = null)
		{
			Name = name;
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
			BrokenNodeDict = new Dictionary<string, BtNode>();
			if (data == null)
			{
				data = new BtNodeData(BtConst.RootName, string.Empty,
					(BtEditorWindow.Window.position.width - BtConst.RightInspectWidth) / 2, 50);
				data.AddData("restart", "1");
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
		}

		public void RemoveNode(BtNode node)
		{
			NodeDict.Remove(node.Guid);
			BrokenNodeDict.Remove(node.Guid);
			NodePosDict.Remove(node.Data.GetPosition().ToString());
		}

		public void AddBrokenNode(BtNode node)
		{
			BrokenNodeDict.Add(node.Guid, node);
		}

		public void RemoveBrokenNode(BtNode node)
		{
			BrokenNodeDict.Remove(node.Guid);
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
				ret.position += BtEditorWindow.Window.Position;
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
				var w = BtConst.IconSize;
				var x = rect.x + (rect.width - w) / 2;
				var y = rect.y + (rect.height - w) / 2;
				return new Rect(x, y, w, w);
			}
		}

		/// <summary>
		/// 下部连接点
		/// </summary>
		public Rect DownPointRect =>
			new Rect(NodeRect.center.x - BtConst.LinePointLength / 2, NodeRect.yMax,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 上部连接点
		/// </summary>
		public Rect UpPointRect =>
			new Rect(NodeRect.center.x - BtConst.LinePointLength / 2,
				NodeRect.yMin - BtConst.LinePointLength,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 错误节点范围
		/// </summary>
		public Rect ErrorRect =>
			new Rect(NodeRect.x + 5, NodeRect.y + 5,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 左上显示区
		/// </summary>
		public Rect PosRect =>
			new Rect(NodeRect.xMin + BtConst.DefaultSpacingX, NodeRect.yMin - BtConst.DefaultHeight / 2f,
				BtConst.DefaultWidth, BtConst.DefaultHeight);


		/// <summary>
		/// 索引显示区
		/// </summary>
		public Rect IndexRect =>
			new Rect(NodeRect.x - BtConst.LinePointLength / 2, NodeRect.y - 8,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 符合节点打断类型显示区
		/// </summary>
		public Rect AbortTypeRect => new Rect(NodeRect.x, NodeRect.y, BtConst.LinePointLength, BtConst.LinePointLength);
	}
}