using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBTest : MonoBehaviour {

	private Rigidbody2D rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		Debug.Log("--------------------  Fixed Update  ---------------------------------");
		Debug.Log("FU Position: " + transform.position.x);
		Debug.Log("FU RB Position: " + rb.position.x);

		rb.MovePosition(transform.position + transform.right);

		Debug.Log("FU New Position: " + transform.position.x);
		Debug.Log("FU New RB Position: " + rb.position.x);
	}

	private void Update()
	{
		Debug.Log("--------------------  Update  ---------------------------------");
		Debug.Log("U Position: " + transform.position.x);
		Debug.Log("U RB Position: " + rb.position.x);
	}
}
