using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class BTNode
	{
		/// <summary>
		/// 编辑化节点
		/// </summary>
		public EditorNode Node;
		/// <summary>
		/// 唯一识别符
		/// </summary>
		public string Guid;
		/// <summary>
		/// 节点名
		/// </summary>
		public string Label;

		private BehaviourTree mOwner;

		public BehaviourTree Owner {
			get{ return mOwner; }
		}

		/// <summary>
		/// 节点状态 . 错误类型
		/// </summary>
		public ErrorType ErrorType {
			get { return Node.GetIsVaild (); }
		}

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
		/// 节点类型
		/// </summary>
		public Type NodeType {
			get { return Node.GetType (); }
		}

		/// <summary>
		/// 节点枚举类型
		/// </summary>
		public EditorNodeEnum NodeEnum {
			get { return Node.NodeEnum; }
		}

		/// <summary>
		/// 默认 取第一个子节点
		/// </summary>
		public EditorNode DefaultNode {
			get {
				if (IsHaveChild) {
					return ChildNodeList [0].Node;
				}

				return null;
			}
		}

		/// <summary>
		/// 实际Rect范围
		/// </summary>
		public Rect RealRect {
			get { return BTNodeGraph.RealRect; }
		}

		/// <summary>
		/// 节点范围
		/// </summary>
		public Rect NodeRect {
			get {
				Rect ret = RealRect;
				ret.position += BTEditorProperty.Instance.Position;
				return ret;
			}
			set { BTNodeGraph.RealRect = value; }
		}

		/// <summary>
		/// 是否是根节点
		/// </summary>
		public bool IsRoot {
			get{ return Label == "Root"; }
		}

		/// <summary>
		/// 是否可以拖拽
		/// </summary>
		private bool mCanDragMove = false;
		/// <summary>
		/// 是否拖拽中
		/// </summary>
		private bool mIsDragging = false;

		private Vector3 startPos;
		private Vector3 endPos;

		public BTNode (BehaviourTree owner, string label, EditorNode node, Rect rect)
		{
			mOwner = owner;
			BTNodeGraph = new BTNodeGraph ();  
			Label = label;
			Node = node;
			BTNodeGraph.RealRect = rect;
			InitNode ();
		}

		private void InitNode ()
		{
			ChildNodeList = new List<BTNode> ();
			Guid = BTUtils.GenerateUniqueStringID ();
			Node.SetBelongNode (this);
		}

		public void Update (Rect canvas)
		{
			DrawNode ();
			DealHandles (canvas);
		}

		private void DrawNode ()
		{
			GUIStyle style = BTEditorProperty.Instance.GetIsSelectNode (this) ? BTNodeStyle.SelectStyle : BTNodeStyle.NormalStyle;
			EditorGUI.LabelField (NodeRect, Label, style);

			if (IsHaveChild) {
				startPos = BTNodeGraph.DownPointRect.center;
				foreach (var node in ChildNodeList) {
					endPos = node.BTNodeGraph.UpPointRect.center;
					float center = startPos.x + (endPos.x - startPos.x) / 2;
					Handles.DrawBezier (startPos, endPos, new Vector3 (center, startPos.y), 
						new Vector3 (center, endPos.y), Color.white, Texture2D.whiteTexture, BTEditorConst.BEZIER_WIDTH);
					GUI.DrawTexture (node.BTNodeGraph.UpPointRect, BTNodeStyle.LinePoint);
				}
				GUI.DrawTexture (BTNodeGraph.DownPointRect, BTNodeStyle.LinePoint);
			}
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles (Rect canvas)
		{
			Event currentEvent = BTEditorProperty.Instance.Event;
			if (currentEvent.isMouse && currentEvent.type == EventType.MouseDrag && currentEvent.button == 0) {
				//拖拽
				if (NodeRect.Contains (currentEvent.mousePosition) && mCanDragMove) {
					mIsDragging = true;
					currentEvent.Use ();
					BTEditorProperty.Instance.AddSelectNode (Guid, this);
					BTNodeGraph.RealRect.position += currentEvent.delta;
				}
			} else if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) {
				//点击
				if (NodeRect.Contains (currentEvent.mousePosition)) {
					BTEditorProperty.Instance.AddSelectNode (this);
					mCanDragMove = true;
				} else {
					if (currentEvent.mousePosition.x <= canvas.width - BTEditorConst.LEFT_INSPECT_WIDTH) {
						BTEditorProperty.Instance.RemoveSelectNode (this);
					}
				}
			} else if (currentEvent.isMouse && currentEvent.type == EventType.MouseUp && currentEvent.button == 0) {
				//松开鼠标
				if (mIsDragging) {
					currentEvent.Use ();
					mIsDragging = false;
				}
				mCanDragMove = false;
			} else if (currentEvent.type == EventType.ContextClick) {
				if (NodeRect.Contains (currentEvent.mousePosition)) {
					//显示右键菜单
					ShowMenu ();
				}
			}
		}

		public void Callback (object obj)
		{
			string name = obj.ToString ();
			switch (name) {
			case "Selector":
			case "Sequence":
				AddChild (Owner, name, new Composite ());
				break;
			}
		}

		void AddChild (BehaviourTree owner, string nodeName, EditorNode node)
		{
			var pos = BTNodeGraph.NodeRect.position;
			var rect = new Rect (new Vector2 (pos.x, pos.y + BTEditorConst.Default_Distance), BTNodeGraph.NodeRect.size);
			var child = new BTNode (owner, nodeName, node, rect);
			owner.AddNode (child);
			ChildNodeList.Add (child);
		}

		void ShowMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			AddMenuItem (menu, "Composite/Selector", "Selector");
			AddMenuItem (menu, "Composite/Sequence", "Sequence");
			menu.AddSeparator ("");
			AddMenuItem (menu, "Action/Wait", "Wait");
			AddMenuItem (menu, "Action/RandomPoint", "RandomPoint");
			menu.ShowAsContext ();
		}

		void AddMenuItem (GenericMenu menu, string menuPath, string node)
		{
			menu.AddItem (new GUIContent (menuPath), false, Callback, node);
		}
	}
}