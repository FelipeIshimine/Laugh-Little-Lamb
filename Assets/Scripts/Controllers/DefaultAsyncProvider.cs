using System.Threading.Tasks;

namespace Controllers
{
	public class DefaultAsyncProvider<T> : IAsyncProvider<T> where T : new()
	{
		private static readonly T Request = new T();
		public Task<T> GetAsync() => Task.FromResult(Request);
	}
}