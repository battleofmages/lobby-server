using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {
	private static bool created = false;
	
	public InputControl[] controls;
	
	void Awake() {
		// Don't destroy this object on level loading
		if(!created) {
			DontDestroyOnLoad(this.gameObject);
			created = true;
		} else {
			Destroy(this.gameObject);
		}
	}
	
	public bool GetButton(int index) {
		return Input.GetKey(controls[index].keyCode) || Input.GetKey(controls[index].altKeyCode);
	}
	
	public bool GetButtonDown(int index) {
		return Input.GetKeyDown(controls[index].keyCode) || Input.GetKeyDown(controls[index].altKeyCode);
	}
	
	public float GetButtonFloat(int index) {
		return Input.GetKey(controls[index].keyCode) || Input.GetKey(controls[index].altKeyCode) ? 1.0f : 0.0f;
	}
	
	public int GetButtonIndex(string name) {
		for(int i = 0; i < controls.Length; i++) {
			if(controls[i].name == name)
				return i;
		}
		
		Debug.LogWarning("Control '" + name + "' doesn't exist");
		return -1;
	}
	
	public void CopySettingsFrom(InputSettings inputSettings) {
		foreach(var control in inputSettings.controls) {
			int index = GetButtonIndex(control.name);
			
			if(index != -1) {
				var myControl = controls[index];
				myControl.keyCode = control.keyCode;
				myControl.altKeyCode = control.altKeyCode;
			}
		}
	}
	
	public static Vector2 GetMousePosition() {
		return new Vector2(Input.mousePosition.x, (Screen.height - Input.mousePosition.y));
	}
}
