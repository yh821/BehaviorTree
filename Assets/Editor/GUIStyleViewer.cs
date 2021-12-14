using UnityEditor;
using UnityEngine;

public sealed class GUIStyleViewer : EditorWindow
{
	private Vector2 mScrollVector2 = Vector2.zero;
	private string mSearch = "";

	[MenuItem("Tools/GUIStyle查看器")]
	public static void InitWindow()
	{
		EditorWindow.GetWindow(typeof(GUIStyleViewer));
	}

	void OnGUI()
	{
		GUILayout.BeginHorizontal("HelpBox");
		GUILayout.Space(30);
		mSearch = EditorGUILayout.TextField("", mSearch, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
		GUILayout.Label("", "SearchCancelButtonEmpty");
		GUILayout.EndHorizontal();
		mScrollVector2 = GUILayout.BeginScrollView(mScrollVector2);
		foreach (var style in GUI.skin.customStyles)
		{
			if (style.name.ToLower().Contains(mSearch.ToLower()))
			{
				DrawStyleItem(style);
			}
		}
		GUILayout.EndScrollView();
	}

	private void DrawStyleItem(GUIStyle style)
	{
		GUILayout.BeginHorizontal("box");
		GUILayout.Space(40);
		EditorGUILayout.SelectableLabel(style.name);
		GUILayout.FlexibleSpace();
		EditorGUILayout.SelectableLabel(style.name, style);
		GUILayout.Space(40);
		EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
		GUILayout.Space(50);
		if (GUILayout.Button("复制到剪贴板"))
		{
			var textEditor = new TextEditor {text = style.name};
			textEditor.OnFocus();
			textEditor.Copy();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
	}
}
