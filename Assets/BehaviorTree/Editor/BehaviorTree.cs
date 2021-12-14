using UnityEngine;
using System.Collections.Generic;

namespace BT
{
	public class BehaviourTree
	{
		public Dictionary<string, BTNode> BTNodeDict;
		public Dictionary<string, BTNode> OrphanNodeDict;

		public string Name { get; set; }

		public BTNode Root { get; }

		public BehaviourTree ()
		{
			BTNodeDict = new Dictionary<string, BTNode> ();     
		}

		public BehaviourTree (string name, BTNodeData data = null)
		{
			Name = name;
			BTNodeDict = new Dictionary<string, BTNode> ();
			OrphanNodeDict = new Dictionary<string, BTNode> ();
			if (data == null) {
				data = new BTNodeData (BTConst.RootName, null, 
					(BTEditorWindow.window.position.width - BTConst.RIGHT_INSPECT_WIDTH) / 2, 50);
				data.AddData ("restart", "1");
			}
			Root = new BTNode (this, null, data);
			AddNode (Root);
			BTHelper.AutoAlignPosition (Root);
		}

		public void Update (Rect canvas)
		{
			if (BTNodeDict != null) {
				foreach (var node in BTNodeDict.Values) {
					node.Update (canvas);
				}
			}
		}

		public void AddNode (BTNode node)
		{
			BTNodeDict.Add (node.Guid, node);
		}

		public void RemoveNode (BTNode node)
		{
			BTNodeDict.Remove (node.Guid);
		}

		public void AddOrphanNode (BTNode node)
		{
			OrphanNodeDict.Add (node.Guid, node);
		}

		public void RemoveOrphanNode (BTNode node)
		{
			if (OrphanNodeDict.ContainsKey (node.Guid))
				OrphanNodeDict.Remove (node.Guid);
		}
	}
}