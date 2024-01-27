using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Views;

namespace Controllers
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private CinemachineBrain brain;
		[SerializeField] private CinemachineTargetGroup targetGroup;
		[SerializeField] private CinemachineVirtualCamera mainVCam;

		private List<EntityView> targetViews;
		
		public void Initialize(EntityView[] views)
		{
			targetViews = new List<EntityView>(views);
			foreach (EntityView view in targetViews)
			{
				targetGroup.AddMember(view.transform, 1, 2);
			}
			mainVCam.Priority = 1;
		}

		public void Terminate()
		{
			mainVCam.Priority = 0;
		}
	}
}