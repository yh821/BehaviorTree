using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Common;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class BTConst
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
		/// 窗口默认尺寸
		/// </summary>
		public const float WINDOWS_WIDTH = 1280;
		public const float WINDOWS_HEIGHT = 768;

		//贝塞尔曲线相关
		public const int BEZIER_WIDTH = 3;

		/// <summary>
		/// 取消连线的按钮大小
		/// </summary>
		public const float LINE_DISABLE_LENGTH = 8;

		/// <summary>
		/// 连接点半径
		/// </summary>
		public const float LINE_POINT_LENGTH = 24;

		/// <summary>
		/// 左侧监视面板宽度
		/// </summary>
		public const float LEFT_INSPECT_WIDTH = 240;

		/// <summary>
		/// 节点默认宽度
		/// </summary>
		public const int DefaultWidth = 120;
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
		public const string RootName = "rootNode";
	}

	public class BTNodeData
	{
		public string name = string.Empty;
		public string type = string.Empty;
		public float posX = 0;
		public float posY = 0;

		public Dictionary<string, string> data;

		public List<BTNodeData> children;

		public BTNodeData (string name, string type, float x, float y)
		{
			this.name = name;
			this.type = type;
			posX = x;
			posY = y;
		}

		public void AddChild (BTNodeData child)
		{
			if (children == null)
				children = new List<BTNodeData> ();
			children.Add (child);
		}

		public void AddData (string key, string value)
		{
			if (data == null)
				data = new Dictionary<string, string> ();
			if (data.ContainsKey (key))
				data [key] = value;
			else
				data.Add (key, value);
		}

		public void RemoveData (string key)
		{
			if (data != null && data.ContainsKey (key))
				data.Remove (key);
		}
	}

	public static class BTHelper
	{
		static private string _clientPath = string.Empty;

		static public string clientPath {
			get {
				if (string.IsNullOrEmpty (_clientPath)) {
					_clientPath = Application.dataPath.Replace ("/Assets", "");
					_clientPath = _clientPath.Replace ("\\", "/");
				}
				return _clientPath;
			}
		}

		static private string _behaviorPath = string.Empty;

		static public string behaviorPath {
			get {
				if (string.IsNullOrEmpty (_behaviorPath)) {
					_behaviorPath = Path.Combine (clientPath, "LocalFile/behaviors");
					_behaviorPath = _behaviorPath.Replace ("\\", "/");
				}
				return _behaviorPath;
			}
		}

		static private string _jsonPath = string.Empty;

		static public string jsonPath {
			get {
				if (string.IsNullOrEmpty (_jsonPath)) {
					_jsonPath = Path.Combine (Application.dataPath, "Editor/Json");
					_jsonPath = _jsonPath.Replace ("\\", "/");
				}
				return _jsonPath;
			}
		}

		static private string _nodePath = string.Empty;

		static public string nodePath {
			get {
				if (string.IsNullOrEmpty (_nodePath)) {
					_nodePath = Path.Combine (clientPath, "LocalFile/lua");
					_nodePath = _nodePath.Replace ("\\", "/");
				}
				return _nodePath;
			}
		}

		private static Dictionary<string, string> mNodeTypeDict = new Dictionary<string, string> ();

		public static string GenerateUniqueStringId ()
		{
			return Guid.NewGuid ().ToString ("N");
		}

		public static void SaveBTData (BehaviourTree tree)
		{
			if (tree != null) {
				WalkNodeData (tree.Root);
				string content = JsonConvert.SerializeObject (tree.Root.Data, Formatting.Indented);
				File.WriteAllText (Path.Combine (jsonPath, string.Format ("{0}.json", tree.Name)), content);

				content = content.Replace ("[", "{");
				content = content.Replace ("]", "}");
				content = content.Replace (":", "=");
				var mc = Regex.Matches (content, "\"[a-zA-Z0-9_]+\"=");
				foreach (Match m in mc) {
					string word = m.Value.Replace ("\"", "");
					content = content.Replace (m.Value, word);
				}

				mc = Regex.Matches (content, "\\s*[a-zA-Z0-9_]+= null,?");
				foreach (Match m in mc) {
					content = content.Replace (m.Value, "");
				}

				mc = Regex.Matches (content, "= \"\\d+\",?");
				foreach (Match m in mc) {
					string word = m.Value.Replace ("\"", "");
					content = content.Replace (m.Value, word);
				}

				content = string.Format ("local __bt__ = {0}\nreturn __bt__", content);
				File.WriteAllText (Path.Combine (behaviorPath, string.Format ("{0}.lua", tree.Name)), content);
			}
		}

		public static void WalkNodeData (BTNode parent)
		{
			parent.Data.name = parent.NodeName;
			parent.Data.posX = parent.BTNodeGraph.RealRect.position.x;
			parent.Data.posY = parent.BTNodeGraph.RealRect.position.y;

			if (parent.IsHaveChild) {
				foreach (var node in parent.ChildNodeList) {
					WalkNodeData (node);
				}

				parent.Data.children.Sort ((a, b) => {
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

		public static BehaviourTree LoadBehaviorTree (string file)
		{
			if (!File.Exists (file))
				return null;
			var content = File.ReadAllText (file);
			var data = JsonConvert.DeserializeObject<BTNodeData> (content);
			var tree = new BehaviourTree (Path.GetFileNameWithoutExtension (file), data);
			WalkJsonData (tree, tree.Root);
			return tree;
		}

		public static void WalkJsonData (BehaviourTree owner, BTNode parent)
		{
			var childrenData = parent.Data.children;
			if (childrenData != null && childrenData.Count > 0) {
				foreach (var data in childrenData) {
					var child = AddChild (owner, parent, data);
					WalkJsonData (owner, child);
				}
			}
		}

		public static BTNode AddChild (BehaviourTree owner, BTNode parent, string name)
		{
			var pos = parent.BTNodeGraph.RealRect.position;
			if (!mNodeTypeDict.ContainsKey (name))
				throw new ArgumentNullException (name, "找不到该类型");
			var data = new BTNodeData (name, mNodeTypeDict [name], pos.x, pos.y + BTConst.DefaultSpacingY);
			parent.Data.AddChild (data);
			return AddChild (owner, parent, data);
		}

		public static BTNode AddChild (BehaviourTree owner, BTNode parent, BTNodeData data)
		{
			var child = new BTNode (owner, parent, data);
			owner.AddNode (child);
			parent.ChildNodeList.Add (child);
			return child;
		}

		public static void RemoveChild (BTNode node)
		{
			if (node.IsHaveChild) {
				foreach (var child in node.ChildNodeList) {
					child.Owner.AddOrphanNode (child);
					child.Parent = null;
				}
			}

			if (node.IsHaveParent) {
				node.Parent.ChildNodeList.Remove (node);
				node.Parent.Data.children.Remove (node.Data);
			}

			node.Owner.RemoveOrphanNode (node);
			node.Owner.RemoveNode (node);
		}

		public static void AutoAlignPosition (BTNode node)
		{
			var width = BTConst.DefaultWidth + BTConst.DefaultSpacingX;
			var multiW = Mathf.RoundToInt (node.BTNodeGraph.RealRect.x / width);
			float x = multiW * width;

			var height = BTConst.DefaultHeight + BTConst.DefaultSpacingY;
			var multiH = Mathf.RoundToInt (node.BTNodeGraph.RealRect.y / height);
			float y = multiH * height;

			node.BTNodeGraph.RealRect.position = new Vector2 (x, y);
		}

		public static void LoadNodeFile ()
		{
			mNodeTypeDict.Clear ();
			var allFiles = FileHelper.GetAllFiles (nodePath, "lua");
			foreach (var fullPath in allFiles) {
				var sortPath = fullPath.Replace ("\\", "/");
				sortPath = sortPath.Replace (nodePath, "");
				if (sortPath.Contains ("/common/"))
					continue;
				var fileName = Path.GetFileNameWithoutExtension (fullPath);
				string type = sortPath.Substring (1, sortPath.LastIndexOf ('/') - 1);
				mNodeTypeDict.Add (fileName, type);
				//Debug.LogFormat ("加载lua节点:{0}", fileName);
			}
		}

		public static BTNodeType CreateNodeType (BTNode node)
		{
			string key = node.NodeName;
			if (key == BTConst.RootName)
				return new Root (node);
			if (mNodeTypeDict.ContainsKey (key)) {
				string type = mNodeTypeDict [key];
				switch (type) {
				case "actions":
					return new Task (node);
				case "composites":
					return new Composite (node);
				case "decorators":
					return new Decorator (node);
				}
			}
			throw new ArgumentNullException (node.NodeName, "找不到该节点");
		}

		public static GenericMenu GetGenericMenu (BTNode node, GenericMenu.MenuFunction2 callback)
		{
			GenericMenu menu = new GenericMenu ();
			if (!node.IsTask && node.ChildNodeList.Count < node.Type.CanAddNodeCount) {
				foreach (var kv in mNodeTypeDict) {
					var data = kv.Key.Replace ("Node", "");
					var menuPath = string.Format ("{0}/{1}", kv.Value, data);
					menu.AddItem (new GUIContent (menuPath), false, callback, kv.Key);
				}
			}

			if (!node.IsRoot) {
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Delete Node"), false, callback, "Delete");
			}

			return menu;
		}

		public static void SetNodeDefaultData (BTNode node, string name)
		{
			var data = node.Data;
			switch (name) {
			case "waitNode":
				data.AddData ("waitMin", "2");
				data.AddData ("waitMax", "5");
				break;
			case "weightNode":
				data.AddData ("weight", "10");
				break;
			case "checkStateNode":
				data.AddData ("stateId", "0");
				break;
			case "speakNode":
				data.AddData ("say", "hello world");
				break;
			}
		}

	}
}