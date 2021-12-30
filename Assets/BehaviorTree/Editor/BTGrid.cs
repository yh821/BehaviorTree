using UnityEngine;
using UnityEditor;

namespace BT
{
	public class BtGrid
	{
		private readonly Texture mBackground;

		public BtGrid()
		{
			var path = BtHelper.ModulePath(false) + "/Editor/GUI/background.png";
			mBackground = AssetDatabase.LoadAssetAtPath<Texture>(path);
		}

		/// <summary>
		/// 绘制背景格子
		/// </summary>
		/// <param name="windowSize"></param>
		public void DrawGrid(Vector2 windowSize)
		{
			Handles();
			var position = BtEditorWindow.Window.Position;
			var rect = new Rect(0, 0, windowSize.x, windowSize.y);
			var texCoords = new Rect(-position.x / mBackground.width,
				(1.0f - windowSize.y / mBackground.height) + position.y / mBackground.height,
				windowSize.x / mBackground.width,
				windowSize.y / mBackground.height);
			GUI.DrawTextureWithTexCoords(rect, mBackground, texCoords);
		}

		/// <summary>
		/// 拖拽背景
		/// </summary>
		public void Handles()
		{
			var currentEvent = BtEditorWindow.Window.Event;
			if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 1)
			{
				currentEvent.Use();
				BtEditorWindow.Window.Position += currentEvent.delta;
			}
		}
	}
}