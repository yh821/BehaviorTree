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

		public BTNode Parent { set; get; }

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
		public bool IsHaveParent { get { return Parent != null; } }

		/// <summary>
		/// 是否拥有子节点
		/// </summary>
		public bool IsHaveChild { get { return ChildNodeList.Count > 0; } }

		/// <summary>
		/// 是否是子节点
		/// </summary>
		public bool IsTask { get { return Type.Type == BTNodeEnum.Task; } }

		/// <summary>
		/// 是否是根节点
		/// </summary>
		public bool IsRoot { get { return NodeName == BTConst.RootName; } }

		/// <summary>
		/// 是否已选中
		/// </summary>
		bool IsSelected { get { return BTEditorWindow.window.CurSelectNode == this; } }

		/// <summary>
		/// 是否可以拖拽
		/// </summary>
		private bool mCanDragMove = false;
		/// <summary>
		/// 是否拖拽中
		/// </summary>
		private bool mIsDragging = false;
		/// <summary>
		/// 正在连父节点
		/// </summary>
		private bool mIsLinkParent = false;

		private Vector3 mBzStartPos;
		private Vector3 mBzEndPos;

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
			GUIStyle style = IsSelected ? Type.SelectStyle : Type.NormalStyle;
			string showLabel;
			if (Data.data != null && Data.data.Count > 0) {
				var first = Data.data.First ();
				showLabel = string.Format ("{0}\n{1}:{2}", Label, first.Key, first.Value);
			} else
				showLabel = Label;

			EditorGUI.LabelField (BTNodeGraph.NodeRect, showLabel, style);

			if (IsHaveChild) {
				mBzStartPos = BTNodeGraph.DownPointRect.center;
				foreach (var node in ChildNodeList) {
					mBzEndPos = node.BTNodeGraph.UpPointRect.center;
					float center = mBzStartPos.x + (mBzEndPos.x - mBzStartPos.x) / 2;
					Handles.DrawBezier (mBzStartPos, mBzEndPos, new Vector3 (center, mBzStartPos.y), 
						new Vector3 (center, mBzEndPos.y), Color.white, Texture2D.whiteTexture, BTConst.BEZIER_WIDTH);
					GUI.DrawTexture (node.BTNodeGraph.UpPointRect, BTNodeStyle.LinePoint);
				}
			}

			if (mIsLinkParent) {
				var startPos = BTNodeGraph.UpPointRect.center;
				var endPos = BTEditorWindow.window.Event.mousePosition;
				float center = startPos.x + (endPos.x - startPos.x) / 2;
				Handles.DrawBezier (startPos, endPos, new Vector3 (center, startPos.y), 
					new Vector3 (center, endPos.y), Color.white, Texture2D.whiteTexture, BTConst.BEZIER_WIDTH);
				//Handles.DrawLine (startPos, endPos);
			}

			if (!IsTask) {
				if (Type.IsValid == ErrorType.Error)
					GUI.DrawTexture (BTNodeGraph.DownPointRect, BTNodeStyle.ErrorPoint);
				else
					GUI.DrawTexture (BTNodeGraph.DownPointRect, BTNodeStyle.LinePoint);
			}

			if (!IsRoot && !IsHaveParent) {
				GUI.DrawTexture (BTNodeGraph.UpPointRect, BTNodeStyle.ErrorPoint);
			}
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles (Rect canvas)
		{
			var window = BTEditorWindow.window;
			Event curEvent = window.Event;
			if (curEvent.isMouse && curEvent.type == EventType.MouseDrag && curEvent.button == 0) {
				//拖拽
				if (BTNodeGraph.NodeRect.Contains (curEvent.mousePosition) && mCanDragMove) {
					curEvent.Use ();
					mIsDragging = true;
					window.CurSelectNode = this;

					Vector2 delta;
					if (BTEditorWindow.IsLockAxisY)
						delta = new Vector2 (curEvent.delta.x, 0);
					else
						delta = curEvent.delta;
					
					UpdateNodePosition (this, delta);
				}
			} else if (curEvent.isMouse && curEvent.type == EventType.MouseDown && curEvent.button == 0) {
				//点击
				if (BTNodeGraph.UpPointRect.Contains (curEvent.mousePosition)) {
					curEvent.Use ();
					if (!IsRoot) {
						if (IsHaveParent) {
							Parent.ChildNodeList.Remove (this);
							Parent.Data.children.Remove (Data);
							Owner.AddOrphanNode (this);
							Parent = null;
						} else {
							window.CurSelectNode = this;
							mIsLinkParent = true;
						}
					}
				} else if (BTNodeGraph.NodeRect.Contains (curEvent.mousePosition)) {
					curEvent.Use ();
					window.CurSelectNode = this;
					mCanDragMove = true;
				} else {
					if (curEvent.mousePosition.x <= canvas.width - BTConst.LEFT_INSPECT_WIDTH) {
						window.CurSelectNode = null;
					}
				}
			} else if (curEvent.isMouse && curEvent.type == EventType.MouseUp && curEvent.button == 0) {
				//松开鼠标
				if (mIsDragging) {
					mIsDragging = false;
					curEvent.Use ();
					if (BTEditorWindow.IsAutoAlign) {
						SetNodePosition (this);
					}
				}

				if (mIsLinkParent) {
					mIsLinkParent = false;
					var parent = window.GetMouseTriggerDownPoint (curEvent.mousePosition);
					if (parent != null && parent != this && parent.ChildNodeList.Count < parent.Type.CanAddNodeCount) {
						parent.ChildNodeList.Add (this);
						parent.Data.AddChild (Data);
						Owner.RemoveOrphanNode (this);
						Parent = parent;
					}
				}

				mCanDragMove = false;
			} else if (curEvent.type == EventType.ContextClick) {
				if (BTNodeGraph.NodeRect.Contains (curEvent.mousePosition)) {
					//显示右键菜单
					ShowMenu ();
				}
			}
		}

		private void UpdateNodePosition (BTNode parent, Vector2 delta)
		{
			parent.BTNodeGraph.RealRect.position += delta;
			if (parent.IsHaveChild) {
				foreach (var node in parent.ChildNodeList) {
					UpdateNodePosition (node, delta);
				}
			}
		}

		private void SetNodePosition (BTNode parent)
		{
			BTHelper.AutoAlignPosition (parent);
			if (parent.IsHaveChild) {
				foreach (var node in parent.ChildNodeList) {
					SetNodePosition (node);
				}
			}
		}

		public void Callback (object obj)
		{
			string name = obj.ToString ();
			if (name == "Delete")
				BTHelper.RemoveChild (this);
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