using UnityEngine;

public class GUIArea : System.IDisposable {
	public static int width = Screen.width;
	public static int height = Screen.height;
	
	public GUIArea(Rect screenRect) {
		GUIArea.width = (int)screenRect.width;
		GUIArea.height = (int)screenRect.height;
		
		GUILayout.BeginArea(screenRect);
	}
	
	public GUIArea(float width, float height) {
		GUIArea.width = (int)width;
		GUIArea.height = (int)height;
		
		GUILayout.BeginArea(new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height));
	}
	
	public GUIArea(float x, float y, float width, float height) {
		if(width <= 1f && height <= 1f) {
			GUIArea.width = (int)(Screen.width * width);
			GUIArea.height = (int)(Screen.height * height);
			
			GUILayout.BeginArea(new Rect(Screen.width * x, Screen.height * y, GUIArea.width, GUIArea.height));
		} else {
			GUIArea.width = (int)(width);
			GUIArea.height = (int)(height);
			
			GUILayout.BeginArea(new Rect(x, y, GUIArea.width, GUIArea.height));
		}
	}
	
	void System.IDisposable.Dispose() {
		GUILayout.EndArea();
		
		GUIArea.width = Screen.width;
		GUIArea.height = Screen.height;
	}
}
