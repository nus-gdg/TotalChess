using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPiece : MonoBehaviour {
    public GameObject destination {
        set; get;
    }
    public float speed = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        destination = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.MoveTowards(transform.position, destination.GetComponent<Transform>().position, speed);
    }
}
