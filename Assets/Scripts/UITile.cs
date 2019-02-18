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

    private void OnMouseDown() {
        // Todo: Test
        Debug.Log("(" + coordx + ", " + coordz + ")");
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveFrom(coordx, coordz);
        Instantiate(sourceDisplay, transform);
    }

    private void OnMouseEnter() {
        // Todo: Test
        Instantiate(selectedDisplay, transform);
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveUpdateDestination(coordx, coordz);
    }

    private void OnMouseExit() {
        // Todo: Test
        foreach (Transform child in transform) {
            if (child.gameObject.tag == "GreenTag") {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    private void OnMouseUp() {
        // Todo: Test
        Debug.Log("(" + coordx + ", " + coordz + ")");
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().MoveTo();
    }
}
