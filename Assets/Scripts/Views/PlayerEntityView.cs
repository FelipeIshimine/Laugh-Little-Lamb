using UnityEngine;

namespace Views
{
	public class PlayerEntityView : EntityView
	{
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private Sprite[] sprites;

		public void SetLookDirection(EntityOrientation entityOrientation) => spriteRenderer.sprite = sprites[(int)entityOrientation];

		[System.Serializable]
		public enum EntityOrientation
		{
			Up = 0,
			Right = 1,
			Down = 2,
			Left = 3
		}

	}
}