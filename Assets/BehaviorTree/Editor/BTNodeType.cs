using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	public enum TaskType
	{
		Composite,
		Decorator,
		Condition,
		Action,
		Root
	}

	public enum AbortType
	{
		None,
		Self,
		Lower,
		Both
	}

	public enum ErrorType
	{
		Warn,
		Error,
		None
	}

	public class BtConst
	{
		/// <summary>
		/// 装饰节点 一般可添加子节点
		/// </summary>
		public const int NormalDecoratorCanAddNode = 1;

		/// <summary>
		/// 复合节点 一般可添加子节点
		/// </summary>
		public const int NormalCompositeCanAddNode = 999;

		/// <summary>
		/// 任务节点 一般可添加子节点
		/// </summary>
		public const int NormalTaskCanAddNode = 0;

		/// <summary>
		/// 贝塞尔曲线粗细
		/// </summary>
		public const int BezierSize = 3;

		/// <summary>
		/// 连接点半径
		/// </summary>
		public const float LinePointLength = 24;

		/// <summary>
		/// 图标尺寸
		/// </summary>
		public const float IconSize = 40;

		/// <summary>
		/// 左侧监视面板宽度
		/// </summary>
		public const float RightInspectWidth = 240;
		/// <summary>
		/// 左侧监视面板高度
		/// </summary>
		public const float RightInspectHeight = 500;

		/// <summary>
		/// 节点默认宽度
		/// </summary>
		public const int DefaultWidth = 90;

		/// <summary>
		/// 节点默认高度
		/// </summary>
		public const int DefaultHeight = 60;

		/// <summary>
		/// 节点默认横行距离
		/// </summary>
		public const int DefaultSpacingX = 10;

		/// <summary>
		/// 节点默认纵向距离
		/// </summary>
		public const int DefaultSpacingY = 60;

		/// <summary>
		/// 根节点名
		/// </summary>
		public const string RootName = "rootNode";

	}

	public class BtNodeLua
	{
		public string file;
		public string type;
		public Dictionary<string, string> data;
		public List<BtNodeLua> children;
	}

	public class BtNodeData
	{
		public string name = string.Empty;
		public string desc = string.Empty;
		public string file = string.Empty;
		public string type = string.Empty;
		public float posX = 0;
		public float posY = 0;
		public int index = -1;
		public bool fold = false;//是否折叠子节点

		public Dictionary<string, string> data;

		public List<BtNodeData> children;

		public BtNodeData(string file, string type, float x, float y)
		{
			this.file = file;
			this.type = type;
			SetPos(x, y);
			name = file.Replace("Node", "");
		}

		public void AddChild(BtNodeData child)
		{
			if (children == null)
				children = new List<BtNodeData>();
			children.Add(child);
		}

		public void AddData(string key, string value)
		{
			if (data == null)
				data = new Dictionary<string, string>();
			if (data.ContainsKey(key))
				data[key] = value;
			else
				data.Add(key, value);
		}

		public void RemoveData(string key)
		{
			if (data != null && data.ContainsKey(key))
				data.Remove(key);
		}

		public BtNodeData Clone()
		{
			var clone = new BtNodeData(file, type, posX, posY) { name = name };
			if (data != null)
				clone.data = new Dictionary<string, string>(data);
			return clone;
		}

		public void SetPos(float x, float y)
		{
			posX = x;
			posY = y;
		}

		public Vector2 GetPosition()
		{
			return new Vector2(posX, posY);
		}

		public void SetPosition(Vector2 pos)
		{
			SetPos(pos.x, pos.y);
		}

	}

	public abstract class BtNodeType
	{
		/// <summary>
		/// 归属图形化节点
		/// </summary>
		protected BtNode BelongNode { get; set; }

		/// <summary>
		/// 节点类型
		/// </summary>
		public abstract TaskType Type { get; }

		/// <summary>
		/// 节点是否有效
		/// </summary>
		/// <returns></returns>
		public abstract ErrorType IsValid { get; }

		/// <summary>
		/// 可添加节点数量
		/// </summary>
		public abstract int CanAddNodeCount { get; }

		public abstract GUIStyle NormalStyle { get; }
		public abstract GUIStyle FoldNormalStyle { get; }
		public abstract GUIStyle SelectStyle { get; }
		public abstract GUIStyle FoldSelectStyle { get; }

		protected BtNodeType(BtNode node)
		{
			BelongNode = node;
		}

		public virtual Texture GetIcon()
		{
			return null;
		}
	}

	public class Root : Decorator
	{
		public override TaskType Type => TaskType.Root;
		public override GUIStyle NormalStyle => BtNodeStyle.RootStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectRootStyle;
		public Root(BtNode node) : base(node)
		{
		}

		public override Texture GetIcon()
		{
			return BtNodeStyle.RootIcon;
		}
	}

	public class Decorator : BtNodeType
	{
		public override TaskType Type => TaskType.Decorator;

		public override int CanAddNodeCount => BtConst.NormalDecoratorCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.DecoratorStyle;
		public override GUIStyle FoldNormalStyle => BtNodeStyle.FoldDecoratorStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectDecoratorStyle;
		public override GUIStyle FoldSelectStyle => BtNodeStyle.FoldSelectDecoratorStyle;

		public override ErrorType IsValid => BelongNode.ChildNodeList.Count == 1 ? ErrorType.None : ErrorType.Error;

		public Decorator(BtNode node) : base(node)
		{
		}
	}

	public class Composite : BtNodeType
	{
		public override TaskType Type => TaskType.Composite;

		public override int CanAddNodeCount => BtConst.NormalCompositeCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.CompositeStyle;
		public override GUIStyle FoldNormalStyle => BtNodeStyle.FoldCompositeStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectCompositeStyle;
		public override GUIStyle FoldSelectStyle => BtNodeStyle.FoldSelectCompositeStyle;

		public override ErrorType IsValid => BelongNode.IsHaveChild ? ErrorType.None : ErrorType.Error;

		public Composite(BtNode node) : base(node)
		{
		}
	}

	public class Condition : BtNodeType
	{
		public override TaskType Type => TaskType.Condition;

		public override int CanAddNodeCount => BtConst.NormalTaskCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.ConditionStyle;
		public override GUIStyle FoldNormalStyle => BtNodeStyle.FoldConditionStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectConditionStyle;
		public override GUIStyle FoldSelectStyle => BtNodeStyle.FoldSelectConditionStyle;

		public override ErrorType IsValid => ErrorType.None;

		public Condition(BtNode node) : base(node)
		{
		}
	}

	public class Action : BtNodeType
	{
		public override TaskType Type => TaskType.Action;

		public override int CanAddNodeCount => BtConst.NormalTaskCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.ActionStyle;
		public override GUIStyle FoldNormalStyle => BtNodeStyle.FoldTaskStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectTaskStyle;
		public override GUIStyle FoldSelectStyle => BtNodeStyle.FoldSelectTaskStyle;

		public override ErrorType IsValid => ErrorType.None;

		public Action(BtNode node) : base(node)
		{
		}
	}
}