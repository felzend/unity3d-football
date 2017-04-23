using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.gameObject.AddComponent<Rigidbody>();
        this.gameObject.AddComponent<BoxCollider>();

        this.GetComponent<Rigidbody>().mass = 10000;        
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        this.GetComponent<BoxCollider>().center = new Vector3(0, 1, 0);
        this.GetComponent<BoxCollider>().size = new Vector3(50, 0, 50);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
