using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {
	private static bool created = false;
	
	private float _mouseSensitivity = 5f;
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
	
	public float mouseSensitivity {
		get { return _mouseSensitivity; }
		set {
			if(_mouseSensitivity != value) {
				_mouseSensitivity = value;
				PlayerPrefs.SetFloat("Input_MouseSensitivity", _mouseSensitivity);
				var camPivot = GameObject.FindGameObjectWithTag("CamPivot");
				
				if(camPivot != null) {
					var mouseLook = camPivot.GetComponent<MouseLook>();
					mouseLook.sensitivityX = _mouseSensitivity;
					mouseLook.sensitivityY = _mouseSensitivity;
				}
			}
		}
	}
	
	public bool GetButton(int index) {
		if(GUIUtility.hotControl != 0 || GUIUtility.keyboardControl != 0)
			return false;
		
		return Input.GetKey(controls[index].keyCode) || Input.GetKey(controls[index].altKeyCode) || Input.GetKey(controls[index].gamePadKeyCode);
	}
	
	public bool GetButtonDown(int index) {
		if(GUIUtility.hotControl != 0 || GUIUtility.keyboardControl != 0)
			return false;
		
		return Input.GetKeyDown(controls[index].keyCode) || Input.GetKeyDown(controls[index].altKeyCode) || Input.GetKeyDown(controls[index].gamePadKeyCode);
	}
	
	public float GetButtonFloat(int index) {
		if(GUIUtility.hotControl != 0 || GUIUtility.keyboardControl != 0)
			return 0f;
		
		return Input.GetKey(controls[index].keyCode) || Input.GetKey(controls[index].altKeyCode) || Input.GetKey(controls[index].gamePadKeyCode) ? 1.0f : 0.0f;
	}
	
	public int GetButtonIndex(string id) {
		for(int i = 0; i < controls.Length; i++) {
			if(controls[i].id == id)
				return i;
		}
		
		LogManager.General.LogWarning("Control '" + id + "' doesn't exist");
		return -1;
	}
	
	public void CopySettingsFrom(InputSettings inputSettings) {
		foreach(var control in inputSettings.controls) {
			int index = GetButtonIndex(control.id);
			
			if(index != -1) {
				var myControl = controls[index];
				myControl.keyCode = control.keyCode;
				myControl.altKeyCode = control.altKeyCode;
				//myControl.gamePadKeyCode = control.gamePadKeyCode;
			}
		}
	}
	
	public static Vector2 GetMousePosition() {
		return new Vector2(Input.mousePosition.x, (Screen.height - Input.mousePosition.y));
	}
	
	public static Vector2 GetRelativeMousePosition() {
		return new Vector2(Input.mousePosition.x - GUIArea.x, (Screen.height - Input.mousePosition.y) - GUIArea.y);
	}
}
