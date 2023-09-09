
using System.Net.Mime;
using System.Threading.Tasks;

using Back.Models.HealthCheck;

using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers;
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/health-check-api/[action]")]
public class HealthCheckController {
	private readonly HealthCheckModel _healthCheckModel;
	public HealthCheckController(HealthCheckModel healthCheckModel) {
		this._healthCheckModel = healthCheckModel;
	}

	[HttpGet]
	[ActionName("get-latest-result")]
	public async Task<ActionResult> GetLatestResultAsync() {
		return new JsonResult(await this._healthCheckModel.GetLatestResultAsync());
	}
}
