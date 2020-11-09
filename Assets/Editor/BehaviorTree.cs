using System;
using UnityEngine;
using System.Collections.Generic;

namespace BT
{
	public class BehaviourTree
	{
		public Dictionary<string, BTNode> BTNodeDic;

		/// <summary>
		/// 根节点
		/// </summary>
		public BTNode root;

		public BehaviourTree ()
		{
			BTNodeDic = new Dictionary<string, BTNode> ();     
		}

		public void CreateRoot ()
		{
			//取一个中间的位子
			root = new BTNode (this, "Root", new Decorator (), 
				new Rect ((BTEditorConst.WINDOWS_WIDTH - BTEditorConst.LEFT_INSPECT_WIDTH) / 2 - 
					BTEditorConst.Default_Width / 2, 
					50, BTEditorConst.Default_Width, BTEditorConst.Default_Height));
			AddNode (root);
		}

		public void Update (Rect canvas)
		{
			foreach (var node in BTNodeDic.Values) {
				node.Update (canvas);
			}
		}

		public void AddNode (BTNode node)
		{
			BTNodeDic.Add (node.Guid, node);
		}

		public void RemoveNode (BTNode node)
		{
			BTNodeDic.Remove (node.Guid);
		}
	}
}