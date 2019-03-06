using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 Generic button behavior.
 You can set a callback to figure out what to do when the button is pressed.
*/
public class ButtonBehavior : MonoBehaviour {

	public List<buttonPressed> callbacks;

	void Awake() {
		callbacks = new List<buttonPressed>();
	}

	private void OnMouseOver() {
		//change cursor to pointer?
	}

	private void OnMouseExit() {
		//change cursor to normal?
	}

	private void OnMouseUp() {
		for(int i = 0; i < callbacks.Count; i++) {
			callbacks[i](gameObject);
		}
	}

	public delegate void buttonPressed(GameObject who);

	public void addCallback(buttonPressed code) {
		callbacks.Add(code);
	}

	public void removeCallback(buttonPressed code) {
		callbacks.Remove(code);
	}
}