using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VCamTrigger : MonoBehaviour
{
	public CinemachineVirtualCamera vCam;

	private void OnTriggerEnter2D(Collider2D c)
	{
		if(c.CompareTag("Player"))
		{
			CameraManager.Instance.SetActiveVCam(vCam);
		}
	}
}
