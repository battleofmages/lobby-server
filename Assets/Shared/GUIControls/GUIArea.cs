using UnityEngine;

public class GUIArea : System.IDisposable {
	public static float width = Screen.width;
	public static float height = Screen.height;
	
	public GUIArea(Rect screenRect) {
		GUIArea.width = screenRect.width;
		GUIArea.height = screenRect.height;
		
		GUILayout.BeginArea(screenRect);
	}
	
	public GUIArea(float width, float height) {
		GUIArea.width = width;
		GUIArea.height = height;
		
		GUILayout.BeginArea(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height));
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.EndArea();
		
		GUIArea.width = Screen.width;
		GUIArea.height = Screen.height;
	}
}
