using UnityEngine;
using UnityEditor;

namespace BT
{
	public partial class BTEditorWindow : EditorWindow
	{
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