using UnityEngine;

namespace Views
{
	public class PlayerEntityView : EntityView
	{
		[SerializeField] private Sprite[] sprites;

		public void SetLookDirection(int index) => spriteRenderer.sprite = sprites[index];

	}
}