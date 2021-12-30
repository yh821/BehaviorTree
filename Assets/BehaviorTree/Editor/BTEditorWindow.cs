using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BtEditorWindow : EditorWindow
	{
		#region Property

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

		public BtNode CurSelectNode { get; set; }

		#endregion

		[HideInInspector] public BtGrid BtGrid;

		private BehaviourTree mBehaviourTree;

		public void Initialize()
		{
			if (mBehaviourTree == null)
				mBehaviourTree = new BehaviourTree();

			if (BtGrid == null)
				BtGrid = new BtGrid();

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
				GUILayout.BeginVertical(GUILayout.MaxWidth(BtConst.RightInspectWidth));
				{
					DrawNodeInspector();
				}
				GUILayout.EndVertical();
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
				if (node.BtNodeGraph.DownPointRect.Contains(mousePos))
					return node;
			}

			return null;
		}
	}
}