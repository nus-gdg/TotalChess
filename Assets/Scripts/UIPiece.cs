using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPiece : MonoBehaviour {
    public const int DAMAGE_CRITICAL_FAIL = 0, DAMAGE_NORMAL = 1, DAMAGE_BONUS = 10, DAMAGE_GANKED2 = 2, DAMAGE_GANKED3 = 3, DAMAGE_GANKED4 = 4;

    public bool moving;
    public bool alive;
    public bool bouncing, bouncingPrime;
    public Vector3 bouncingInf;
    public String puid;

    public Queue<GameObject> destination;
    public float speed = 1.0f;

    public int curHP = 50, maxHP = 100;
    public float scale = 5.0f;

    public GameObject damageNumbers;
    public Vector3 damageHeightVector = new Vector3(0, 17, 0);

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
        if (parentHP.activeInHierarchy) {
            parentHP.transform.LookAt(Camera.main.transform);
        }

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
        this.transform.Find("DieRotator").rotation = Quaternion.RotateTowards(
            this.transform.Find("DieRotator").rotation,
            Quaternion.Euler(90, 0, 0),
            speed * 5
        );
        if (this.transform.Find("DieRotator").rotation == Quaternion.Euler(90, 0, 0)) {
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

    public void TakeDamage(int damage) {
        TakeDamage(damage, DAMAGE_NORMAL);
    }

    public void TakeDamage(int damage, int flag) {
        switch (flag) {
            case 0:
                curHP -= damage;
                GameObject display0 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display0.GetComponent<TextMesh>().color = Color.blue;
                display0.GetComponent<TextMesh>().text = "" + damage;
                break;
            case 1:
                curHP -= damage;
                GameObject display1 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display1.GetComponent<TextMesh>().text = "" + damage;
                break;
            case 2:
                curHP -= damage;
                GameObject display2 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display2.GetComponent<TextMesh>().characterSize *= 0.8f;
                display2.GetComponent<TextMesh>().text = "" + damage;
                break;
            case 3:
                curHP -= damage;
                GameObject display3 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display3.GetComponent<TextMesh>().characterSize *= 0.7f;
                display3.GetComponent<TextMesh>().text = "" + damage;
                break;
            case 4:
                curHP -= damage;
                GameObject display4 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display4.GetComponent<TextMesh>().characterSize *= 0.5f;
                display4.GetComponent<TextMesh>().text = "" + damage;
                break;
            case 10:
                curHP -= damage;
                GameObject display10 = Instantiate(damageNumbers, this.transform.position + damageHeightVector, Quaternion.identity);
                display10.GetComponent<TextMesh>().characterSize *= 1.2f;
                display10.GetComponent<TextMesh>().color = Color.red;
                display10.GetComponent<TextMesh>().text = "" + damage;
                break;
            default:
                curHP -= damage;
                break;
        }
        UpdateHP();
    }
}
