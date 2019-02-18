using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITileManager : MonoBehaviour {
    public int x, z;
    public GameObject tilePrefab; //expand multiple types of terrain? maybe an array
    public GameObject[] piecePrefab; //WIP array of pieces?
    public const int tileSize = 10;


    private GameObject[,] tileArray;
    private GameObject[,] pieceArray;

    private int srcx, srcz;
    private int destx, destz;
    // Start is called before the first frame update
    void Start() {
        srcx = -1;
        srcz = -1;
        destx = -1;
        destz = -1;

        float sx = -tileSize * (x-1) / 2f;
        float sz = -tileSize * (z-1) / 2f;
        tileArray = new GameObject[x, z];
        pieceArray = new GameObject[x, z];
        for (int i = 0; i < x; i++) {
            for (int j = 0; j < z; j++) {
                GameObject cur = Instantiate(tilePrefab, new Vector3(sx + i * tileSize, 0, sz + j * tileSize), Quaternion.identity);
                cur.GetComponentInChildren<UITile>().coordx = i;
                cur.GetComponentInChildren<UITile>().coordz = j;
                tileArray[i, j] = cur;
            }
        }
        // TEST
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 1, 1);
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 1, 2);
    }

    // Update is called once per frame
    void Update() {

    }

    //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<CreateTiles>().InitializePiece(0, coordx, coordz);
    public void InitializePiece(int typeOfPiece, int x, int z) {
        if (typeOfPiece > piecePrefab.Length) {
            return;
            //die
        }
        pieceArray[x, z] = Instantiate(piecePrefab[typeOfPiece], tileArray[x, z].GetComponent<Transform>().transform.position, Quaternion.identity);
    }

    public void MoveFrom(int x, int z) {
        Debug.Log("(" + x + ", " + z + ")");
        srcx = x;
        srcz = z;
    }

    public void MoveUpdateDestination(int x, int z) {
        destx = x;
        destz = z;
    }

    public void MoveTo() {
        Debug.Log("(" + destx + ", " + destz + ")");
        //Debug.Log(pieceArray[srcx, srcz]);
        if (pieceArray[destx, destz] == null) {
            pieceArray[srcx, srcz].GetComponent<UIPiece>().destination = tileArray[destx, destz];
            pieceArray[destx, destz] = pieceArray[srcx, srcz];
            if (destx != srcx || destz != srcz) {
                pieceArray[srcx, srcz] = null;
            }
        } else {

        }
        srcx = -1;
        srcz = -1;
    }

    
}
