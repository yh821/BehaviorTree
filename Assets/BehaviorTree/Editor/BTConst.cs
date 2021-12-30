namespace BT
{
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
		/// 左侧监视面板宽度
		/// </summary>
		public const float RightInspectWidth = 230;

		/// <summary>
		/// 节点默认宽度
		/// </summary>
		public const int DefaultWidth = 120;

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
}