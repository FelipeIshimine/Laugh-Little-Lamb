using UnityEngine;

namespace Views
{
	public class EnemyEntityView : EntityView
	{
		[SerializeField] private SpriteRenderer renderer;

		public void LookRight() => renderer.flipX = false;
		public void LookLeft() => renderer.flipX = true;
	}
}