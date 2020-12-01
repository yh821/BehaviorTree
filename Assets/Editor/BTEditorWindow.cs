using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BTEditorWindow : EditorWindow
	{
		#region Property
		[MenuItem ("Tools/BehaviorEditor")]
		static void ShowWindow ()
		{
			var win = GetWindowWithRect<BTEditorWindow> (
				new Rect (0, 0, BTConst.WINDOWS_WIDTH, BTConst.WINDOWS_HEIGHT), false, "编辑行为树");
			win.Initialize ();
		}

		private static BTEditorWindow mWindow = null;

		public static BTEditorWindow window {
			get { 
				if (mWindow == null) {
					mWindow = GetWindowWithRect<BTEditorWindow> (new Rect (0, 0, BTConst.WINDOWS_WIDTH, BTConst.WINDOWS_HEIGHT), false, "行为树编辑器");
				}
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
				GUILayout.BeginVertical (GUILayout.MaxWidth (BTConst.LEFT_INSPECT_WIDTH));
				{
					DrawNodeInspector ();
				}
				GUILayout.EndVertical ();
			}
			GUILayout.EndHorizontal ();
		}
	}
}