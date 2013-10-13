using UnityEngine;

public class GUIScrollView : System.IDisposable {
	public GUIScrollView(ref Vector2 scrollPosition, params GUILayoutOption[] options) {
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, options);
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.EndScrollView();
	}
}
