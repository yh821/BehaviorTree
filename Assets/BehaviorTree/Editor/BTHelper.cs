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
		private static string _toolPath = string.Empty;
		private static string _behaviorPath = string.Empty;
		private static string _jsonPath = string.Empty;
		private static string _nodePath = string.Empty;

		public static string ToolPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_toolPath)) return _toolPath;
				_toolPath = Path.Combine(Application.dataPath, "BehaviorTree/Editor");
				_toolPath = _toolPath.Replace('\\', '/');
				return _toolPath;
			}
		}

		public static string BehaviorPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_behaviorPath)) return _behaviorPath;
				_behaviorPath = Path.Combine(Application.dataPath, "Game/Lua/behavior/config");
				_behaviorPath = _behaviorPath.Replace('\\', '/');
				return _behaviorPath;
			}
		}

		public static string JsonPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_jsonPath)) return _jsonPath;
				_jsonPath = Path.Combine(ToolPath, "Json");
				_jsonPath = _jsonPath.Replace('\\', '/');
				return _jsonPath;
			}
		}

		public static string NodePath
		{
			get
			{
				if (!string.IsNullOrEmpty(_nodePath)) return _nodePath;
				_nodePath = Path.Combine(Application.dataPath, "Game/Lua/behavior/nodes");
				_nodePath = _nodePath.Replace('\\', '/');
				return _nodePath;
			}
		}

		public static void CleanPath()
		{
			_behaviorPath = string.Empty;
			_jsonPath = string.Empty;
			_nodePath = string.Empty;
		}

		private static readonly Dictionary<string, string> NodeTypeDict = new Dictionary<string, string>();

		private static Dictionary<string, Dictionary<string, string>> _nodeOptions;
		public static Dictionary<string, Dictionary<string, string>> NodeOptions => _nodeOptions ??= ReadBtNodeOption();

		public static string GenerateUniqueStringId()
		{
			return Guid.NewGuid().ToString("N");
		}

		public static void SaveBtData(BehaviourTree tree)
		{
			if (tree == null) return;
			FlushNodeData(tree.Root);
			var content = JsonConvert.SerializeObject(tree.Root.Data, Formatting.Indented);
			File.WriteAllText(Path.Combine(JsonPath, $"{tree.Name}.json"), content);

			var luaData = SwitchToLua(tree.Root.Data);
			content = JsonConvert.SerializeObject(luaData, Formatting.Indented);

			content = content.Replace("[", "{");
			content = content.Replace("]", "}");
			content = content.Replace(":", "=");
			var mc = Regex.Matches(content, "\"[a-zA-Z0-9_]+\"=");
			foreach (Match m in mc)
			{
				var word = m.Value.Replace("\"", "");
				content = content.Replace(m.Value, word);
			}

			mc = Regex.Matches(content, "\\s*[a-zA-Z0-9_]+= \\{\\},?");
			foreach (Match m in mc)
			{
				content = content.Replace(m.Value, "");
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

			content = $"local __bt__ = {content}\nreturn __bt__";
			File.WriteAllText(Path.Combine(BehaviorPath, $"{tree.Name}.lua"), content);
		}

		public static void FlushNodeData(BtNode node)
		{
			WalkNodeData(node);
			_nodeIndex = 0;
			WalkNodeIndex(node);
		}

		public static void WalkNodeData(BtNode node)
		{
			node.Data.file = node.NodeName;
			node.Data.SetPosition(node.Graph.RealRect.position);
			if (node.IsHaveChild)
			{
				foreach (var child in node.ChildNodeList)
				{
					WalkNodeData(child);
				}

				node.ChildNodeList.Sort(SortNodeList);
				node.Data.children.Sort(SortNodeList);
			}
		}

		private static int _nodeIndex;

		public static void WalkNodeIndex(BtNode node)
		{
			node.Data.index = _nodeIndex++;
			if (node.IsHaveChild)
				foreach (var child in node.ChildNodeList)
					WalkNodeIndex(child);
		}

		private static int SortNodeList(BtNode a, BtNode b)
		{
			return SortNodeList(a.Data.posX, a.Data.posY, b.Data.posX, b.Data.posY);
		}

		private static int SortNodeList(BtNodeData a, BtNodeData b)
		{
			return SortNodeList(a.posX, a.posY, b.posX, b.posY);
		}

		private static int SortNodeList(float ax, float ay, float bx, float by)
		{
			if (ax > bx)
				return 1;
			if (ax < bx)
				return -1;
			if (ay > by)
				return 1;
			if (ay < by)
				return -1;
			return 0;
		}

		public static BtNodeLua SwitchToLua(BtNodeData data)
		{
			if (!data.enabled) return null;
			var lua = new BtNodeLua
				{file = data.file, type = data.type, data = data.data, sharedData = data.sharedData};
			if (data.children != null && data.children.Count > 0)
			{
				foreach (var child in data.children)
				{
					var childLua = SwitchToLua(child);
					if (childLua == null) continue;
					if (lua.children == null) lua.children = new List<BtNodeLua>();
					lua.children.Add(childLua);
				}
			}
			return lua;
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

		public static Dictionary<string, Dictionary<string, string>> ReadBtNodeOption()
		{
			var file = Path.Combine(ToolPath, "BTNodeOption.json");
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
			File.WriteAllText(Path.Combine(ToolPath, "BTNodeOption.json"), content);
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

		public static BtNode AddChildNode(BehaviourTree owner, BtNode parent, string file)
		{
			var pos = parent.Graph.RealRect.position;
			if (!NodeTypeDict.ContainsKey(file))
				throw new ArgumentNullException(file, "找不到该类型");
			var data = new BtNodeData(file, NodeTypeDict[file], pos.x,
				pos.y + BtConst.DefaultHeight + BtConst.DefaultSpacingY);
			parent.Data.AddChild(data);
			return AddChildNode(owner, parent, data);
		}

		public static BtNode PasteChild(BehaviourTree owner, BtNode parent, Vector2 pos)
		{
			return PasteChild(owner, parent, pos.x, pos.y);
		}

		public static BtNode PasteChild(BehaviourTree owner, BtNode parent, float x, float y)
		{
			var nodeData = BtEditorWindow.CopyNode.Data.Clone();
			nodeData.SetPosition(x, y);
			parent?.Data.AddChild(nodeData);
			return AddChildNode(owner, parent, nodeData);
		}

		public static BtNode AddChildNode(BehaviourTree owner, BtNode parent, BtNodeData data)
		{
			data.SetPosition(owner.GenNodePos(data.GetPosition())); //避免重叠
			var child = new BtNode(owner, parent, data);
			owner.AddNode(child);
			parent?.ChildNodeList.Add(child);
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
			var multiW = Mathf.RoundToInt(node.Graph.RealRect.x / width);
			float x = multiW * width;

			var height = (BtConst.DefaultHeight + BtConst.DefaultSpacingY) / 2;
			var multiH = Mathf.RoundToInt(node.Graph.RealRect.y / height);
			float y = multiH * height;

			node.Graph.RealRect.position = new Vector2(x, y);
		}

		public static void LoadNodeFile()
		{
			NodeTypeDict.Clear();
			var files = Directory.GetFiles(NodePath, "*.lua", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				var sortPath = file.Replace("\\", "/");
				sortPath = sortPath.Replace(NodePath + "/", "");
				var fileName = Path.GetFileNameWithoutExtension(file);
				var type = sortPath.Substring(0, sortPath.LastIndexOf('.'));
				NodeTypeDict.Add(fileName, type);
			}
		}

		public static BtNodeType CreateNodeType(BtNode node)
		{
			var key = node.NodeName;
			if (key == BtConst.RootName)
				return new Root(node);
			if (NodeTypeDict.ContainsKey(key))
			{
				var type = NodeTypeDict[key];
				if (type.StartsWith("composites/SelectorNode"))
					return new Selector(node);
				if (type.StartsWith("composites/SequenceNode"))
					return new Sequence(node);
				if (type.StartsWith("composites/ParallelNode"))
					return new Parallel(node);
				if (type.StartsWith("conditions/IsTriggerNode"))
					return new IsTriggerNode(node);
				if (type.StartsWith("decorators/TriggerNode"))
					return new TriggerNode(node);

				if (type.StartsWith("actions/"))
					return new Action(node);
				if (type.StartsWith("composites/"))
					return new Composite(node);
				if (type.StartsWith("conditions/"))
					return new Condition(node);
				if (type.StartsWith("decorators/"))
					return new Decorator(node);
			}

			throw new ArgumentNullException(node.NodeName, "找不到该节点");
		}

		public static GenericMenu GetGenericMenu(BtNode node, GenericMenu.MenuFunction2 callback)
		{
			var menu = new GenericMenu();
			if (node.ChildNodeList.Count < node.NodeType.CanAddNodeCount)
			{
				foreach (var kv in NodeTypeDict)
				{
					//var data = kv.Key.Replace("Node", "")
					menu.AddItem(new GUIContent(kv.Value), false, callback, kv.Key);
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
			var options = NodeOptions;
			if (options.TryGetValue(name, out var option))
			{
				foreach (var kv in option)
				{
					if (kv.Key == "name")
						data.name = kv.Value;
					else
						data.AddData(kv.Key, kv.Value);
				}
			}
		}

		public static bool CheckKey(string key)
		{
			if (string.IsNullOrEmpty(key))
				return false;
			if (key == "name" || key == "file" || key == "type"
			    || key == "data" || key == "desc" || key == "children")
				return false; //保留字段
			return true;
		}

		public static List<string> GetAllFiles(string path, string extension)
		{
			if (!Directory.Exists(path)) return null;
			var names = new List<string>();
			var root = new DirectoryInfo(path);
			var files = root.GetFiles();

			foreach (var file in files)
			{
				var ext = Path.GetExtension(file.FullName);
				if (extension == ext)
					names.Add(file.FullName);
			}

			var dirs = root.GetDirectories();
			foreach (var dir in dirs)
			{
				var subNames = GetAllFiles(dir.FullName, extension);
				foreach (var subName in subNames)
				{
					names.Add(subName);
				}
			}

			return names;
		}

		public static void OpenFile(string fullPath)
		{
			var path = fullPath.Replace("/", "\\");
			if (File.Exists(path))
				System.Diagnostics.Process.Start(path);
		}
	}
}