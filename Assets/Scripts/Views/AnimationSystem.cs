using System.Collections;
using UnityEngine;

namespace Views
{
	public class AnimationSystem : MonoBehaviour
	{
		public bool IsPlaying { get; set; }

		//cada instancia de animacion tiene la lista de "participantes" y asi podemos definir que animaciones tiene depedencia
		//Agregar modos de reproduccion. Lineal o Concurrente
		public void Play(AnimationInstance animationInstance)
		{

			//StartCoroutine(PlayAndPauseAnimation(animationInstance));
		}

		/*private IEnumerator PlayAndPauseAnimation(AnimationInstance animationInstance)
		{
			
		}*/
	}


	public class AnimationInstance
	{
		public IEnumerator Animation;
		public int[] Participants;
		public bool IsPaused;
	}
}