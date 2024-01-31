using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BT
{
	public enum TaskType
	{
		Composite,
		Decorator,
		Condition,
		Action,
		Root,
		Selector,
		Sequence,
		Parallel,
		Trigger,
		IsTrigger,
	}

	public enum AbortType
	{
		None,
		Self,
		Lower,
		Both
	}

	public enum TriggerType
	{
		Equals,
		NotEqual,
		Greater,
		Less
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
		/// 连线颜色
		/// </summary>
		public static readonly Color LineColor = Color.white;

		/// <summary>
		/// 连接点半径
		/// </summary>
		public const float LinePointLength = 24;

		/// <summary>
		/// 加号半径
		/// </summary>
		public const float LinePlusLength = 16;

		/// <summary>
		/// 选中框半径
		/// </summary>
		public const float ToggleLength = 18;

		/// <summary>
		/// 图标尺寸
		/// </summary>
		public const float IconSize = 40;

		/// <summary>
		/// 左侧监视面板宽度
		/// </summary>
		public const float InspectWidth = 240;

		/// <summary>
		/// 左侧监视面板高度
		/// </summary>
		public const float NodeInspectHeight = 300;

		/// <summary>
		/// 左侧监视面板高度
		/// </summary>
		public const float TreeInspectHeight = 440;

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
		public const string RootName = "RootNode";

		public const string Restart = "restart";
		public const string AbortType = "abortType";
		public const string TriggerType = "triggerType";
		public const string TriggerValue = "triggerValue";
	}

	public class BtNodeLua
	{
		public string file;
		public string type;
		public Dictionary<string, string> sharedData;
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
		public bool isOn = true; //是否勾上启用
		private bool lastIsOn = true;
		public bool enabled = true; //是否启用节点
		public bool fold = false; //是否折叠子节点
		public bool visable = true; //是否显示

		public Dictionary<string, string> data;
		public Dictionary<string, string> sharedData; //共享数据,只存放在根节点里

		public List<BtNodeData> children;

		public BtNodeData(string file, string type, float x, float y)
		{
			this.file = file;
			this.type = type;
			SetPosition(x, y);
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
			var clone = new BtNodeData(file, type, posX, posY) {name = name};
			if (data != null)
				clone.data = new Dictionary<string, string>(data);
			return clone;
		}

		public void AddSharedData(string key, string value)
		{
			if (sharedData == null)
				sharedData = new Dictionary<string, string>();
			if (sharedData.ContainsKey(key))
				sharedData[key] = value;
			else
				sharedData.Add(key, value);
		}

		public void RemoveSharedData(string key)
		{
			if (sharedData != null && sharedData.ContainsKey(key))
				sharedData.Remove(key);
		}

		public Vector2 GetPosition()
		{
			return new Vector2(posX, posY);
		}

		public void SetPosition(float x, float y)
		{
			posX = x;
			posY = y;
		}

		public void SetPosition(Vector2 pos)
		{
			SetPosition(pos.x, pos.y);
		}

		public bool IsChangeToggle(bool curIsOn)
		{
			if (curIsOn == lastIsOn) return false;
			lastIsOn = curIsOn;
			return true;
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
		public abstract GUIStyle SelectStyle { get; }

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
		public Root(BtNode node) : base(node) { }
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
		public override GUIStyle SelectStyle => BtNodeStyle.SelectDecoratorStyle;
		public override ErrorType IsValid => BelongNode.ChildNodeList.Count == 1 ? ErrorType.None : ErrorType.Error;
		public Decorator(BtNode node) : base(node) { }
	}

	public class Composite : BtNodeType
	{
		public override TaskType Type => TaskType.Composite;
		public override int CanAddNodeCount => BtConst.NormalCompositeCanAddNode;
		public override GUIStyle NormalStyle => BtNodeStyle.CompositeStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectCompositeStyle;
		public override ErrorType IsValid => BelongNode.IsHaveChild ? ErrorType.None : ErrorType.Error;
		public Composite(BtNode node) : base(node) { }
	}

	public class Condition : BtNodeType
	{
		public override TaskType Type => TaskType.Condition;
		public override int CanAddNodeCount => BtConst.NormalTaskCanAddNode;
		public override GUIStyle NormalStyle => BtNodeStyle.ConditionStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectConditionStyle;
		public override ErrorType IsValid => ErrorType.None;
		public Condition(BtNode node) : base(node) { }
	}

	public class Action : BtNodeType
	{
		public override TaskType Type => TaskType.Action;
		public override int CanAddNodeCount => BtConst.NormalTaskCanAddNode;
		public override GUIStyle NormalStyle => BtNodeStyle.ActionStyle;
		public override GUIStyle SelectStyle => BtNodeStyle.SelectTaskStyle;
		public override ErrorType IsValid => ErrorType.None;
		public Action(BtNode node) : base(node) { }
	}

	#region CustomType

	public class Selector : Composite
	{
		public override TaskType Type => TaskType.Selector;
		public Selector(BtNode node) : base(node) { }

		public override Texture GetIcon()
		{
			return BtNodeStyle.SelectorIcon;
		}
	}

	public class Sequence : Composite
	{
		public override TaskType Type => TaskType.Sequence;
		public Sequence(BtNode node) : base(node) { }

		public override Texture GetIcon()
		{
			return BtNodeStyle.SequenceIcon;
		}
	}

	public class Parallel : Composite
	{
		public override TaskType Type => TaskType.Parallel;
		public Parallel(BtNode node) : base(node) { }

		public override Texture GetIcon()
		{
			return BtNodeStyle.ParallelIcon;
		}
	}

	public class TriggerNode : Decorator
	{
		public override TaskType Type => TaskType.Trigger;
		public TriggerNode(BtNode node) : base(node) { }
	}

	public class IsTriggerNode : Condition
	{
		public override TaskType Type => TaskType.IsTrigger;
		public IsTriggerNode(BtNode node) : base(node) { }
	}

	#endregion

	public static class BtNodeStyle
	{
		public static GUIStyle RootStyle => "flow node 0";
		public static GUIStyle SelectRootStyle => "flow node 0 on";

		public static GUIStyle DecoratorStyle => "flow node 2";
		public static GUIStyle SelectDecoratorStyle => "flow node 2 on";

		public static GUIStyle CompositeStyle => "flow node 1";
		public static GUIStyle SelectCompositeStyle => "flow node 1 on";

		public static GUIStyle ActionStyle => "flow node 3";
		public static GUIStyle SelectTaskStyle => "flow node 3 on";

		public static GUIStyle ConditionStyle => "flow node 2"; //5";
		public static GUIStyle SelectConditionStyle => "flow node 2 on"; //5
		public static GUIStyle IndexStyle => "AssetLabel";


		private static GUIContent _linePoint;
		public static GUIContent LinePoint => _linePoint ??= EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo");

		private static GUIContent _warnPoint;
		public static GUIContent WarnPoint => _warnPoint ??= EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo");

		private static GUIContent _errorPoint;
		public static GUIContent ErrorPoint => _errorPoint ??= EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo");

		private static GUIContent _foldoutPlus;
		public static GUIContent FoldoutPlus => _foldoutPlus ??= EditorGUIUtility.IconContent("P4_AddedLocal");

		private static Texture _rootIcon;

		public static Texture RootIcon
		{
			get
			{
				if (_rootIcon == null)
				{
					var path = BtHelper.ToolPath + "/GUI/root.png";
					path = FileUtil.GetProjectRelativePath(path);
					_rootIcon = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _rootIcon;
			}
		}

		private static Texture _selectorIcon;

		public static Texture SelectorIcon
		{
			get
			{
				if (_selectorIcon == null)
				{
					var path = BtHelper.ToolPath + "/GUI/selector.png";
					path = FileUtil.GetProjectRelativePath(path);
					_selectorIcon = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _selectorIcon;
			}
		}

		private static Texture _sequenceIcon;

		public static Texture SequenceIcon
		{
			get
			{
				if (_sequenceIcon == null)
				{
					var path = BtHelper.ToolPath + "/GUI/sequence.png";
					path = FileUtil.GetProjectRelativePath(path);
					_sequenceIcon = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _sequenceIcon;
			}
		}

		private static Texture _parallelIcon;

		public static Texture ParallelIcon
		{
			get
			{
				if (_parallelIcon == null)
				{
					var path = BtHelper.ToolPath + "/GUI/parallel.png";
					path = FileUtil.GetProjectRelativePath(path);
					_parallelIcon = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _parallelIcon;
			}
		}

		private static Texture _abortSelfLogo;

		public static Texture AbortSelfLogo
		{
			get
			{
				if (_abortSelfLogo == null)
				{
					var path = BtHelper.ToolPath + "/GUI/self.png";
					path = FileUtil.GetProjectRelativePath(path);
					_abortSelfLogo = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _abortSelfLogo;
			}
		}

		private static Texture _abortLowerLogo;

		public static Texture AbortLowerLogo
		{
			get
			{
				if (_abortLowerLogo == null)
				{
					var path = BtHelper.ToolPath + "/GUI/lower.png";
					path = FileUtil.GetProjectRelativePath(path);
					_abortLowerLogo = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _abortLowerLogo;
			}
		}

		private static Texture _abortBothLogo;

		public static Texture AbortBothLogo
		{
			get
			{
				if (_abortBothLogo == null)
				{
					var path = BtHelper.ToolPath + "/GUI/both.png";
					path = FileUtil.GetProjectRelativePath(path);
					_abortBothLogo = AssetDatabase.LoadAssetAtPath<Texture>(path);
				}
				return _abortBothLogo;
			}
		}
	}
}