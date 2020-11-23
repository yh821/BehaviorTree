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
				ret.position += BTEditorWindow.window.Position;
				return ret;
			}
			set { BTNodeGraph.RealRect = value; }
		}

		/// <summary>
		/// 是否是根节点
		/// </summary>
		public bool IsRoot { get; }

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

		public BTNode (BehaviourTree owner, BTNode parent, BTNodeData data)
		{
			Owner = owner;
			Parent = parent;
			Data = data;
			BTNodeGraph = new BTNodeGraph ();
			NodeName = data.name;
			IsRoot = NodeName == BTConst.RootName;
			Label = NodeName.Replace ("Node", "");
			BTNodeGraph.RealRect = new Rect (data.posX, data.posY, BTConst.Default_Width, BTConst.Default_Height);
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

			EditorGUI.LabelField (NodeRect, showLabel, style);

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
				if (NodeRect.Contains (currentEvent.mousePosition) && mCanDragMove) {
					mIsDragging = true;
					currentEvent.Use ();
					window.CurSelectNode = this;
					WalkChildPos (this, currentEvent.delta);
				}
			} else if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) {
				//点击
				if (NodeRect.Contains (currentEvent.mousePosition)) {
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
				}
				mCanDragMove = false;
			} else if (currentEvent.type == EventType.ContextClick) {
				if (NodeRect.Contains (currentEvent.mousePosition)) {
					//显示右键菜单
					ShowMenu ();
				}
			}
		}

		private void WalkChildPos (BTNode parent, Vector2 delta)
		{
			parent.BTNodeGraph.RealRect.position += delta;
			if (parent.IsHaveChild) {
				foreach (var node in parent.ChildNodeList) {
					WalkChildPos (node, delta);
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
				var data = node.Data;
				switch (name) {
				case "waitNode":
					data.AddData ("waitMin", "2");
					data.AddData ("waitMax", "5");
					break;
				case "weightNode":
					data.AddData ("weight", "10");
					break;
				}
			}
		}

		void ShowMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			if (!IsRoot && !IsHaveChild)
				AddMenuItem (menu, "Delete Node", "Delete");

			if (Type.Type != BTNodeEnum.Task && ChildNodeList.Count < Type.CanAddNodeCount) {
				menu.AddSeparator ("");
				AddMenuItem (menu, "Composite/Random Selector", "randomSelectorNode");
				AddMenuItem (menu, "Composite/Selector", "selectorNode");
				AddMenuItem (menu, "Composite/Sequence", "SequenceNode");
				AddMenuItem (menu, "Composite/Parallel", "parallelNode");
				menu.AddSeparator ("");
				AddMenuItem (menu, "Decorator/Failure", "failureNode");
				AddMenuItem (menu, "Decorator/Inverter", "inverterNode");
				AddMenuItem (menu, "Decorator/Success", "successNode");
				menu.AddSeparator ("");
				AddMenuItem (menu, "Action/Wait", "waitNode");
				AddMenuItem (menu, "Action/Weight", "weightNode");
				AddMenuItem (menu, "Action/MoveToPosition", "moveToPositionNode");
				AddMenuItem (menu, "Action/RandomPosition", "randomPositionNode");
				AddMenuItem (menu, "Action/RunAnimator", "runAnimatorNode");
			}

			menu.ShowAsContext ();
		}

		void AddMenuItem (GenericMenu menu, string menuPath, string node)
		{
			menu.AddItem (new GUIContent (menuPath), false, Callback, node);
		}
	}
}