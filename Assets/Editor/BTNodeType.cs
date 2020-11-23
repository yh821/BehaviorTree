using System;
using UnityEngine;

namespace BT
{
	public enum BTNodeEnum
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
	public abstract class BTNodeType
	{
		/// <summary>
		/// 归属图形化节点
		/// </summary>
		protected BTNode BelongNode { get; set; }

		/// <summary>
		/// 节点类型
		/// </summary>
		public abstract BTNodeEnum Type { get; }

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

		protected BTNodeType (BTNode node)
		{
			BelongNode = node;
		}
	}

	public class Decorator : BTNodeType
	{
		public override BTNodeEnum Type {
			get { return BTNodeEnum.Decorator; }
		}

		public override int CanAddNodeCount {
			get { return BTConst.Normal_Decorator_CanAddNode; }
		}

		public override GUIStyle NormalStyle {
			get { return BTNodeStyle.DecoratorStyle; }
		}

		public override GUIStyle SelectStyle {
			get { return BTNodeStyle.SelectDecoratorStyle; }
		}

		public override ErrorType IsValid {
			get{ return BelongNode.ChildNodeList.Count == 1 ? ErrorType.None : ErrorType.Error; }
		}

		public Decorator (BTNode node) : base (node)
		{
		}
	}

	public class Root : Decorator
	{
		public override GUIStyle NormalStyle {
			get { return BTNodeStyle.RootStyle; }
		}

		public override GUIStyle SelectStyle {
			get { return BTNodeStyle.SelectRootStyle; }
		}

		public Root (BTNode node) : base (node)
		{
		}
	}

	public class Composite : BTNodeType
	{
		public override BTNodeEnum Type {
			get { return BTNodeEnum.Composite; }
		}

		public override int CanAddNodeCount {
			get { return BTConst.Normal_Composite_CanAddNode; }
		}

		public override GUIStyle NormalStyle {
			get { return BTNodeStyle.CompositeStyle; }
		}

		public override GUIStyle SelectStyle {
			get { return BTNodeStyle.SelectCompositeStyle; }
		}

		public override ErrorType IsValid {
			get { return BelongNode.IsHaveChild ? ErrorType.None : ErrorType.Error; }
		}

		public Composite (BTNode node) : base (node)
		{
		}
	}

	public class Task : BTNodeType
	{
		public override BTNodeEnum Type {
			get { return BTNodeEnum.Task; }
		}

		public override int CanAddNodeCount {
			get { return BTConst.Normal_Task_CanAddNode; }
		}

		public override GUIStyle NormalStyle {
			get { return BTNodeStyle.TaskStyle; }
		}

		public override GUIStyle SelectStyle {
			get { return BTNodeStyle.SelectTaskStyle; }
		}

		public override ErrorType IsValid {
			get { return ErrorType.None; }
		}

		public Task (BTNode node) : base (node)
		{
		}
	}
}