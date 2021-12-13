using UnityEngine;

namespace BT
{
	public class BTNodeGraph
	{
		/// <summary>
		/// 实际节点
		/// </summary>
		public Rect RealRect;

		/// <summary>
		/// 节点范围
		/// </summary>
		public Rect NodeRect {
			get {
				Rect ret = RealRect;
				ret.position += BTEditorWindow.window.Position;
				return ret;
			}
		}

		/// <summary>
		/// 下部连接点
		/// </summary>
		public Rect DownPointRect {
			get {
				return new Rect (NodeRect.center.x - BTConst.LINE_POINT_LENGTH / 2, NodeRect.yMax,
					BTConst.LINE_POINT_LENGTH, BTConst.LINE_POINT_LENGTH);
			}
		}

		/// <summary>
		/// 上部连接点
		/// </summary>
		public Rect UpPointRect {
			get {
				return new Rect (NodeRect.center.x - BTConst.LINE_POINT_LENGTH / 2,
					NodeRect.yMin - BTConst.LINE_POINT_LENGTH,
					BTConst.LINE_POINT_LENGTH, BTConst.LINE_POINT_LENGTH);
			}
		}

		/// <summary>
		/// 错误节点范围
		/// </summary>
		public Rect ErrorRect {
			get {
				return new Rect (NodeRect.x + 5, NodeRect.y + 5,
					BTConst.LINE_POINT_LENGTH, BTConst.LINE_POINT_LENGTH);
			}
		}
	}
}