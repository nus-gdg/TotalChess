using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// primitive camera controls
public class UICameraControl : MonoBehaviour {
    public float distanceFromCenter;
    public float heightFromCenter;
    public float scale;

    private float angle;
    // Start is called before the first frame update
    void Start() {
        angle = Mathf.PI;
        UpdateCamera();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.A)) {
            ClampAngle(.05f);
            UpdateCamera();
        }
        if (Input.GetKey(KeyCode.D)) {
            ClampAngle(-.05f);
            UpdateCamera();
        }
        if (Input.GetKey(KeyCode.W)) {
            ClampCameraHeight(1f);
            UpdateCamera();
        }
        if (Input.GetKey(KeyCode.S)) {
            ClampCameraHeight(-1f);
            UpdateCamera();
        }
        if (Input.mouseScrollDelta.y != 0) {
            ClampCameraDistance(Input.mouseScrollDelta.y * -.05f);
            UpdateCamera();
        }

        //this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(-Input.mouseScrollDelta.y + this.GetComponent<Camera>().fieldOfView, 30, 107);
    }

    void ClampAngle(float inc) {
        angle = (angle + inc) % (Mathf.PI * 2);
    }

    void ClampCameraDistance(float inc) {
        scale = Mathf.Clamp(scale + inc, .6f, 1.25f);
    }

    void ClampCameraHeight(float inc) {
        heightFromCenter = Mathf.Clamp(heightFromCenter + inc, 10f, 90f);
    }

    void UpdateCamera() {
        this.GetComponent<Transform>().position = scale * (new Vector3(Mathf.Sin(angle) * distanceFromCenter, heightFromCenter, Mathf.Cos(angle) * distanceFromCenter));
        this.GetComponent<Transform>().LookAt(new Vector3(0, -10, 0));
    }
}
