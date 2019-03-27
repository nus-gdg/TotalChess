using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	//Drag the MainCamera under CameraTarget into this field
	public Camera camera;

	 //TODO:
	 //Each unit's position on the map should be relative to the tile it is upon.
	 //So we need each unit to have a reference to a tile, or at least its position
	 //Someone needs to tell it where to stand on the map
	
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    	//This code makes the sprite always face the camera perpendicularly.
         transform.LookAt(camera.transform.position, Vector3.up);
         transform.Rotate(90, 0, 0, Space.Self);
    }
}