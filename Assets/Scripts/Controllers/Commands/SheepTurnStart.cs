namespace Controllers.Commands
{
	public class SheepTurnStart : Command<SheepTurnStart>
	{
		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
	
	public class SheepTurnEnd : Command<SheepTurnEnd>
	{
		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
	
	public class EnemyTurnStart : Command<EnemyTurnStart>
	{
		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
	
	public class EnemyTurnEnd : Command<EnemyTurnEnd>
	{
		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
}