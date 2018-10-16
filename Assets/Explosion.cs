﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    [SerializeField] float pushForce = 1f;
    [SerializeField] float damageMultiplier = 100f;

	// Use this for initialization
	void Start () {
        //foreach (var item in find)
        //{

       // }





	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log(collision.name);

        if (collision.GetComponent<Rigidbody2D>() && !collision.GetComponent<oil>())
        {
            Debug.Log(collision.gameObject.transform.position);

            Vector3 dir = collision.gameObject.transform.position - this.transform.position;
            dir.Normalize();
            Debug.Log(dir);
            collision.GetComponent<Rigidbody2D>().AddForce(pushForce * dir);
             //   * (1f / Vector3.Distance(this.transform.position, collision.gameObject.transform.position)));

            if (collision.GetComponent<playerController>())
            {
                collision.GetComponent<playerController>().
                    takeDamage(damageMultiplier* (1f / Vector3.Distance(this.transform.position, collision.gameObject.transform.position)));
            }

        }

        if (collision.GetComponent<oil>())
        {
            collision.GetComponent<oil>().burn();
        }
        
    }

}
