using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPiece : MonoBehaviour {
    public bool moving;
    public bool alive;
    public bool bouncing, bouncingPrime;
    public Vector3 bouncingInf;
    public String puid;

    public Queue<GameObject> destination;
    public float speed = 1.0f;

    public int curHP = 50, maxHP = 100;
    public float scale = 5.0f;

    private GameObject displayCurHP, displayHP, parentHP; 
    // Start is called before the first frame update
    void Start() {
        alive = true;
        moving = false;
        bouncing = false;
        bouncingPrime = false;
        bouncingInf = new Vector3();
        destination = new Queue<GameObject>();

        parentHP = transform.Find("HP").gameObject;
        displayCurHP = transform.Find("HP").Find("CurrentHP").gameObject;
        displayHP = transform.Find("HP").Find("NumberHP").gameObject;
    }

    // Update is called once per frame
    void Update() {
        if (!alive) {
            //transform.position = Vector3.MoveTowards(transform.position, destination.Peek().GetComponent<Transform>().position, speed);
            //doing stuff
            Die();
            return;
        }
        if (bouncing) {
            Bounce();
        } else if (moving) {
            Move();
        }

        UpdateHP();

    }

    void UpdateHP() {
        float percentHP = Mathf.Clamp(1.0f * curHP / maxHP, 0, 1);
        displayCurHP.transform.localScale = new Vector3(percentHP * scale, 1f, 1f);
        if (percentHP < 0.50f) {
            displayCurHP.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        displayHP.GetComponent<TextMesh>().text = "" + curHP;
    }

    void Bounce() {
        if (destination.Count == 0) {
            bouncing = false;
            return;
        }
        if (bouncingPrime) {
            this.transform.position = Vector3.MoveTowards(transform.position, destination.Peek().GetComponent<Transform>().position, speed);
            if (transform.position.Equals(destination.Peek().GetComponent<Transform>().position)) {
                destination.Dequeue();
                bouncing = false;
                bouncingPrime = false;
            }
        } else {
            this.transform.position = Vector3.MoveTowards(transform.position, bouncingInf, speed);
            if (transform.position.Equals(bouncingInf)) {
                destination.Dequeue();
                bouncingPrime = true;
            }
        }
    }

    void Move() {
        if (destination.Count == 0) {
            moving = false;
            return;
        }
        this.transform.position = Vector3.MoveTowards(transform.position, destination.Peek().GetComponent<Transform>().position, speed);
        if (transform.position.Equals(destination.Peek().GetComponent<Transform>().position)) {
            destination.Dequeue();
        }
    }

    void Die() {
        this.transform.rotation = Quaternion.RotateTowards(
            this.transform.rotation,
            Quaternion.Euler(90, 0, 0),
            speed * 5
        );
        if (this.transform.rotation == Quaternion.Euler(90, 0, 0)) {
            Invoke("Disappear", 1);
        }
    }

    void Disappear() {
        GameObject.Destroy(this.gameObject);
    }

    public void ShowHPBar() {
        parentHP.SetActive(true);
    }

    public void HideHPBar() {
        parentHP.SetActive(false);
    }
}
