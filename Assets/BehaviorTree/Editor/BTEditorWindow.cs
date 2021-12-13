using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BTEditorWindow : EditorWindow
	{
		#region Property

		[MenuItem ("Tools/Behavior Editor %&d")]
		static void ShowWindow ()
		{
			mWindow = GetWindow<BTEditorWindow> ("行为树编辑器");
			mWindow.Initialize ();
		}

		private static BTEditorWindow mWindow = null;

		public static BTEditorWindow window {
			get { 
				if (mWindow == null)
					ShowWindow ();
				return mWindow; 
			}
		}

		/// <summary>
		/// 当前移动坐标 鼠标拖拽背景偏移
		/// </summary>
		public Vector2 Position { get { return mPosition; } set { mPosition = value; } }

		private Vector2 mPosition = Vector2.zero;

		public Event Event {
			get { 
				if (Event.current == null) {
					Event evt = new Event { type = EventType.Ignore };
					return evt;
				}
				return Event.current;
			}
		}

		public BTNode CurSelectNode { get; set; }

		#endregion

		[HideInInspector] public BTGrid BTGrid;

		private BehaviourTree mBehaviourTree;

		public void Initialize ()
		{
			if (mBehaviourTree == null) {
				mBehaviourTree = new BehaviourTree (); 
			}

			if (BTGrid == null) {
				BTGrid = new BTGrid ();
			}

			BTHelper.LoadNodeFile ();

			LoadBehaviorTree ();
		}

		void OnGUI ()
		{
			BTGrid.DrawGrid (position.size);
			GUILayout.BeginHorizontal ();
			{
				GUILayout.BeginVertical ();
				{
					mBehaviourTree.Update (position);
				}
				GUILayout.EndVertical ();
				GUILayout.BeginVertical (GUILayout.MaxWidth (BTConst.RIGHT_INSPECT_WIDTH));
				{
					DrawNodeInspector ();
				}
				GUILayout.EndVertical ();
			}
			GUILayout.EndHorizontal ();
		}

		public BTNode GetMouseTriggerDownPoint (Vector2 mousePos)
		{
			foreach (var node in mBehaviourTree.BTNodeDict.Values) {
				if (node.BTNodeGraph.DownPointRect.Contains (mousePos))
					return node;
			}
			return null;
		}
	}
}