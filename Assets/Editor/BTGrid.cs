using UnityEngine;
using UnityEditor;

namespace BT
{
	public class BTGrid
	{
		private Texture mBackground;

		public BTGrid ()
		{
			mBackground = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/BT/EditorGUI/background.png");
		}

		/// <summary>
		/// 绘制背景格子
		/// </summary>
		/// <param name="windowSize"></param>
		public void DrawGrid (Vector2 windowSize)
		{
			Handles ();
			BTEditorProperty editor = BTEditorProperty.Instance;
			Rect position = new Rect (0, 0, windowSize.x, windowSize.y);
			Rect texCoords = new Rect (-editor.Position.x / mBackground.width,
				                (1.0f - windowSize.y / mBackground.height) + editor.Position.y / mBackground.height,
				                windowSize.x / mBackground.width,
				                windowSize.y / mBackground.height);
			GUI.DrawTextureWithTexCoords (position, mBackground, texCoords);
		}

		/// <summary>
		/// 拖拽背景
		/// </summary>
		public void Handles ()
		{
			Event currentEvent = BTEditorProperty.Instance.Event;
			if (currentEvent.isMouse && currentEvent.button == 2 && currentEvent.type == EventType.MouseDrag) {
				currentEvent.Use ();
				BTEditorProperty.Instance.Position += currentEvent.delta;                
			}
		}
	}
}