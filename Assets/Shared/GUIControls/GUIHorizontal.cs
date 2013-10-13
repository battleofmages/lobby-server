using UnityEngine;

public class GUIHorizontal : System.IDisposable {
	public GUIHorizontal(params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(options);
	}
	
	public GUIHorizontal(GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(style, options);
	}
	
	public GUIHorizontal(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(content, style, options);
	}
	
	public GUIHorizontal(Texture image, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(image, style, options);
	}
	
	public GUIHorizontal(string text, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginHorizontal(text, style, options);
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.EndHorizontal();
	}
}
