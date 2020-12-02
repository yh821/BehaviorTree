using UnityEditor;
using UnityEngine;

namespace BT
{
	public static class BTNodeStyle
	{
		public static GUIStyle RootStyle {
			get { return "flow node 0"; }
		}

		public static GUIStyle SelectRootStyle {
			get { return "flow node 0 on"; }
		}

		public static GUIStyle DecoratorStyle {
			get { return "flow node 2"; }
		}

		public static GUIStyle SelectDecoratorStyle {
			get { return "flow node 2 on"; }
		}

		public static GUIStyle CompositeStyle {
			get { return "flow node 1"; }
		}

		public static GUIStyle SelectCompositeStyle {
			get { return "flow node 1 on"; }
		}

		public static GUIStyle TaskStyle {
			get { return "flow node 3"; }
		}

		public static GUIStyle SelectTaskStyle {
			get { return "flow node 3 on"; }
		}


		private static Texture _XLinePoint;

		public static Texture XLinePoint {
			get {
				if (_XLinePoint == null) {
					_XLinePoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/GUI/Icon_ModalBox_Cross.png");
				}

				return _XLinePoint;
			}
		}

		private static Texture _LinePoint;

		public static Texture LinePoint {
			get {
				if (_LinePoint == null) {
					_LinePoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/GUI/Minimap_Pin_Green.png");
				}

				return _LinePoint;
			}
		}

		private static Texture _ErrorPoint;

		public static Texture ErrorPoint {
			get {
				if (_ErrorPoint == null) {
					_ErrorPoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/GUI/Minimap_Pin_Red.png");
				}

				return _ErrorPoint;
			}
		}

		private static Texture _WarnPoint;

		public static Texture WarnPoint {
			get {
				if (_WarnPoint == null) {
					_WarnPoint = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/GUI/Minimap_Pin_Yellow.png");
				}

				return _WarnPoint;
			}
		}

		private static GUIStyle style;
	}
}