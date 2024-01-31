using Sirenix.OdinInspector;
using UnityEngine;

namespace Controllers.Level
{
	public class TimeController : MonoBehaviour
	{
		[SerializeField] private float normalSpeed = 1;
		[SerializeField] private float fastSpeed = 3;
		[Button] public void Normal() => Time.timeScale = normalSpeed;
		[Button] public void Fast() => Time.timeScale = fastSpeed;
	}
}
