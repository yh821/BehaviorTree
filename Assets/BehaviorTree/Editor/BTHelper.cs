using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public static class BtHelper
	{
		private static string _gamePath = string.Empty;
		private static string _moduleAbsPath = string.Empty;
		private static string _modulePath = string.Empty;
		private static string _behaviorPath = string.Empty;
		private static string _jsonPath = string.Empty;
		private static string _nodePath = string.Empty;

        public static string GamePath
		{
			get
			{
				if (string.IsNullOrEmpty(_gamePath))
				{
					_gamePath = Path.Combine(Application.dataPath, "Game");
					_gamePath = _gamePath.Replace('\\', '/');
				}

				return _gamePath;
			}
		}

		public static string ModulePath(bool isAp = false)
		{
			if (string.IsNullOrEmpty(_modulePath))
			{
				_moduleAbsPath = Application.dataPath.Replace('\\', '/') + "/BehaviorTree";
				_modulePath = "Assets/BehaviorTree";
			}

			return isAp ? _moduleAbsPath : _modulePath;
		}

		public static string behaviorPath
		{
			get
			{
				if (string.IsNullOrEmpty(_behaviorPath))
					_behaviorPath = GamePath + "/Lua/behaviors";
				return _behaviorPath;
			}
		}

		public static string jsonPath
		{
			get
			{
				if (string.IsNullOrEmpty(_jsonPath))
					_jsonPath = ModulePath(true) + "/Editor/Json";
				return _jsonPath;
			}
		}

		public static string nodePath
		{
			get
			{
				if (string.IsNullOrEmpty(_nodePath))
					_nodePath = GamePath + "/Lua/nodes";
				return _nodePath;
			}
		}

		public static void CleanPath()
		{
			_modulePath = string.Empty;
			_behaviorPath = string.Empty;
			_jsonPath = string.Empty;
			_nodePath = string.Empty;
		}

		private static readonly Dictionary<string, string> mNodeTypeDict = new Dictionary<string, string>();

		private static Dictionary<string, Dictionary<string, string>> _nodeOptions;
		public static Dictionary<string, Dictionary<string, string>> nodeOptions => _nodeOptions ??= ReadBTNodeOption();

		public static string GenerateUniqueStringId()
		{
			return Guid.NewGuid().ToString("N");
		}

		public static void SaveBTData(BehaviourTree tree)
		{
			if (tree != null)
			{
				WalkNodeData(tree.Root);
				var content = JsonConvert.SerializeObject(tree.Root.Data, Formatting.Indented);
				File.WriteAllText(Path.Combine(jsonPath, $"{tree.Name}.json"), content);

				content = content.Replace("[", "{");
				content = content.Replace("]", "}");
				content = content.Replace(":", "=");
				var mc = Regex.Matches(content, "\"[a-zA-Z0-9_]+\"=");
				foreach (Match m in mc)
				{
					var word = m.Value.Replace("\"", "");
					content = content.Replace(m.Value, word);
				}

				mc = Regex.Matches(content, "\\s*[a-zA-Z0-9_]+= null,?");
				foreach (Match m in mc)
				{
					content = content.Replace(m.Value, "");
				}

				mc = Regex.Matches(content, "= \"[\\d.]+\",?");
				foreach (Match m in mc)
				{
					var word = m.Value.Replace("\"", "");
					content = content.Replace(m.Value, word);
				}

				mc = Regex.Matches(content, "= \"{\\S+}\"");
				foreach (Match m in mc)
				{
					var word = m.Value.Replace("\\", "");
					word = " =" + word.Substring(3, word.Length - 4);
					content = content.Replace(m.Value, word);
				}

				mc = Regex.Matches(content, "\\s*displayName= [^\\s}]+,?");
				foreach (Match m in mc)
				{
					content = content.Replace(m.Value, "");
				}

				mc = Regex.Matches(content, "\\s*desc= [^\\s}]+,?");
				foreach (Match m in mc)
				{
					content = content.Replace(m.Value, "");
				}

				content = $"local __bt__ = {content}\nreturn __bt__";
				File.WriteAllText(Path.Combine(behaviorPath, $"{tree.Name}.lua"), content);
			}
		}

		public static void WalkNodeData(BtNode parent)
		{
			parent.Data.name = parent.NodeName;
			parent.Data.SetPosition(parent.BtNodeGraph.RealRect.position);

			if (parent.IsHaveChild)
			{
				foreach (var node in parent.ChildNodeList)
				{
					WalkNodeData(node);
				}

				parent.Data.children.Sort((a, b) =>
				{
					if (a.posX > b.posX)
						return 1;
					if (a.posX < b.posX)
						return -1;
					if (a.posY > b.posY)
						return 1;
					if (a.posY < b.posY)
						return -1;
					return 0;
				});
			}
		}

		public static BehaviourTree LoadBehaviorTree(string file)
		{
			if (!File.Exists(file))
				return null;
			var content = File.ReadAllText(file);
			var data = JsonConvert.DeserializeObject<BtNodeData>(content);
			var tree = new BehaviourTree(Path.GetFileNameWithoutExtension(file), data);
			WalkJsonData(tree, tree.Root);
			return tree;
		}

		public static Dictionary<string, Dictionary<string, string>> ReadBTNodeOption()
		{
			var file = Path.Combine(Application.dataPath, "Editor/BTNodeDefaultOption.json");
			if (File.Exists(file))
			{
				var content = File.ReadAllText(file);
				return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(content);
			}

			return new Dictionary<string, Dictionary<string, string>>();
		}

		public static void WriteBtNodeOption(Dictionary<string, Dictionary<string, string>> data)
		{
			var content = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(Path.Combine(Application.dataPath, "Editor/BTNodeDefaultOption.json"), content);
			_nodeOptions = null;
		}

		public static void WalkJsonData(BehaviourTree owner, BtNode parent)
		{
			var childrenData = parent.Data.children;
			if (childrenData != null && childrenData.Count > 0)
			{
				foreach (var data in childrenData)
				{
					var child = AddChildNode(owner, parent, data);
					WalkJsonData(owner, child);
				}
			}
		}

		public static BtNode AddChildNode(BehaviourTree owner, BtNode parent, string name)
		{
			var pos = parent.BtNodeGraph.RealRect.position;
			if (!mNodeTypeDict.ContainsKey(name))
				throw new ArgumentNullException(name, "找不到该类型");
			var data = new BtNodeData(name, mNodeTypeDict[name], pos.x,
				pos.y + BtConst.DefaultHeight + BtConst.DefaultSpacingY);
			parent.Data.AddChild(data);
			return AddChildNode(owner, parent, data);
		}

		public static BtNode PasteChild(BehaviourTree owner, BtNode parent, float x, float y)
		{
			var nodeData = BtEditorWindow.CopyNode.Data.Clone();
			nodeData.SetPos(x, y);
			parent.Data.AddChild(nodeData);
			return AddChildNode(owner, parent, nodeData);
		}

		public static BtNode AddChildNode(BehaviourTree owner, BtNode parent, BtNodeData data)
		{
			data.SetPosition(owner.GenNodePos(data.GetPosition())); //避免重叠
			var child = new BtNode(owner, parent, data);
			owner.AddNode(child);
			parent.ChildNodeList.Add(child);
			return child;
		}

		public static void RemoveChild(BtNode node)
		{
			if (node.IsHaveChild)
			{
				foreach (var child in node.ChildNodeList)
				{
					child.Owner.AddBrokenNode(child);
					child.Parent = null;
				}
			}

			if (node.IsHaveParent)
			{
				node.Parent.ChildNodeList.Remove(node);
				node.Parent.Data.children.Remove(node.Data);
			}

			node.Owner.RemoveNode(node);
		}

		public static void AutoAlignPosition(BtNode node)
		{
			var width = (BtConst.DefaultWidth + BtConst.DefaultSpacingX) / 2;
			var multiW = Mathf.RoundToInt(node.BtNodeGraph.RealRect.x / width);
			float x = multiW * width;

			var height = (BtConst.DefaultHeight + BtConst.DefaultSpacingY) / 2;
			var multiH = Mathf.RoundToInt(node.BtNodeGraph.RealRect.y / height);
			float y = multiH * height;

			node.BtNodeGraph.RealRect.position = new Vector2(x, y);
		}

		public static void LoadNodeFile()
		{
			mNodeTypeDict.Clear();
			var files = Directory.GetFiles(nodePath, "*.lua", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				var sortPath = file.Replace("\\", "/");
				sortPath = sortPath.Replace(nodePath, "");
				if (sortPath.Contains("/actions/") ||
				    sortPath.Contains("/composites/") ||
				    sortPath.Contains("/decorators/"))
				{
					var fileName = Path.GetFileNameWithoutExtension(file);
					var type = sortPath.Substring(1, sortPath.LastIndexOf('/') - 1);
					mNodeTypeDict.Add(fileName, type);
				}
			}
		}

		public static BtNodeType CreateNodeType(BtNode node)
		{
			var key = node.NodeName;
			if (key == BtConst.RootName)
				return new Root(node);
			if (mNodeTypeDict.ContainsKey(key))
			{
				var type = mNodeTypeDict[key];
				switch (type)
				{
					case "actions":
						return new Task(node);
					case "composites":
						return new Composite(node);
					case "decorators":
						return new Decorator(node);
				}
			}

			throw new ArgumentNullException(node.NodeName, "找不到该节点");
		}

		public static GenericMenu GetGenericMenu(BtNode node, GenericMenu.MenuFunction2 callback)
		{
			var menu = new GenericMenu();
			if (!node.IsTask && node.ChildNodeList.Count < node.Type.CanAddNodeCount)
			{
				foreach (var kv in mNodeTypeDict)
				{
					var data = kv.Key.Replace("Node", "");
					var menuPath = $"{kv.Value}/{data}";
					menu.AddItem(new GUIContent(menuPath), false, callback, kv.Key);
				}

				if (BtEditorWindow.CopyNode != null)
				{
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("Paste Node"), false, callback, "Paste");
				}
			}

			if (!node.IsRoot)
			{
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Copy Node"), false, callback, "Copy");
				menu.AddItem(new GUIContent("Delete Node"), false, callback, "Delete");
			}

			return menu;
		}

		public static void SetNodeDefaultData(BtNode node, string name)
		{
			var data = node.Data;
			var options = nodeOptions;
			if (options.TryGetValue(name, out var option))
			{
				foreach (var kv in option)
				{
					if (kv.Key == "displayName")
						data.displayName = kv.Value;
					else
						data.AddData(kv.Key, kv.Value);
				}
			}
		}

	}
}