using UnityEngine;

public class GUIVerticalCenter : System.IDisposable {
	public GUIVerticalCenter(params GUILayoutOption[] options) {
		GUILayout.BeginVertical(options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIVerticalCenter(GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIVerticalCenter(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(content, style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIVerticalCenter(Texture image, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(image, style, options);
		GUILayout.FlexibleSpace();
	}
	
	public GUIVerticalCenter(string text, GUIStyle style, params GUILayoutOption[] options) {
		GUILayout.BeginVertical(text, style, options);
		GUILayout.FlexibleSpace();
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}
}
