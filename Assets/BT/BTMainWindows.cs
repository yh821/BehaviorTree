using UnityEngine;
using UnityEditor;

namespace BT
{
	public class BTMainWindows : EditorWindow
	{
		[MenuItem ("Tools/BehaviorEditor")]
		static void AddWindow ()
		{
			var win = GetWindowWithRect<BTMainWindows> (
				new Rect (0, 0, BTEditorConst.WINDOWS_WIDTH, BTEditorConst.WINDOWS_HEIGHT), true, "编辑行为树");
			win.Initialize ();
		}

		private BTEditorProperty _mBtEditorAttribute;
		private BehaviourTree _mBehaviourTree;

		[HideInInspector] public BTGrid BTGrid;

		public void Initialize ()
		{
			if (_mBtEditorAttribute == null) {
				_mBtEditorAttribute = new BTEditorProperty ();
				BTEditorProperty.Instance = _mBtEditorAttribute;
			}

			if (_mBehaviourTree == null) {
				_mBehaviourTree = new BehaviourTree (); 
				_mBehaviourTree.CreateRoot ();
			}

			if (BTGrid == null) {
				BTGrid = new BTGrid ();
			}
		}

		void DrawInspector ()
		{
			EditorGUILayout.HelpBox ("节点信息", MessageType.Info);
		}

		void OnGUI ()
		{
			GUILayout.EndHorizontal ();
			//BTGrid.DrawGrid (position.size);
			GUILayout.BeginVertical ();
			{
				//_mBehaviourTree.Update (position);
			}
			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			{
				DrawInspector ();
			}
			GUILayout.EndVertical ();
			GUILayout.EndVertical ();
		}
	}
}