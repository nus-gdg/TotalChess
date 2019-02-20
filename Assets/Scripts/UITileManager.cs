using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UITileManager : MonoBehaviour {
    public int x, z;
    public GameObject tilePrefab; //expand multiple types of terrain? maybe an array
    public GameObject[] piecePrefab; //WIP array of pieces?
    public const int tileSize = 10;


	private GameObject[,] tileArray;
	private GameObject[,] pieceArray;
	private Queue<GameObject>[,] moveArray;
	private Queue<GameObject> moveDir;


	private bool selecting;
    private int srcx, srcz, destx, destz;
    // Start is called before the first frame update
    void Start() {
        srcx = -1;
        srcz = -1;
        destx = -1;
        destz = -1;
		selecting = false;

        float sx = -tileSize * (x-1) / 2f;
        float sz = -tileSize * (z-1) / 2f;
		tileArray = new GameObject[x, z];
		pieceArray = new GameObject[x, z];
		moveArray = new Queue<GameObject>[x, z];
        for (int i = 0; i < x; i++) {
            for (int j = 0; j < z; j++) {
                GameObject cur = Instantiate(tilePrefab, new Vector3(sx + i * tileSize, 0, sz + j * tileSize), Quaternion.identity);
                cur.GetComponentInChildren<UITile>().coordx = i;
                cur.GetComponentInChildren<UITile>().coordz = j;
                tileArray[i, j] = cur;
            }
        }

		
		moveDir = new Queue<GameObject>();
        // TEST
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 1, 1);
        GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 1, 2);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            Debug.Log("E");
            BounceTowards(1,1,1,2);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("R");
            pieceArray[1, 1].GetComponent<UIPiece>().alive = false;
        }

        if (Input.GetKeyDown(KeyCode.Return)){
			// Todo: code to send to processing
			for(int i = 0; i < x; i++){
				for(int j = 0; j < z; j++){
					if(moveArray[i, j] != null){
						int lastx = i, lastz = j;
                        foreach (GameObject c in moveArray[i, j]) {
                            Debug.Log(111);
                            c.GetComponentInChildren<UITile>().killGreen(i, j);
                            MoveFromTo(lastx, lastz, c.GetComponentInChildren<UITile>().coordx, c.GetComponentInChildren<UITile>().coordz) ;
                            lastx = c.GetComponentInChildren<UITile>().coordx;
                            lastz = c.GetComponentInChildren<UITile>().coordz;
                        }
					}
				}
			}
			moveArray = new Queue<GameObject>[x, z];
		}
    }

    // use this 
    void BounceTowards(int sx, int sz, int dx, int dz) {
        int deltax = sx, deltaz = sz;
        bool laze = (deltax == 1 && deltaz == 0) || (deltax == -1 && deltaz == 0) || (deltax == 0 && deltaz == 1) || (deltax == 0 && deltaz == -1);
        if (pieceArray[sx, sz] != pieceArray[dx, dz]) {
            pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[dx, dz]);
            pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[sx, sz]);
            pieceArray[sx, sz].GetComponent<UIPiece>().bouncing = true;
            pieceArray[sx, sz].GetComponent<UIPiece>().bouncingInf = (tileArray[sx, sz].GetComponent<Transform>().position + tileArray[dx, dz].GetComponent<Transform>().position) / 2;
        }
    }

    // use this 
    void MoveFromTo(int sx, int sz, int dx, int dz) {
        if (pieceArray[sx, sz] != pieceArray[dx, dz]) {
            pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[dx, dz]);
            pieceArray[sx, sz].GetComponent<UIPiece>().moving = true;
            pieceArray[dx, dz] = pieceArray[sx, sz];
            pieceArray[sx, sz] = null;
        }
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
        srcx = x;
		srcz = z;
		destx = x;
		destz = z;
		if(pieceArray[srcx, srcz] != null){
			selecting = true;
		}
		moveDir = new Queue<GameObject>();
		if(moveArray[srcx, srcz] == null){
			//moveArray[srcx, srcz] = new Queue<GameObject>();
			return;
		}
		foreach(GameObject c in moveArray[srcx, srcz]){
			//Debug.Log("1");
			c.GetComponentInChildren<UITile>().killGreen(x, z);
		}
    }

    public bool MoveUpdateDestination(int x, int z) {
		//pieceArray[srcx, srcz].GetComponent<UIPiece>().NewDestination(tileArray[destx, destz]);
		if(selecting){
			int deltax = destx - x, deltaz = destz - z;
			bool laze = (deltax == 1 && deltaz == 0) || (deltax == -1 && deltaz == 0) || (deltax == 0 && deltaz == 1) || (deltax == 0 && deltaz == -1) ;
			if(moveDir.Count < 3 && laze){
				destx = x;
				destz = z;
				moveDir.Enqueue(tileArray[x, z]);
				tileArray[x, z].GetComponentInChildren<UITile>().makeGreen(srcx, srcz);
				return true;
			}else{
				return false;
			}
		}
		return false;
    }

    public void MoveTo() {
        //Debug.Log("(" + destx + ", " + destz + ")");
        //Debug.Log(pieceArray[srcx, srcz]);
		try{
			if(pieceArray[srcx, srcz] != pieceArray[destx, destz]){
				/*
				pieceArray[srcx, srcz].GetComponent<UIPiece>().destination = moveDir;
				pieceArray[srcx, srcz].GetComponent<UIPiece>().moving = true;

				pieceArray[destx, destz] = pieceArray[srcx, srcz];
				pieceArray[srcx, srcz] = null;
				*/
			}
		}catch(NullReferenceException e){
			//no piece at srcx, srcz	
		}finally{
			//c.GetComponentInChildren<UITile>().killGreen();
			moveArray[srcx, srcz] = moveDir;
			moveDir = new Queue<GameObject>();
			//moveDir = new Queue<GameObject>();
			selecting = false;
		}
    }


    
}
