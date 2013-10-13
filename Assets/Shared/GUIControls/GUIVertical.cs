using UnityEngine;

public class GUIVertical : System.IDisposable {
	public GUIVertical(params GUILayoutOption[] options) {
		GUILayout.BeginVertical(options);
	}
	
	public GUIVertical(GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(style, options);
	}
	
	public GUIVertical(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(content, style, options);
	}
	
	public GUIVertical(Texture image, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(image, style, options);
	}
	
	public GUIVertical(string text, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(text, style, options);
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.EndVertical();
	}
}
