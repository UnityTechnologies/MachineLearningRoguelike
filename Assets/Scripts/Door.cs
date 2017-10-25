using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	private Animator animator;
	private int closeHash, openHash;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		closeHash = Animator.StringToHash("Close");
		openHash = Animator.StringToHash("Open");
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		animator.SetTrigger(openHash);
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		animator.SetTrigger(closeHash);
	}
}
