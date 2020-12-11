using UnityEngine;
using UnityEditor;

namespace BT
{
	public class BTGrid
	{
		private Texture mBackground;

		public BTGrid ()
		{
			mBackground = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Editor/GUI/background.png");
		}

		/// <summary>
		/// 绘制背景格子
		/// </summary>
		/// <param name="windowSize"></param>
		public void DrawGrid (Vector2 windowSize)
		{
			Handles ();
			var position = BTEditorWindow.window.Position;
			Rect rect = new Rect (0, 0, windowSize.x, windowSize.y);
			Rect texCoords = new Rect (-position.x / mBackground.width,
				                 (1.0f - windowSize.y / mBackground.height) + position.y / mBackground.height,
				                 windowSize.x / mBackground.width,
				                 windowSize.y / mBackground.height);
			GUI.DrawTextureWithTexCoords (rect, mBackground, texCoords);
		}

		/// <summary>
		/// 拖拽背景
		/// </summary>
		public void Handles ()
		{
			Event currentEvent = BTEditorWindow.window.Event;
			if (currentEvent.isMouse && currentEvent.button == 1 && currentEvent.type == EventType.MouseDrag) {
				currentEvent.Use ();
				BTEditorWindow.window.Position += currentEvent.delta;                
			}
		}
	}
}