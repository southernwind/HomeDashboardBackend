
using System.Linq;
using System.Threading.Tasks;

using DataBase;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Back.Models.Palmie; 
public class PalmieModel(HomeServerDbContext db) {
	private readonly HomeServerDbContext _db = db;

	public async Task<string> GetAllAsync() {
		var records = await this._db.Palmies.OrderBy(x => x.Id).Select(x => x.Json).ToArrayAsync();
		return $"{"{"}\"courses\":[{string.Join(",", records)}]{"}"}";
	}

	public async Task<string> GetSearchResultAsync(string word) {
		var records = await this._db.Palmies.Where(x => x.Json.Contains(word)).OrderBy(x => x.Id).Select(x => x.Json).ToArrayAsync();
		return $"{"{"}\"courses\":[{string.Join(",", records)}]{"}"}";
	}
}
