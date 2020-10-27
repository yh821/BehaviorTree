using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BT
{
	public class BTEditorProperty
	{
		#region 单例

		private static BTEditorProperty _instance = null;

		public static BTEditorProperty Instance {
			get { return _instance; }
			set { _instance = value; }
		}

		#endregion

		/// <summary>
		/// 当前移动坐标 鼠标拖拽背景偏移
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// 当前选中的节点
		/// </summary>
		private Dictionary<string,BTNode> mSelectNodeDic;

		public int SelectNodeCount {
			get { return mSelectNodeDic.Count; }
		}

		public BTNode DefaultNode {
			get { return mSelectNodeDic.First ().Value; }
		}

		public Event Event {
			get {
				if (Event.current == null) {
					Event evt = new Event { type = EventType.Ignore };
					return evt;
				}

				return Event.current;
			}
		}

		public BTEditorProperty ()
		{
			Position = Vector2.zero;
			mSelectNodeDic = new Dictionary<string, BTNode> ();
		}

		public bool GetIsSelectNode (BTNode node)
		{
			return mSelectNodeDic.ContainsKey (node.Guid);
		}

		public void AddSelectNode (string id, BTNode node)
		{
			if (mSelectNodeDic.ContainsKey (id)) {
				return;
			}   
			mSelectNodeDic.Add (id, node);
		}

		public void AddSelectNode (BTNode node)
		{
			if (mSelectNodeDic.ContainsKey (node.Guid)) {
				return;
			}   
			mSelectNodeDic.Add (node.Guid, node);
		}

		public void RemoveSelectNode (string id)
		{
			if (mSelectNodeDic.ContainsKey (id)) {
				mSelectNodeDic.Remove (id);
			}
		}

		public void RemoveSelectNode (BTNode node)
		{
			if (mSelectNodeDic.ContainsKey (node.Guid)) {
				mSelectNodeDic.Remove (node.Guid);
			}
		}
	}
}