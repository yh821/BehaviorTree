using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace BT
{
	public class BtNode
	{
		/// <summary>
		/// 编辑化节点
		/// </summary>
		public BtNodeType Type { get; }

		/// <summary>
		/// 唯一识别符
		/// </summary>
		public string Guid { get; }

		public string NodeName { get; }

		public BtNodeData Data { get; }

		public BehaviourTree Owner { get; }

		public BtNode Parent { set; get; }

		/// <summary>
		/// 节点Rect定义
		/// </summary>
		public BtNodeGraph BtNodeGraph;

		/// <summary>
		/// 子节点
		/// </summary>
		public List<BtNode> ChildNodeList;

		/// <summary>
		/// 是否拥有子节点
		/// </summary>
		public bool IsHaveParent => Parent != null;

		/// <summary>
		/// 是否拥有子节点
		/// </summary>
		public bool IsHaveChild => ChildNodeList.Count > 0;

		/// <summary>
		/// 是否是子节点
		/// </summary>
		public bool IsTask => Type.Type == BtNodeEnum.Task;

		/// <summary>
		/// 是否是根节点
		/// </summary>
		public bool IsRoot => NodeName == BtConst.RootName;

		/// <summary>
		/// 是否已选中
		/// </summary>
		private bool IsSelected => BtEditorWindow.Window.CurSelectNode == this;

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

		public BtNode(BehaviourTree owner, BtNode parent, BtNodeData data)
		{
			Owner = owner;
			Parent = parent;
			Data = data;
			BtNodeGraph = new BtNodeGraph();
			NodeName = data.name;
			BtNodeGraph.RealRect = new Rect(data.posX, data.posY, BtConst.DefaultWidth, BtConst.DefaultHeight);
			ChildNodeList = new List<BtNode>();
			Guid = BtHelper.GenerateUniqueStringId();
			Type = BtHelper.CreateNodeType(this);
		}

		public void Update(Rect canvas)
		{
			DrawNode();
			DealHandles(canvas);
		}

		private void DrawNode()
		{
			if (IsHaveChild)
			{
				mBzStartPos = BtNodeGraph.DownPointRect.center;
				foreach (var node in ChildNodeList)
				{
					mBzEndPos = node.BtNodeGraph.UpPointRect.center;
					var center = mBzStartPos.x + (mBzEndPos.x - mBzStartPos.x) / 2;
					Handles.DrawBezier(mBzStartPos, mBzEndPos, new Vector3(center, mBzStartPos.y),
						new Vector3(center, mBzEndPos.y), Color.white, Texture2D.whiteTexture, BtConst.BezierSize);
					GUI.Label(node.BtNodeGraph.UpPointRect, EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo"));
				}
			}

			if (mIsLinkParent)
			{
				var startPos = BtNodeGraph.UpPointRect.center;
				var endPos = BtEditorWindow.Window.Event.mousePosition;
				var center = startPos.x + (endPos.x - startPos.x) / 2;
				Handles.DrawBezier(startPos, endPos, new Vector3(center, startPos.y),
					new Vector3(center, endPos.y), Color.white, Texture2D.whiteTexture, BtConst.BezierSize);
				//Handles.DrawLine (startPos, endPos);
			}

			if (!IsRoot && !IsHaveParent)
				GUI.Label(BtNodeGraph.UpPointRect, BtNodeStyle.ErrorPoint);

			if (!IsTask)
				GUI.Label(BtNodeGraph.DownPointRect,
					Type.IsValid == ErrorType.Error ? BtNodeStyle.ErrorPoint : BtNodeStyle.LinePoint);

			var style = IsSelected ? Type.SelectStyle : Type.NormalStyle;
			var showLabel = Data.displayName;
			if (Data.data == null)
			{
				showLabel = $"\n{showLabel}";
			}
			else if (Data.data != null && Data.data.Count == 1)
			{
				var first = Data.data.First();
				showLabel = $"{showLabel}\n{first.Key}:{first.Value}";
			}
			else if (Data.data != null && Data.data.Count >= 2)
			{
				var i = 0;
				foreach (var data in Data.data)
				{
					if (i < 2)
						showLabel = $"{showLabel}\n{data.Key}:{data.Value}";
					else
						break;
					i++;
				}
			}

			EditorGUI.LabelField(BtNodeGraph.NodeRect, showLabel, style);

			if (BtEditorWindow.IsDebug)
			{
				GUI.Label(BtNodeGraph.LeftUpRect, new GUIContent($"{BtNodeGraph.RealRect.x},{BtNodeGraph.RealRect.y}"));
			}
		}

		/// <summary>
		/// 处理事件
		/// </summary>
		private void DealHandles(Rect canvas)
		{
			var window = BtEditorWindow.Window;
			var curEvent = window.Event;
			if (curEvent.type == EventType.MouseDrag && curEvent.button == 0)
			{
				//拖拽
				if (BtNodeGraph.NodeRect.Contains(curEvent.mousePosition) && mCanDragMove)
				{
					curEvent.Use();
					mIsDragging = true;
					window.CurSelectNode = this;
					var delta = BtEditorWindow.IsLockAxisY ? new Vector2(curEvent.delta.x, 0) : curEvent.delta;
					UpdateNodePosition(this, delta);
				}
			}
			else if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
			{
				//点击
				if (curEvent.mousePosition.x >= canvas.width - BtConst.RightInspectWidth)
				{
					//window.CurSelectNode = null;
				}
				else if (BtNodeGraph.UpPointRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					if (!IsRoot)
					{
						if (IsHaveParent)
						{
							Parent.ChildNodeList.Remove(this);
							Parent.Data.children.Remove(Data);
							Owner.AddBrokenNode(this);
							Parent = null;
						}
						else
						{
							window.CurSelectNode = this;
							mIsLinkParent = true;
						}
					}
				}
				else if (BtNodeGraph.NodeRect.Contains(curEvent.mousePosition))
				{
					curEvent.Use();
					window.CurSelectNode = this;
					mCanDragMove = true;
				}
				else
				{
					window.CurSelectNode = null;
				}
			}
			else if (curEvent.type == EventType.MouseUp && curEvent.button == 0)
			{
				//松开鼠标
				if (mIsDragging)
				{
					curEvent.Use();
					mIsDragging = false;
					if (BtEditorWindow.IsAutoAlign)
					{
						SetNodePosition(this);
					}
				}

				if (mIsLinkParent)
				{
					mIsLinkParent = false;
					var parent = window.GetMouseTriggerDownPoint(curEvent.mousePosition);
					if (parent != null && parent != this && parent.ChildNodeList.Count < parent.Type.CanAddNodeCount)
					{
						parent.ChildNodeList.Add(this);
						parent.Data.AddChild(Data);
						Owner.RemoveBrokenNode(this);
						Parent = parent;
					}
				}

				mCanDragMove = false;
			}
			else if (curEvent.type == EventType.ContextClick)
			{
				if (BtNodeGraph.NodeRect.Contains(curEvent.mousePosition))
				{
					//显示右键菜单
					ShowMenu();
				}
			}
		}

		private void UpdateNodePosition(BtNode parent, Vector2 delta)
		{
			parent.BtNodeGraph.RealRect.position += delta;
			if (parent.IsHaveChild)
			{
				foreach (var node in parent.ChildNodeList)
				{
					UpdateNodePosition(node, delta);
				}
			}
		}

		private void SetNodePosition(BtNode parent)
		{
			BtHelper.AutoAlignPosition(parent);
			if (parent.IsHaveChild)
			{
				foreach (var node in parent.ChildNodeList)
				{
					SetNodePosition(node);
				}
			}
		}

		public void Callback(object obj)
		{
			var name = obj.ToString();
			if (name == "Delete")
				BtHelper.RemoveChild(this);
			else if (name == "Copy")
				BtEditorWindow.CopyNode = this;
			else if (name == "Paste")
				BtHelper.PasteChild(Owner, this, Data.posX, Data.posY + BtConst.DefaultHeight);
			else
			{
				var node = BtHelper.AddChildNode(Owner, this, name);
				BtHelper.SetNodeDefaultData(node, name);
			}
		}

		private void ShowMenu()
		{
			var menu = BtHelper.GetGenericMenu(this, Callback);
			menu.ShowAsContext();
		}
	}
}