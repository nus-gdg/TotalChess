using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UITileManager : MonoBehaviour
{
    public int x, z;
    public GameObject tilePrefab; //expand multiple types of terrain? maybe an array
    public GameObject[] piecePrefab; //WIP array of pieces?
    public const int tileSize = 10;


    private Guid uuid;
    private GameObject[,] tileArray;
    private GameObject[,] pieceArray;
    private GameObject[,] destPieceArray;
    private Dictionary<String, Tuple<int, int, GameObject>> pieceCodex;
    private Queue<GameObject>[,] moveArray;
    private Queue<GameObject> moveDir;
    private bool selecting;
    private int srcx, srcz, destx, destz;
    // Start is called before the first frame update
    void Awake()
    {
        srcx = -1;
        srcz = -1;
        destx = -1;
        destz = -1;
        selecting = false;

        tileArray = new GameObject[x, z];
        pieceArray = new GameObject[x, z];
        destPieceArray = new GameObject[x, z];
        pieceCodex = new Dictionary<string, Tuple<int, int, GameObject>>();
        moveArray = new Queue<GameObject>[x, z];
        uuid = Guid.NewGuid();

        CreateTiles();


        moveDir = new Queue<GameObject>();
        // TEST
        //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 1, 1);
        //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(1, 2, 1);
        //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(0, 3, 1);
        //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(2, 7, 7);
        //GameObject.FindGameObjectsWithTag("TileManager")[0].GetComponent<UITileManager>().InitializePiece(1, 6, 7);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            BounceTowards(1, 1, 1, 2);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            pieceArray[1, 1].GetComponent<UIPiece>().alive = false;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            pieceArray[1, 1].GetComponent<UIPiece>().TakeDamage(15, UIPiece.DAMAGE_GANKED4);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            FinalizeInput();
        }
    }

    //methods for moving pieces*********************************
    // use this
    void Kill(int sx, int sz)
    {
        pieceArray[sx, sz].GetComponent<UIPiece>().alive = false;
        pieceArray[sx, sz] = null;
        pieceCodex[pieceArray[sx, sz].GetComponent<UIPiece>().puid] = null;
    }
    void Kill(string puid)
    {
        int sx = pieceCodex[puid].Item1;
        int sz = pieceCodex[puid].Item2;
        Kill(sx, sz);
    }
    // use this
    void BounceTowards(int sx, int sz, int dx, int dz)
    {
        int deltax = sx, deltaz = sz;
        bool laze = (deltax == 1 && deltaz == 0) || (deltax == -1 && deltaz == 0) || (deltax == 0 && deltaz == 1) || (deltax == 0 && deltaz == -1);
        if (pieceArray[sx, sz] != pieceArray[dx, dz])
        {
            pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[dx, dz]);
            pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[sx, sz]);
            pieceArray[sx, sz].GetComponent<UIPiece>().bouncing = true;
            pieceArray[sx, sz].GetComponent<UIPiece>().bouncingInf = (tileArray[sx, sz].GetComponent<Transform>().position + tileArray[dx, dz].GetComponent<Transform>().position) / 2;
        }
    }
    void BounceTowards(string puid, int dx, int dz)
    {
        int sx = pieceCodex[puid].Item1;
        int sz = pieceCodex[puid].Item2;
        BounceTowards(sx, sz, dx, dz);
    }
    // use this
    void MoveFromTo(int sx, int sz, int dx, int dz)
    {
        pieceArray[sx, sz].GetComponent<UIPiece>().destination.Enqueue(tileArray[dx, dz]);
        pieceArray[sx, sz].GetComponent<UIPiece>().moving = true;
        pieceCodex[pieceArray[sx, sz].GetComponent<UIPiece>().puid] = new Tuple<int, int, GameObject>(dx, dz, pieceArray[sx, sz]);
        destPieceArray[dx, dz] = pieceArray[sx, sz];
    }
    public void MoveFromTo(string puid, int dx, int dz)
    {
        MoveFromTo(pieceCodex[puid].Item1, pieceCodex[puid].Item2, dx, dz);
    }

    public void PhasePostProcess()
    {
        GameObject[,] newPieceArray = new GameObject[x, z];
        foreach (KeyValuePair<string, Tuple<int, int, GameObject>> entry in pieceCodex)
        {
            newPieceArray[entry.Value.Item1, entry.Value.Item2] = entry.Value.Item3;
        }
        pieceArray = newPieceArray;
        destPieceArray = new GameObject[x, z];
    }
    //end methods for moving pieces******************************

    //show damage************************************************
    void Damage()
    {

    }
    //end show damage********************************************

    //UI initialize board and pieces*****************************
    void CreateTiles()
    {
        float sx = -tileSize * (x - 1) / 2f;
        float sz = -tileSize * (z - 1) / 2f;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                GameObject cur = Instantiate(tilePrefab, new Vector3(sx + i * tileSize, 0, sz + j * tileSize), Quaternion.identity);
                cur.GetComponentInChildren<UITile>().coordx = i;
                cur.GetComponentInChildren<UITile>().coordz = j;
                tileArray[i, j] = cur;
            }
        }
    }
    public String InitializePiece(int typeOfPiece, int x, int z)
    {
        if (typeOfPiece > piecePrefab.Length)
        {
            return null;
            //die
        }
        pieceArray[x, z] = Instantiate(piecePrefab[typeOfPiece], tileArray[x, z].GetComponent<Transform>().transform.position, Quaternion.identity);
        String newPuid = uuid + ":(" + typeOfPiece + ")" + x + ":" + z;
        pieceArray[x, z].GetComponent<UIPiece>().puid = newPuid;

        pieceCodex[newPuid] = new Tuple<int, int, GameObject>(x, z, pieceArray[x, z]);
        destPieceArray = pieceArray;
        return newPuid;
    }

    // for debug
    public void InitializePiece(int typeOfPiece, int x, int z, string newPuid)
    {
        if (typeOfPiece > piecePrefab.Length)
        {
            return;
            //die
        }
        pieceArray[x, z] = Instantiate(piecePrefab[typeOfPiece], tileArray[x, z].GetComponent<Transform>().transform.position, Quaternion.identity);
        pieceArray[x, z].GetComponent<UIPiece>().puid = newPuid;

        pieceCodex[newPuid] = new Tuple<int, int, GameObject>(x, z, pieceArray[x, z]);
        destPieceArray = pieceArray;
    }
    //end UI initialize board and pieces**************************


    //UI receive player input*************************************
    public void TryShowHP(int x, int z)
    {
        if (pieceArray[x, z])
        {
            pieceArray[x, z].GetComponent<UIPiece>().ShowHPBar();
        }
    }
    public void TryHideHP(int x, int z)
    {
        if (pieceArray[x, z])
        {
            pieceArray[x, z].GetComponent<UIPiece>().HideHPBar();
        }
    }
    public void MoveFrom(int x, int z)
    {
        srcx = x;
        srcz = z;
        destx = x;
        destz = z;
        if (pieceArray[srcx, srcz] != null)
        {
            selecting = true;
        }
        moveDir = new Queue<GameObject>();
        if (moveArray[srcx, srcz] == null)
        {
            //moveArray[srcx, srcz] = new Queue<GameObject>();
            return;
        }
        foreach (GameObject c in moveArray[srcx, srcz])
        {
            //Debug.Log("1");
            c.GetComponentInChildren<UITile>().KillGreen(x, z);
        }
    }
    public bool MoveUpdateDestination(int x, int z)
    {
        //pieceArray[srcx, srcz].GetComponent<UIPiece>().NewDestination(tileArray[destx, destz]);
        if (selecting)
        {
            int deltax = destx - x, deltaz = destz - z;
            bool laze = (deltax == 1 && deltaz == 0) || (deltax == -1 && deltaz == 0) || (deltax == 0 && deltaz == 1) || (deltax == 0 && deltaz == -1);
            if (moveDir.Count < 3 && laze)
            {
                destx = x;
                destz = z;
                moveDir.Enqueue(tileArray[x, z]);
                tileArray[x, z].GetComponentInChildren<UITile>().MakeGreen(srcx, srcz);
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public void MoveTo()
    {
        //Debug.Log("(" + destx + ", " + destz + ")");
        //Debug.Log(pieceArray[srcx, srcz]);
        try
        {
            if (pieceArray[srcx, srcz] != pieceArray[destx, destz])
            {
                /*
				pieceArray[srcx, srcz].GetComponent<UIPiece>().destination = moveDir;
				pieceArray[srcx, srcz].GetComponent<UIPiece>().moving = true;

				pieceArray[destx, destz] = pieceArray[srcx, srcz];
				pieceArray[srcx, srcz] = null;
				*/
            }
        }
        catch (NullReferenceException e)
        {
            //no piece at srcx, srcz
        }
        finally
        {
            //c.GetComponentInChildren<UITile>().killGreen();
            moveArray[srcx, srcz] = moveDir;
            moveDir = new Queue<GameObject>();
            //moveDir = new Queue<GameObject>();
            selecting = false;
        }
    }
    void FinalizeInput()
    {
        SendInput();
    }
    void SendInput()
    {
        List<List<Tuple<String, int, int>>> newDataToBeSent = new List<List<Tuple<String, int, int>>>();
        for (int phase = 1; phase <= 3; phase++)
        {
            List<Tuple<String, int, int>> phaseT = new List<Tuple<String, int, int>>();
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < z; j++)
                {
                    if (moveArray[i, j] != null && moveArray[i, j].Count > 0)
                    {
                        String curPuid = pieceArray[i, j].GetComponent<UIPiece>().puid;
                        UITile t = moveArray[i, j].Dequeue().GetComponentInChildren<UITile>();
                        t.KillGreen(i, j);
                        phaseT.Add(new Tuple<string, int, int>(curPuid, t.coordx, t.coordz));
                        pieceArray[i, j].GetComponent<UIPiece>().HideHPBar();
                    }
                }
            }
            newDataToBeSent.Add(phaseT);
        }

        moveArray = new Queue<GameObject>[x, z];
        //test
        NewTestProcessInput(newDataToBeSent);
    }

    public void RunPhase()
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                if (moveArray[i, j] != null && moveArray[i, j].Count > 0)
                {
                    String curPuid = pieceArray[i, j].GetComponent<UIPiece>().puid;
                    UITile t = moveArray[i, j].Dequeue().GetComponentInChildren<UITile>();
                    t.KillGreen(i, j);
                    pieceArray[i, j].GetComponent<UIPiece>().HideHPBar();
                }
            }
        }
    }
    //end UI receive player input**********************************

    //UI process incoming data*************************************
    // use this now
    void NewTestProcessInput(List<List<Tuple<String, int, int>>> input)
    {
        foreach (List<Tuple<String, int, int>> phase in input)
        {
            foreach (Tuple<String, int, int> move in phase)
            {
                MoveFromTo(move.Item1, move.Item2, move.Item3);
            }
            PhasePostProcess();
        }
    }
    void ProcessInput(List<Tuple<string, Queue<Tuple<string, int, int>>>> input)
    {
        foreach (Tuple<string, Queue<Tuple<string, int, int>>> pieceMove in input)
        {
            foreach (Tuple<string, int, int> inner in pieceMove.Item2)
            {
                switch (inner.Item1)
                {
                    case "move":
                        MoveFromTo(pieceMove.Item1, inner.Item2, inner.Item3);
                        break;
                    case "bounce":
                        BounceTowards(pieceMove.Item1, inner.Item2, inner.Item3);
                        break;
                    case "die":
                        Kill(pieceMove.Item1);
                        break;
                    case "default":
                        break;
                }
            }
        }
    }
    //end UI process incoming data*********************************


}
