using System.Threading.Tasks;

namespace Controllers
{
	public interface IAsyncProvider<T>
	{
		public Task<T> GetAsync();
	}
}