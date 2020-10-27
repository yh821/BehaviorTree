using UnityEditor;
using UnityEngine;

namespace BT
{
	public static class BTNodeStyle
	{
		public static GUIStyle NormalStyle {
			get { return "flow node 1"; }
		}

		public static GUIStyle SelectStyle {
			get { return "flow node 1 on"; }
		}

		private static Texture _LineBoxTex;

		public static Texture LineBoxTex {
			get {
				if (_LineBoxTex == null) {
					_LineBoxTex = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/BT/EditorGUI/Icon_ModalBox_Cross.png");
				}

				return _LineBoxTex;
			}
		}

		private static Texture _LinePoint;

		public static Texture LinePoint {
			get {
				if (_LinePoint == null) {
					_LinePoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/BT/EditorGUI/Minimap_Pin_Green.png");
				}

				return _LinePoint;
			}
		}

		private static Texture _ErrorPoint;

		public static Texture ErrorPoint {
			get {
				if (_ErrorPoint == null) {
					_ErrorPoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/BT/EditorGUI/Minimap_Pin_Red.png");
				}

				return _ErrorPoint;
			}
		}

		private static Texture _WarnPoint;

		public static Texture WarnPoint {
			get {
				if (_WarnPoint == null) {
					_WarnPoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/BT/EditorGUI/Minimap_Pin_Yellow.png");
				}

				return _WarnPoint;
			}
		}

		private static GUIStyle style;
	}
}