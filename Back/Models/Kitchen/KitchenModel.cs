using System.Net.Http;
using System.Threading.Tasks;

using Database.Tables;

using DataBase;

using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Back.Models.Kitchen; 
public class KitchenModel(HomeServerDbContext db, IHttpClientFactory clientFactory) {
	private readonly HomeServerDbContext _db = db;
	private readonly IHttpClientFactory _clientFactory = clientFactory;

	public async Task RegisterRecipeAsync(Recipe recipe) {
		if (string.IsNullOrWhiteSpace(recipe.ImageUrl) || string.IsNullOrWhiteSpace(recipe.Title)) {
			var client = this._clientFactory.CreateClient();
			var html = await client.GetStringAsync(recipe.Url);
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);
			if (string.IsNullOrWhiteSpace(recipe.Title)) {
				recipe.Title = htmlDoc
					.DocumentNode
					.QuerySelector("head title")?.InnerText;
			}

			if (string.IsNullOrWhiteSpace(recipe.ImageUrl)) {
				recipe.ImageUrl = htmlDoc.DocumentNode.QuerySelector("head meta[property=og:image]")
					?.GetAttributeValue("content", null);
			}
		}

		await this._db.Recipes.AddAsync(recipe);
		await this._db.SaveChangesAsync();
	}

	public async Task DeleteRecipeAsync(Recipe recipe) {
		await using var tran = await this._db.Database.BeginTransactionAsync();
		var record = await this._db.Recipes.SingleAsync(x => x.Id == recipe.Id);
		this._db.Recipes.Remove(record);
		await this._db.SaveChangesAsync();
		await tran.CommitAsync();
	}

	public async Task<Recipe[]> GetRecipeListAsync() {
		return await this._db.Recipes.ToArrayAsync();
	}
}
