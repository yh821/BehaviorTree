using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class BtGrid
	{
		private readonly Texture mBackground;
		public BtGrid()
		{
			var path = BtHelper.toolPath + "/GUI/background.png";
			path = FileUtil.GetProjectRelativePath(path);
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

        public BtNodeGraph Graph { get; }

        public List<BtNode> ChildNodeList;

        public bool IsHaveParent => Parent != null;

        public bool IsHaveChild => ChildNodeList.Count > 0;

        public bool IsTask => Type.Type == BtNodeEnum.Task;

        public bool IsRoot => NodeName == BtConst.RootName;

        public bool IsSelected => BtEditorWindow.Window.CurSelectNode == this;

        private bool mCanDragMove = false;
        private bool mIsDragging = false;
        private bool mIsLinkParent = false;

        private Vector3 mBzStartPos;
        private Vector3 mBzEndPos;

        public BtNode(BehaviourTree owner, BtNode parent, BtNodeData data)
        {
            Owner = owner;
            Parent = parent;
            Data = data;
            Graph = new BtNodeGraph();
            NodeName = data.name;
            Graph.RealRect = new Rect(data.posX, data.posY, BtConst.DefaultWidth, BtConst.DefaultHeight);
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
            if (IsHaveChild && !Data.fold)
            {
                mBzStartPos = Graph.DownPointRect.center;
                foreach (var node in ChildNodeList)
                {
                    mBzEndPos = node.Graph.UpPointRect.center;
                    var center = mBzStartPos.x + (mBzEndPos.x - mBzStartPos.x) / 2;
                    Handles.DrawBezier(mBzStartPos, mBzEndPos, new Vector3(center, mBzStartPos.y),
                        new Vector3(center, mBzEndPos.y), Color.white, Texture2D.whiteTexture, BtConst.BezierSize);
                    GUI.Label(node.Graph.UpPointRect, BtNodeStyle.LinePoint);
                }
            }

            if (mIsLinkParent)
            {
                var startPos = Graph.UpPointRect.center;
                var endPos = BtEditorWindow.Window.Event.mousePosition;
                var center = startPos.x + (endPos.x - startPos.x) / 2;
                Handles.DrawBezier(startPos, endPos, new Vector3(center, startPos.y),
                    new Vector3(center, endPos.y), Color.white, Texture2D.whiteTexture, BtConst.BezierSize);
                //Handles.DrawLine (startPos, endPos);
            }

            if (!IsRoot && !IsHaveParent)
                GUI.Label(Graph.UpPointRect, BtNodeStyle.ErrorPoint);

            if (!IsTask)
                GUI.Label(Graph.DownPointRect,
                    Type.IsValid == ErrorType.Error ? BtNodeStyle.ErrorPoint : BtNodeStyle.LinePoint);

            GUIStyle style;
            if (IsSelected)
                style = Data.fold ? Type.FoldSelectStyle : Type.SelectStyle;
            else
                style = Data.fold ? Type.FoldNormalStyle : Type.NormalStyle;

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

            var icon = Type.GetIcon();
            if (icon == null)
                GUI.Label(Graph.NodeRect, showLabel, style);
            else
            {
                GUI.Label(Graph.NodeRect, "", style);
                GUI.Label(Graph.IconRect, icon);
                GUI.Label(Graph.LabelRect, showLabel);
            }

            if (BtEditorWindow.IsDebug)
            {
                GUI.Label(Graph.LeftUpRect, new GUIContent($"{Graph.RealRect.x},{Graph.RealRect.y}"));
            }
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        private void DealHandles(Rect canvas)
        {
            var window = BtEditorWindow.Window;
            var curEvent = window.Event;
            //拖拽
            if (curEvent.type == EventType.MouseDrag && curEvent.button == 0)
            {
                if (Graph.NodeRect.Contains(curEvent.mousePosition) && mCanDragMove)
                {
                    curEvent.Use();
                    mIsDragging = true;
                    window.CurSelectNode = this;
                    var delta = BtEditorWindow.IsLockAxisY ? new Vector2(curEvent.delta.x, 0) : curEvent.delta;
                    UpdateNodePosition(this, delta);
                }
            }
            //点击
            else if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
            {
                if (curEvent.mousePosition.x >= canvas.width - BtConst.RightInspectWidth)
                {
                    //window.CurSelectNode = null;
                }
                else if (Graph.UpPointRect.Contains(curEvent.mousePosition))
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
                else if (Graph.DownPointRect.Contains(curEvent.mousePosition))
                {
                    curEvent.Use();
                    Data.fold = !Data.fold;
                }
                else if (Graph.NodeRect.Contains(curEvent.mousePosition))
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
            //松开鼠标
            else if (curEvent.type == EventType.MouseUp && curEvent.button == 0)
            {
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
                if (Graph.NodeRect.Contains(curEvent.mousePosition))
                {
                    //显示右键菜单
                    ShowMenu();
                }
            }
        }

        private void UpdateNodePosition(BtNode parent, Vector2 delta)
        {
            parent.Graph.RealRect.position += delta;
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

    public class BehaviourTree
    {
        public Dictionary<string, BtNode> NodeDict;
        public Dictionary<string, BtNode> BrokenNodeDict;
        public Dictionary<string, int> NodePosDict;

        public string Name { get; set; }

        public BtNode Root { get; }

        public BehaviourTree()
        {
            NodeDict = new Dictionary<string, BtNode>();
            NodePosDict = new Dictionary<string, int>();
        }

        public BehaviourTree(string name, BtNodeData data = null)
        {
            Name = name;
            NodeDict = new Dictionary<string, BtNode>();
            NodePosDict = new Dictionary<string, int>();
            BrokenNodeDict = new Dictionary<string, BtNode>();
            if (data == null)
            {
                data = new BtNodeData(BtConst.RootName, string.Empty,
                    (BtEditorWindow.Window.position.width - BtConst.RightInspectWidth) / 2, 50);
                data.AddData("restart", "1");
            }

            Root = new BtNode(this, null, data);
            AddNode(Root);
            BtHelper.AutoAlignPosition(Root);
        }

        public void Update(Rect canvas)
        {
            if (NodeDict == null) return;
            foreach (var node in NodeDict.Values)
            {
                node.Update(canvas);
            }
        }

        public void AddNode(BtNode node)
        {
            NodeDict.Add(node.Guid, node);
            //用于判断是否重叠
            var key = node.Data.GetPosition().ToString();
            if (NodePosDict.TryGetValue(key, out var count))
                NodePosDict[key] = count + 1;
            else
                NodePosDict.Add(key, 1);
        }

        public void RemoveNode(BtNode node)
        {
            NodeDict.Remove(node.Guid);
            BrokenNodeDict.Remove(node.Guid);
            NodePosDict.Remove(node.Data.GetPosition().ToString());
        }

        public void AddBrokenNode(BtNode node)
        {
            BrokenNodeDict.Add(node.Guid, node);
        }

        public void RemoveBrokenNode(BtNode node)
        {
            BrokenNodeDict.Remove(node.Guid);
        }

        public Vector2 GenNodePos(Vector2 pos)
        {
            while (true)
            {
                if (NodePosDict.ContainsKey(pos.ToString()))
                {
                    pos.y += (BtConst.DefaultHeight + BtConst.DefaultSpacingY) / 2f;
                    continue;
                }

                NodePosDict.Add(pos.ToString(), 1);
                return pos;
            }
        }
    }

    public class BtNodeGraph
	{
		/// <summary>
		/// 实际节点
		/// </summary>
		public Rect RealRect;

		/// <summary>
		/// 节点范围
		/// </summary>
		public Rect NodeRect
		{
			get
			{
				var ret = RealRect;
				ret.position += BtEditorWindow.Window.Position;
				return ret;
			}
		}

		/// <summary>
		/// 图标位置
		/// </summary>
		public Rect IconRect
		{
			get
			{
				var rect = NodeRect;
				return new Rect(rect.x, rect.y, rect.width, BtConst.IconSize);
			}
		}

		/// <summary>
		/// 文本位置
		/// </summary>
		public Rect LabelRect
		{
			get
			{
				var rect = NodeRect;
				return new Rect(rect.x, rect.y + BtConst.IconSize,
					rect.width, rect.height - BtConst.IconSize);
			}
		}

		/// <summary>
		/// 下部连接点
		/// </summary>
		public Rect DownPointRect =>
			new Rect(NodeRect.center.x - BtConst.LinePointLength / 2, NodeRect.yMax,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 上部连接点
		/// </summary>
		public Rect UpPointRect =>
			new Rect(NodeRect.center.x - BtConst.LinePointLength / 2,
				NodeRect.yMin - BtConst.LinePointLength,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 错误节点范围
		/// </summary>
		public Rect ErrorRect =>
			new Rect(NodeRect.x + 5, NodeRect.y + 5,
				BtConst.LinePointLength, BtConst.LinePointLength);

		/// <summary>
		/// 左上显示区
		/// </summary>
		public Rect LeftUpRect => new Rect(NodeRect.xMin,
			NodeRect.yMin - BtConst.DefaultHeight / 2f,
			BtConst.DefaultWidth, BtConst.DefaultHeight);
	}

	public static class BtNodeStyle
	{
		public static GUIStyle RootStyle => "flow node 0";
		public static GUIStyle FoldRootStyle => "flow node hex 0";
		public static GUIStyle SelectRootStyle => "flow node 0 on";
		public static GUIStyle FoldSelectRootStyle => "flow node hex 0 on";

		public static GUIStyle DecoratorStyle => "flow node 2";
		public static GUIStyle FoldDecoratorStyle => "flow node hex 2";
		public static GUIStyle SelectDecoratorStyle => "flow node 2 on";
		public static GUIStyle FoldSelectDecoratorStyle => "flow node hex 2 on";

		public static GUIStyle CompositeStyle => "flow node 1";
		public static GUIStyle FoldCompositeStyle => "flow node hex 1";
		public static GUIStyle SelectCompositeStyle => "flow node 1 on";
		public static GUIStyle FoldSelectCompositeStyle => "flow node hex 1 on";

		public static GUIStyle TaskStyle => "flow node 3";
		public static GUIStyle FoldTaskStyle => "flow node hex 3";
		public static GUIStyle SelectTaskStyle => "flow node 3 on";
		public static GUIStyle FoldSelectTaskStyle => "flow node hex 3 on";


		private static GUIContent _RootContent;
		public static GUIContent RootContent => _RootContent ??= EditorGUIUtility.IconContent("Import");

		private static GUIContent _LinePoint;
		public static GUIContent LinePoint => _LinePoint ??= EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo");

		private static GUIContent _WarnPoint;
		public static GUIContent WarnPoint => _WarnPoint ??= EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo");

		private static GUIContent _ErrorPoint;
		public static GUIContent ErrorPoint => _ErrorPoint ??= EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo");
	}
}