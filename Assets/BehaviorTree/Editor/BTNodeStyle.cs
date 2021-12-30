using UnityEditor;
using UnityEngine;

namespace BT
{
	public static class BtNodeStyle
	{
		private static Texture _nodeEditorBg;

		public static Texture NodeEditorBg
		{
			get
			{
				if (_nodeEditorBg == null)
				{
					var path = BtHelper.ModulePath() + "/Editor/GUI/node_editor_bg.png";
					_nodeEditorBg = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}

				return _nodeEditorBg;
			}
		}

		public static GUIStyle RootStyle => "flow node 0";

		public static GUIStyle SelectRootStyle => "flow node 0 on";

		public static GUIStyle DecoratorStyle => "flow node 2";

		public static GUIStyle SelectDecoratorStyle => "flow node 2 on";

		public static GUIStyle CompositeStyle => "flow node 1";

		public static GUIStyle SelectCompositeStyle => "flow node 1 on";

		public static GUIStyle TaskStyle => "flow node 3";

		public static GUIStyle SelectTaskStyle => "flow node 3 on";

		private static GUIContent _LinePoint;
		public static GUIContent LinePoint => _LinePoint ??= EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo");

		private static GUIContent _WarnPoint;
		public static GUIContent WarnPoint => _WarnPoint ??= EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo");

		private static GUIContent _ErrorPoint;
		public static GUIContent ErrorPoint => _ErrorPoint ??= EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo");
	}
}