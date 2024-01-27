using UnityEngine;

namespace Views
{
	public class EntityView : MonoBehaviour
	{
		[SerializeField] protected SpriteRenderer spriteRenderer;

		void Reset()
		{
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}
	}
}