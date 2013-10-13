using UnityEngine;

public class GUIHorizontalCenter : System.IDisposable {
	public GUIHorizontalCenter(params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIHorizontalCenter(GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIHorizontalCenter(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(content, style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIHorizontalCenter(Texture image, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(image, style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIHorizontalCenter(string text, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(text, style, options);
		GUILayout.FlexibleSpace();
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
