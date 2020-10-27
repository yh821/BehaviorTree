using System;

namespace BT
{
	public enum EditorNodeEnum
	{
		Decorator,
		Composite,
		Task
	}

	public enum ErrorType
	{
		Warn,
		Error,
		None
	}

	public class EditorNodeConst
	{
		/// <summary>
		/// 装饰节点 一般可添加子节点
		/// </summary>
		public const int Normal_Decorator_CanAddNode = 1;
		/// <summary>
		/// 复合节点 一般可添加子节点
		/// </summary>
		public const int Normal_Composite_CanAddNode = 999;
		/// <summary>
		/// 任务节点 一般可添加子节点
		/// </summary>
		public const int Normal_Task_CanAddNode = 0;
	}

	public class BTEditorConst
	{
		public const float WINDOWS_WIDTH = 1280;
		public const float WINDOWS_HEIGHT = 768;

		//贝塞尔曲线相关
		public const int BEZIER_WIDTH = 3;

		public const int TOP_TOOLBAR_HEIGHT = 24;

		/// <summary>
		/// 取消连线的按钮大小
		/// </summary>
		public const float LINE_DISABLE_LENGTH = 30;

		/// <summary>
		/// 连接点半径
		/// </summary>
		public const float LINE_POINT_LENGTH = 24;

		/// <summary>
		/// 左侧监视面板宽度
		/// </summary>
		public const float LEFT_INSPECT_WIDTH = 300;

		/// <summary>
		/// 节点默认宽度
		/// </summary>
		public const int Default_Width = 120;
		/// <summary>
		/// 节点默认高度
		/// </summary>
		public const int Default_Height = 70;
		/// <summary>
		/// 节点默认高度
		/// </summary>
		public const int Default_Distance = 150;
	}

	public static class BTUtils
	{
		public static string GenerateUniqueStringID ()
		{
			return Guid.NewGuid ().ToString ("N");
		}

		public static string GetTypeName (string fullName)
		{
			string str = fullName.Substring (fullName.IndexOf ('.') + 1);
			return str;
		}

		public static string GetTypeName (Type type)
		{
			return GetTypeName (type.FullName);
		}
	}
}