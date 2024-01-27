using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Views
{
	public class AnimationSystem : MonoBehaviour
	{
		public bool IsPlaying => activeAnimations.Count > 0;

		//cada instancia de animacion tiene la lista de "participantes" y asi podemos definir que animaciones tiene depedencia
		//Agregar modos de reproduccion. Lineal o Concurrente

		private readonly List<AnimationInstance> activeAnimations = new List<AnimationInstance>();
		
		[ShowInInspector] public bool IsPaused { get; set; }

		public Mode mode;
		public enum Mode
		{
			Sequential,
			Parallel
		}
		
		public AnimationInstance Play(IEnumerator animationRoutine, params object[] participants)
		{
			var animationInstance = new AnimationInstance(animationRoutine,participants);
			activeAnimations.Add(animationInstance);
			StartCoroutine(PlayAndPauseAnimation(animationInstance));
			return animationInstance;
		}

		private IEnumerator PlayAndPauseAnimation(AnimationInstance animationInstance)
		{
			while (mode == Mode.Sequential && activeAnimations[0] != animationInstance)
			{
				yield return null;
			}
			
			while (animationInstance.Animation.MoveNext())
			{
				yield return animationInstance.Animation.Current;
				while (animationInstance.IsPaused || IsPaused)
				{
					yield return null;
				}
			}
			activeAnimations.Remove(animationInstance);
		}
	}


	public class AnimationInstance
	{
		public IEnumerator Animation;
		public object[] Participants;
		public bool IsPaused;

		public AnimationInstance(IEnumerator animationRoutine, object[] participants)
		{
			Animation = animationRoutine;
			Participants = participants;
		}
	}
}