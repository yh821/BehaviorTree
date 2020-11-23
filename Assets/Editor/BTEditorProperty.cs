using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BTEditorWindow
	{
		[MenuItem ("Tools/BehaviorEditor")]
		static void ShowWindow ()
		{
			var win = GetWindowWithRect<BTEditorWindow> (
				          new Rect (0, 0, BTConst.WINDOWS_WIDTH, BTConst.WINDOWS_HEIGHT), true, "编辑行为树");
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
	}
}