using UnityEngine;

namespace Views
{
	public class EnemyEntityView : EntityView
	{
		public void LookRight() => spriteRenderer.flipX = false;
		public void LookLeft() => spriteRenderer.flipX = true;
	}
}