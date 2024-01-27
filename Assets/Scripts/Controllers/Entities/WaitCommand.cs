using Controllers.Commands;

namespace Controllers.Entities
{
	public class WaitCommand : Command<WaitCommand>
	{
		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
}