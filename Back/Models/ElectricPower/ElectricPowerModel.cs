using System;
using System.Linq;
using System.Threading.Tasks;

using Back.Models.ElectricPower.ResponseDto;

using DataBase;

using Microsoft.EntityFrameworkCore;

namespace Back.Models.ElectricPower;
public class ElectricPowerModel(HomeServerDbContext db) {
	private readonly HomeServerDbContext _db = db;

	/// <summary>
	/// 電力消費量取得
	/// </summary>
	/// <param name="from">開始日時</param>
	/// <param name="to">終了日時</param>
	/// <returns>水質状態</returns>
	public async Task<ElectricPowerConsumptionResponseDto[]> GetElectricPowerConsumptionListAsync(string from, string to) {
		var records = await
			this._db.ElectricPowers
				.Where(x => x.TimeStamp >= DateTime.Parse(from) && x.TimeStamp <= DateTime.Parse(to))
				.OrderBy(x => x.TimeStamp)
				.ToArrayAsync();

		return records.Select(x =>
			new ElectricPowerConsumptionResponseDto {
				TimeStamp = x.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
				ElectricPower = x.ElectricPowerValue
			})
			.ToArray();
		;
	}
}
