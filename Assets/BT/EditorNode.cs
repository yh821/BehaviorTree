using System;
using System.Text;

namespace BT
{
	[Serializable]
	public abstract class EditorNode
	{
		/// <summary>
		/// 归属图形化节点
		/// </summary>
		protected BTNode BelongNode { get; set; }

		/// <summary>
		/// 节点类型
		/// </summary>
		public abstract EditorNodeEnum NodeEnum { get; }

		/// <summary>
		/// 节点是否有效
		/// </summary>
		/// <returns></returns>
		public abstract ErrorType GetIsVaild ();

		/// <summary>
		/// 可添加节点数量
		/// </summary>
		public abstract int CanAddNodeCount { get; }

		/// <summary>
		/// 设置属于的图形节点
		/// </summary>
		/// <param name="node"></param>
		public void SetBelongNode (BTNode node)
		{
			BelongNode = node;
		}

		/// <summary>
		/// 处理特殊的属性
		/// </summary>
		/// <param name="sb"></param>
		protected void DealField (StringBuilder sb)
		{
		}

		/// <summary>
		/// 输出代码格式
		/// </summary>
		/// <returns></returns>
		public abstract string ToChild ();
	}

	public class Composite : EditorNode
	{
		public override EditorNodeEnum NodeEnum {
			get { return EditorNodeEnum.Composite; }
		}

		public override int CanAddNodeCount {
			get { return EditorNodeConst.Normal_Composite_CanAddNode; }
		}

		public override ErrorType GetIsVaild ()
		{
			return BelongNode.DefaultNode != null ? ErrorType.None : ErrorType.Error;
		}

		public override string ToChild ()
		{
			return "";
		}
	}

	public class Decorator : EditorNode
	{
		public override EditorNodeEnum NodeEnum {
			get { return EditorNodeEnum.Decorator; }
		}

		public override int CanAddNodeCount {
			get { return EditorNodeConst.Normal_Decorator_CanAddNode; }
		}

		public override ErrorType GetIsVaild ()
		{
			return BelongNode.DefaultNode != null ? ErrorType.None : ErrorType.Error;
		}

		public override string ToChild ()
		{
			return "";
		}
	}

	public class Task : EditorNode
	{
		public override EditorNodeEnum NodeEnum {
			get { return EditorNodeEnum.Task; }
		}

		public override int CanAddNodeCount {
			get { return EditorNodeConst.Normal_Task_CanAddNode; }
		}

		public override ErrorType GetIsVaild ()
		{
			return ErrorType.None;
		}

		public override string ToChild ()
		{
			return "";
		}
	}
}