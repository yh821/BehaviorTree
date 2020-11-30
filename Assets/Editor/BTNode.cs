using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BT
{
	public class BTNode
	{
		/// <summary>
		/// 编辑化节点
		/// </summary>
		public BTNodeType Type { get; }

		/// <summary>
		/// 唯一识别符
		/// </summary>
		public string Guid { get; }

		/// <summary>
		/// 节点名
		/// </summary>
		public string Label { get; }

		public string NodeName { get; }

		public BTNodeData Data { get; }

		public BehaviourTree Owner { get; }

		public BTNode Parent { get; }

		/// <summary>
		/// 节点Rect定义
		/// </summary>
		public BTNodeGraph BTNodeGraph;
		/// <summary>
		/// 子节点
		/// </summary>
		public List<BTNode> ChildNodeList;

		/// <summary>
		/// 是否拥有子节点
		/// </summary>
		public bool IsHaveChild {
			get { return ChildNodeList.Count > 0; }
		}

		/// <summary>
		/// 是否是根节点
		/// </summary>
		public bool IsRoot { get { return NodeName == BTConst.RootName; } }

		/// <summary>
		/// 是否可以拖拽
		/// </summary>
		private bool mCanDragMove = false;
		/// <summary>
		/// 是否拖拽中
		/// </summary>
		private bool mIsDragging = false;
		private Vector2 mDragDelta;

		private Vector3 startPos;
		private Vector3 endPos;

		public BTNode (BehaviourTree owner, BTNode parent, BTNodeData data)
		{
			Owner = owner;
			Parent = parent;
			Data = data;
			BTNodeGraph = new BTNodeGraph ();
			NodeName = data.name;
			Label = NodeName.Replace ("Node", "");
			BTNodeGraph.RealRect = new Rect (data.posX, data.posY, BTConst.DefaultWidth, BTConst.DefaultHeight);
			ChildNodeList = new List<BTNode> ();
			Guid = BTHelper.GenerateUniqueStringId ();
			Type = BTHelper.CreateNodeType (this);
		}

		public void Update (Rect canvas)
		{
			DrawNode ();
			DealHandles (canvas);
		}

		private void DrawNode ()
		{
			GUIStyle style = BTEditorWindow.window.CurSelectNode == this ? Type.SelectStyle : Type.NormalStyle;
			string showLabel;
			if (Data.data != null && Data.data.Count > 0) {
				var first = Data.data.First ();
				showLabel = string.Format ("{0}\n{1}:{2}", Label, first.Key, first.Value);
			} else
				showLabel = Label;

			EditorGUI.LabelField (BTNodeGraph.NodeRect, showLabel, style);

			if (IsHaveChild) {
				startPos = BTNodeGraph.DownPointRect.center;
				foreach (var node in ChildNodeList) {
					endPos = node.BTNodeGraph.UpPointRect.center;
					float center = startPos.x + (endPos.x - startPos.x) / 2;
					Handles.DrawBezier (startPos, endPos, new Vector3 (center, startPos.y), 
						new Vector3 (center, endPos.y), Color.white, Texture2D.whiteTexture, BTConst.BEZIER_WIDTH);
					GUI.DrawTexture (node.BTNodeGraph.UpPointRect, BTNodeStyle.LinePoint);
				}
				GUI.DrawTexture (BTNodeGraph.DownPointRect, BTNodeStyle.LinePoint);
			}

			if (Type.IsValid == ErrorType.Error)
				GUI.DrawTexture (BTNodeGraph.ErrorRect, BTNodeStyle.ErrorPoint);
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles (Rect canvas)
		{
			var window = BTEditorWindow.window;
			Event currentEvent = window.Event;
			if (currentEvent.isMouse && currentEvent.type == EventType.MouseDrag && currentEvent.button == 0) {
				//拖拽
				if (BTNodeGraph.NodeRect.Contains (currentEvent.mousePosition) && mCanDragMove) {
					mIsDragging = true;
					currentEvent.Use ();
					window.CurSelectNode = this;

					Vector2 delta;
					if (BTEditorWindow.IsLockAxisY)
						delta = new Vector2 (currentEvent.delta.x, 0);
					else
						delta = currentEvent.delta;

					mDragDelta += delta;

					SetRealPosition (this, delta, true);
				}
			} else if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) {
				//点击
				if (BTNodeGraph.NodeRect.Contains (currentEvent.mousePosition)) {
					window.CurSelectNode = this;
					mCanDragMove = true;
					currentEvent.Use ();
				} else {
					if (currentEvent.mousePosition.x <= canvas.width - BTConst.LEFT_INSPECT_WIDTH) {
						window.CurSelectNode = null;
					}
				}
			} else if (currentEvent.isMouse && currentEvent.type == EventType.MouseUp && currentEvent.button == 0) {
				//松开鼠标
				if (mIsDragging) {
					mIsDragging = false;
					currentEvent.Use ();
					if (BTEditorWindow.IsAutoAlign) {
						SetRealPosition (this, GetAutoAlignX (), true);
					}
					mDragDelta = Vector2.zero;
				}
				mCanDragMove = false;
			} else if (currentEvent.type == EventType.ContextClick) {
				if (BTNodeGraph.NodeRect.Contains (currentEvent.mousePosition)) {
					//显示右键菜单
					ShowMenu ();
				}
			}
		}

		private Vector2 GetAutoAlignX ()
		{
			if (Mathf.Abs (mDragDelta.x) >= BTConst.AutoAlignDistanceX) {
				var dist = BTNodeGraph.RealRect.x - Parent.BTNodeGraph.RealRect.x;
				var width = BTConst.DefaultWidth + BTConst.DefaultSpacing;
				if (mDragDelta.x > 0) {
					var multi = Mathf.CeilToInt (dist / width);
					return new Vector2 (multi * width - dist, 0);
				} else if (mDragDelta.x < 0) {
					var multi = Mathf.FloorToInt (dist / width);
					return new Vector2 (multi * width - dist, 0);
				} else {
					return Vector2.zero;
				}
			} else {
				return -mDragDelta;
			}
		}

		private void SetRealPosition (BTNode parent, Vector2 delta, bool includeChildren)
		{
			parent.BTNodeGraph.RealRect.position += delta;
			if (includeChildren && parent.IsHaveChild) {
				foreach (var node in parent.ChildNodeList) {
					SetRealPosition (node, delta, includeChildren);
				}
			}
		}

		public void Callback (object obj)
		{
			string name = obj.ToString ();
			if (name == "Delete")
				BTHelper.RemoveChild (Owner, Parent, this);
			else {
				var node = BTHelper.AddChild (Owner, this, name);
				BTHelper.SetNodeDefaultData (node, name);
			}
		}

		void ShowMenu ()
		{
			var menu = BTHelper.GetGenericMenu (this, Callback);
			menu.ShowAsContext ();
		}
	}
}