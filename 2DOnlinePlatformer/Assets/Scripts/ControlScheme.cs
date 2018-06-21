using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ControlScheme {

	public KeyCode jump;
	public KeyCode left;
	public KeyCode right;
	public KeyCode up;
	public KeyCode down;
	public KeyCode zoom;
	public KeyCode suicide;
	public KeyCode characterSwap;

	public enum ControlSchemePreset { Null, WASD, Arrows, NumPad, Controller }

	public ControlScheme (ControlSchemePreset preset) {
		switch (preset) {
			case (ControlSchemePreset.WASD):
				jump = KeyCode.Space;
				left = KeyCode.A;
				right = KeyCode.D;
				up = KeyCode.W;
				down = KeyCode.S;
				suicide = KeyCode.K;
				zoom = KeyCode.Z;
				characterSwap = KeyCode.Q;
				break;
			case (ControlSchemePreset.Arrows):
				jump = KeyCode.Return;
				left = KeyCode.LeftArrow;
				right = KeyCode.RightArrow;
				up = KeyCode.UpArrow;
				down = KeyCode.DownArrow;
				suicide = KeyCode.RightControl;
				characterSwap = KeyCode.RightShift;
				break;
			case (ControlSchemePreset.NumPad):
				jump = KeyCode.KeypadEnter;
				left = KeyCode.Keypad4;
				right = KeyCode.Keypad6;
				up = KeyCode.Keypad8;
				down = KeyCode.Keypad5;
				suicide = KeyCode.KeypadPeriod;
				characterSwap = KeyCode.Plus;
				break;
		}


	}

	
}
