using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialiseCameraTargetVariables : MonoBehaviour
{
	//Vector3 _position = new Vector3(0f, 15f, 0f);
	//Vector3 _rotation = new Vector3(30f, 45f, 0f);
	//Vector3 _scale = new Vector3(0.1f, 1f, 0.1f);
    // Start is called before the first frame update
    void Start()
    {
    	//This sets the Camera Target to a specific position at the start of the Scene,
    	//Which helps to position the Camera for Isometric projection
    	transform.position = new Vector3(0f, 15f, 0f);
    	transform.rotation = Quaternion.Euler(30, 45, 0);
    	transform.localScale = new Vector3(0.1f, 1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
