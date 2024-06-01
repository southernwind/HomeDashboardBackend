using System.Linq;
using System.Threading.Tasks;

using Back.Models.HealthCheck.ResponseDto;

using DataBase;

using Microsoft.EntityFrameworkCore;

namespace Back.Models.HealthCheck;
/// <summary>
/// ヘルスチェックデータベースの操作
/// </summary>
public class HealthCheckModel {
	/// <summary>
	/// データベース
	/// </summary>
	private readonly HomeServerDbContext _db;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="db">データベース</param>
	public HealthCheckModel(HomeServerDbContext db) {
		this._db = db;
	}

	/// <summary>
	/// 最新状況取得
	/// </summary>
	/// <returns>最新状況</returns>
	public async Task<HealthCheckResultResponseDto[]> GetLatestResultAsync() {
		var idList = from r in this._db.HealthCheckResults
					 group r by r.HealthCheckTargetId into g
					 select g.Max(x => x.HealthCheckResultId);

		var query = from t in this._db.HealthCheckTargets
			   join r in this._db.HealthCheckResults
				on t.HealthCheckTargetId equals r.HealthCheckTargetId
			   where idList.Any(x => x == r.HealthCheckResultId)
			   select new HealthCheckResultResponseDto() {
				   HealthCheckTargetId = t.HealthCheckTargetId,
				   Name = t.Name,
				   Host = t.Host,
				   CheckType = t.CheckType,
				   IsEnable = t.IsEnable,
				   HealthCheckResultId = r.HealthCheckResultId,
				   DateTime = r.DateTime,
				   State = r.State,
				   Reason = r.Reason
			   };
		return
			await query
				.OrderByDescending(x => x.IsEnable)
				.OrderBy(x => x.State)
				.OrderBy(x => x.HealthCheckTargetId)
				.ToArrayAsync();
	}
}
