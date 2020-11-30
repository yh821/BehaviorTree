using System;
using UnityEngine;
using System.Collections.Generic;

namespace BT
{
	public class BehaviourTree
	{
		public Dictionary<string, BTNode> BTNodeDic;

		public string Name { get; set; }

		public BTNode Root { get; }

		public BehaviourTree ()
		{
			BTNodeDic = new Dictionary<string, BTNode> ();     
		}

		public BehaviourTree (string name, BTNodeData data = null)
		{
			Name = name;
			BTNodeDic = new Dictionary<string, BTNode> ();
			if (data == null) {
				data = new BTNodeData (BTConst.RootName, null, 
					(BTConst.WINDOWS_WIDTH - BTConst.LEFT_INSPECT_WIDTH) / 2 - BTConst.DefaultWidth / 2, 50);
				data.AddData ("restartOnComplete", "1");
			}
			Root = new BTNode (this, null, data);
			AddNode (Root);
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