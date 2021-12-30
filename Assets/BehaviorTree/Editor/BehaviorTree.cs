using UnityEngine;
using System.Collections.Generic;

namespace BT
{
	public class BehaviourTree
	{
		public Dictionary<string, BtNode> NodeDict;
		public Dictionary<string, BtNode> BrokenNodeDict;
		public Dictionary<string, int> NodePosDict;

		public string Name { get; set; }

		public BtNode Root { get; }

		public BehaviourTree()
		{
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
		}

		public BehaviourTree(string name, BtNodeData data = null)
		{
			Name = name;
			NodeDict = new Dictionary<string, BtNode>();
			NodePosDict = new Dictionary<string, int>();
			BrokenNodeDict = new Dictionary<string, BtNode>();
			if (data == null)
			{
				data = new BtNodeData(BtConst.RootName, null,
					(BtEditorWindow.Window.position.width - BtConst.RightInspectWidth) / 2, 50);
				data.AddData("restart", "1");
			}

			Root = new BtNode(this, null, data);
			AddNode(Root);
			BtHelper.AutoAlignPosition(Root);
		}

		public void Update(Rect canvas)
		{
			if (NodeDict == null) return;
			foreach (var node in NodeDict.Values)
			{
				node.Update(canvas);
			}
		}

		public void AddNode(BtNode node)
		{
			NodeDict.Add(node.Guid, node);
			//用于判断是否重叠
			var key = node.Data.GetPosition().ToString();
			if (NodePosDict.TryGetValue(key, out var count))
				NodePosDict[key] = count + 1;
			else
				NodePosDict.Add(key, 1);
		}

		public void RemoveNode(BtNode node)
		{
			NodeDict.Remove(node.Guid);
			BrokenNodeDict.Remove(node.Guid);
			NodePosDict.Remove(node.Data.GetPosition().ToString());
		}

		public void AddBrokenNode(BtNode node)
		{
			BrokenNodeDict.Add(node.Guid, node);
		}

		public void RemoveBrokenNode(BtNode node)
		{
			BrokenNodeDict.Remove(node.Guid);
		}

		public Vector2 GenNodePos(Vector2 pos)
		{
			while (true)
			{
				if (NodePosDict.ContainsKey(pos.ToString()))
				{
					pos.y += (BtConst.DefaultHeight + BtConst.DefaultSpacingY) / 2f;
					continue;
				}

				NodePosDict.Add(pos.ToString(), 1);
				return pos;
			}
		}
	}
}