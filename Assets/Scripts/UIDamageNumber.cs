using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDamageNumber : MonoBehaviour {
    public float deltaA = -0.02f;
    public float timeToDie = 1f;
    // Start is called before the first frame update
    void Start() {
        this.transform.LookAt(Camera.main.transform);
        Destroy(this.gameObject, timeToDie);
    }

    // Update is called once per frame
    void Update() {
        this.transform.LookAt(Camera.main.transform);
    }
}
