namespace Views.Entities
{
	public class EnemyEntityView : EntityView
	{
		public void LookRight() => spriteRenderer.flipX = false;
		public void LookLeft() => spriteRenderer.flipX = true;
	}
}