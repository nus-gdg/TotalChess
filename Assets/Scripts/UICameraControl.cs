using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(-Input.mouseScrollDelta.y + this.GetComponent<Camera>().fieldOfView, 30, 107);
		if(Input.GetKey(KeyCode.W)){
			GetComponent<Transform>().position = GetComponent<Transform>().position + new Vector3(0, 0, 1);
		}
		if(Input.GetKey(KeyCode.S)){
			GetComponent<Transform>().position = GetComponent<Transform>().position + new Vector3(0, 0, -1);
		}
		if(Input.GetKey(KeyCode.A)){
			GetComponent<Transform>().position = GetComponent<Transform>().position + new Vector3(-1, 0, 0);
		}
		if(Input.GetKey(KeyCode.D)){
			GetComponent<Transform>().position = GetComponent<Transform>().position + new Vector3(1, 0, 0);
		}
	}
}
