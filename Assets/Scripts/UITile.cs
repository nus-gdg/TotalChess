using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITile : MonoBehaviour {
    public int coordx {
        get; set;
    }
    public int coordz {
        get; set;
    }
    public GameObject sourceDisplay;
    public GameObject selectedDisplay;

    // Start is called before the first frame update
    void Start() {
    
    }

    // Update is called once per frame
    void Update() {

    }

	public void makeGreen(int x, int z){
		GameObject a = Instantiate(selectedDisplay, transform);
		a.GetComponent<UIGreenSelector>().source = "" + x + "," + z;
	}

	public void killGreen(int x, int z){
		foreach (Transform child in transform) {
			Debug.Log(child.gameObject.GetComponent<UIGreenSelector>().source);
			if (child.gameObject.GetComponent<UIGreenSelector>().source.Equals("" + x + "," +  z)) {
				GameObject.Destroy(child.gameObject);
			}
		}
	}

    private void OnMouseDown() {
        // Todo: Test
        //Debug.Log("(" + coordx + ", " + coordz + ")");
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveFrom(coordx, coordz);
        Instantiate(sourceDisplay, transform);
    }

    private void OnMouseEnter() {
        // Todo: Test
		if(GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveUpdateDestination(coordx, coordz)){

			//Debug.Log("" + coordx + "," + coordz);
		}
    }

    private void OnMouseExit() {
        // Todo: Test
    }

    private void OnMouseUp() {
        // Todo: Test
        //Debug.Log("(" + coordx + ", " + coordz + ")");
        foreach (Transform child in transform) {
			if(child.GetComponent<UIGreenSelector>() == null){
	            GameObject.Destroy(child.gameObject);

			}
		}
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveTo();
    }
}
