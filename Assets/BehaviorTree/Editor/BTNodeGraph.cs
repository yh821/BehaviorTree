using UnityEngine;

namespace BT
{
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
}