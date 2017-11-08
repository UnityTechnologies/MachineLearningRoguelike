using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
	[SerializeField]
	private CinemachineVirtualCamera currentlyActiveVCam;

	public void SetActiveVCam(CinemachineVirtualCamera newVCam)
	{
		newVCam.Priority = 10;
		currentlyActiveVCam.Priority = 0;
		currentlyActiveVCam = newVCam;
	}
}
