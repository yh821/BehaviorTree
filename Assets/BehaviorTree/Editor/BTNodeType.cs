using System;
using UnityEngine;

namespace BT
{
	public enum BtNodeEnum
	{
		Composite,
		Decorator,
		Task
	}

	public enum ErrorType
	{
		Warn,
		Error,
		None
	}

	[Serializable]
	public abstract class BtNodeType
	{
		/// <summary>
		/// 归属图形化节点
		/// </summary>
		protected BtNode BelongNode { get; set; }

		/// <summary>
		/// 节点类型
		/// </summary>
		public abstract BtNodeEnum Type { get; }

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

		public abstract GUIStyle SelectStyle { get; }

		protected BtNodeType(BtNode node)
		{
			BelongNode = node;
		}
	}

	public class Decorator : BtNodeType
	{
		public override BtNodeEnum Type => BtNodeEnum.Decorator;

		public override int CanAddNodeCount => BtConst.NormalDecoratorCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.DecoratorStyle;

		public override GUIStyle SelectStyle => BtNodeStyle.SelectDecoratorStyle;

		public override ErrorType IsValid => BelongNode.ChildNodeList.Count == 1 ? ErrorType.None : ErrorType.Error;

		public Decorator(BtNode node) : base(node)
		{
		}
	}

	public class Root : Decorator
	{
		public override GUIStyle NormalStyle => BtNodeStyle.RootStyle;

		public override GUIStyle SelectStyle => BtNodeStyle.SelectRootStyle;

		public Root(BtNode node) : base(node)
		{
		}
	}

	public class Composite : BtNodeType
	{
		public override BtNodeEnum Type => BtNodeEnum.Composite;

		public override int CanAddNodeCount => BtConst.NormalCompositeCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.CompositeStyle;

		public override GUIStyle SelectStyle => BtNodeStyle.SelectCompositeStyle;

		public override ErrorType IsValid => BelongNode.IsHaveChild ? ErrorType.None : ErrorType.Error;

		public Composite(BtNode node) : base(node)
		{
		}
	}

	public class Task : BtNodeType
	{
		public override BtNodeEnum Type => BtNodeEnum.Task;

		public override int CanAddNodeCount => BtConst.NormalTaskCanAddNode;

		public override GUIStyle NormalStyle => BtNodeStyle.TaskStyle;

		public override GUIStyle SelectStyle => BtNodeStyle.SelectTaskStyle;

		public override ErrorType IsValid => ErrorType.None;

		public Task(BtNode node) : base(node)
		{
		}
	}
}