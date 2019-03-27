using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDisplay : MonoBehaviour
{
	public enum DirectionFacing { UpLeft, UpRight, DownLeft, DownRight };
	
	public SpriteRenderer spriteRenderer;
	//Drag the MainCamera under CameraTarget into this field
	public Camera camera;
	public Sprite frontFacingSprite;
	public Sprite backFacingSprite;
	public DirectionFacing directionFacing;
	private float x, y;

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
    	switch(directionFacing) 
    	{
    		case DirectionFacing.UpLeft:
    			spriteRenderer.sprite = backFacingSprite;
    			x = 90; y = 0;
    			break;
    		case DirectionFacing.UpRight:
    			spriteRenderer.sprite = backFacingSprite;
    			x = 270; y = 180;
    			break;
    		case DirectionFacing.DownLeft:
    			spriteRenderer.sprite = frontFacingSprite;
    			x = 270; y = 180;
    			break;
    		case DirectionFacing.DownRight:
    			spriteRenderer.sprite = frontFacingSprite;
    			x = 90; y = 0;
    			break;
    		default:
    			spriteRenderer.sprite = frontFacingSprite;
    			x = 90; y = 0;
    			break;
    	}
    	
    	//This code makes the sprite always face the camera perpendicularly.
         transform.LookAt(camera.transform.position, Vector3.up);
         transform.Rotate(x, y, 0, Space.Self);
    }
}