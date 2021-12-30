using System.Collections.Generic;
using UnityEngine;

namespace BT
{
	public class BtNodeData
	{
		public string displayName = string.Empty;
		public string desc = string.Empty;
		public string name = string.Empty;
		public string type = string.Empty;
		public float posX = 0;
		public float posY = 0;

		public Dictionary<string, string> data;

		public List<BtNodeData> children;

		public BtNodeData(string name, string type, float x, float y)
		{
			this.name = name;
			this.type = type;
			SetPos(x, y);
			displayName = name.Replace("Node", "");
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
			var clone = new BtNodeData(name, type, posX, posY) {displayName = displayName};
			if (data != null)
				clone.data = new Dictionary<string, string>(data);
			return clone;
		}

		public void SetPos(float x, float y)
		{
			posX = x;
			posY = y;
		}

		public Vector2 GetPosition()
		{
			return new Vector2(posX, posY);
		}

		public void SetPosition(Vector2 pos)
		{
			SetPos(pos.x, pos.y);
		}

    }
}