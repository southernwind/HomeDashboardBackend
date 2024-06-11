using Database.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataBase {
	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="options">DbContextOptions</param>
	public class HomeServerDbContext(DbContextOptions options) : DbContext(options) {
		/// <summary>
		/// 取引履歴
		/// </summary>

		public DbSet<MfTransaction> MfTransactions {
			get;
			set;
		} = null!;

		/// <summary>
		/// 資産推移
		/// </summary>
		public DbSet<MfAsset> MfAssets {
			get;
			set;
		} = null!;

		/// <summary>
		/// ユーザー設定
		/// </summary>
		public DbSet<UserSetting> UserSettings {
			get;
			set;
		} = null!;

		/// <summary>
		/// ユーザー設定(WakeOnLan
		/// </summary>
		public DbSet<WakeOnLanTarget> WakeOnLanTargets {
			get;
			set;
		} = null!;

		/// <summary>
		/// Macアドレスベンダーコード
		/// </summary>
		public DbSet<MacAddressVendor> MacAddressVendors {
			get;
			set;
		} = null!;


		/// <summary>
		/// レシピリスト
		/// </summary>
		public DbSet<Recipe> Recipes {
			get;
			set;
		} = null!;

		/// <summary>
		/// 水質情報
		/// </summary>
		public DbSet<WaterState> WaterStates {
			get;
			set;
		} = null!;

		/// <summary>
		/// 電力情報
		/// </summary>
		public DbSet<ElectricPower> ElectricPowers {
			get;
			set;
		} = null!;

		/// <summary>
		/// パルミー動画
		/// </summary>
		public DbSet<Palmie> Palmies {
			get;
			set;
		} = null!;

		/// <summary>
		/// 投資商品
		/// </summary>
		public DbSet<InvestmentProduct> InvestmentProducts {
			get;
			set;
		} = null!;

		/// <summary>
		/// 投資商品レート
		/// </summary>
		public DbSet<InvestmentProductRate> InvestmentProductRates {
			get;
			set;
		} = null!;

		/// <summary>
		/// 取得投資商品
		/// </summary>
		public DbSet<InvestmentProductAmount> InvestmentProductAmounts {
			get;
			set;
		} = null!;

		/// <summary>
		/// 投資通貨単位
		/// </summary>
		public DbSet<InvestmentCurrencyUnit> InvestmentCurrencyUnits {
			get;
			set;
		} = null!;

		/// <summary>
		/// 投資通貨レート
		/// </summary>
		public DbSet<InvestmentCurrencyRate> InvestmentCurrencyRates {
			get;
			set;
		} = null!;

		/// <summary>
		/// 証券口座
		/// </summary>
		public DbSet<TradingAccount> TradingAccounts {
			get;
			set;
		} = null!;

		/// <summary>
		/// 証券口座
		/// </summary>
		public DbSet<TradingAccountCategory> TradingAccountCategories {
			get;
			set;
		} = null!;

		/// <summary>
		/// デイリー資産推移
		/// </summary>
		public DbSet<DailyAssetProgress> DailyAssetProgresses {
			get;
			set;
		} = null!;

		/// <summary>
		/// ヘルスチェック対象
		/// </summary>
		public DbSet<HealthCheckTarget> HealthCheckTargets {
			get;
			set;
		} = null!;

		/// <summary>
		/// ヘルスチェック結果
		/// </summary>
		public DbSet<HealthCheckResult> HealthCheckResults{
			get;
			set;
		} = null!;

		/// <summary>
		/// DBモデル設定
		/// </summary>
		/// <param name="modelBuilder">ModelBuilder</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<MfTransaction>().HasKey(x => x.TransactionId);
			modelBuilder.Entity<MfAsset>().HasKey(x => new { x.Date, x.Institution, x.Category });
			modelBuilder.Entity<UserSetting>().HasKey(x => x.Id);
			modelBuilder.Entity<WakeOnLanTarget>().HasKey(x => x.MacAddress);
			modelBuilder.Entity<MacAddressVendor>().HasKey(x => x.Assignment);
			modelBuilder.Entity<Recipe>().HasKey(x => x.Id);
			modelBuilder.Entity<Recipe>().Property(x => x.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<WaterState>().HasKey(x => x.TimeStamp);
			modelBuilder.Entity<ElectricPower>().HasKey(x => x.TimeStamp);
			modelBuilder.Entity<Palmie>().HasKey(x => x.Id);
			modelBuilder.Entity<Palmie>().Property(x => x.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<InvestmentProduct>().HasKey(x => x.InvestmentProductId);
			modelBuilder.Entity<InvestmentProduct>().Property(x => x.InvestmentProductId).ValueGeneratedOnAdd();
			modelBuilder.Entity<InvestmentProductRate>().HasKey(x => new { x.InvestmentProductId, x.Date });
			modelBuilder.Entity<InvestmentProductAmount>().HasKey(x => new { x.InvestmentProductId, x.InvestmentProductAmountId });
			modelBuilder.Entity<InvestmentCurrencyUnit>().HasKey(x => x.Id);
			modelBuilder.Entity<InvestmentCurrencyUnit>().Property(x => x.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<InvestmentCurrencyRate>().HasKey(x => new { x.InvestmentCurrencyUnitId, x.Date });
			modelBuilder.Entity<TradingAccount>().Property(x => x.TradingAccountId).ValueGeneratedOnAdd();
			modelBuilder.Entity<TradingAccount>().HasKey(x => x.TradingAccountId);
			modelBuilder.Entity<TradingAccountCategory>().HasKey(x => new { x.TradingAccountId, x.TradingAccountCategoryId});
			modelBuilder.Entity<DailyAssetProgress>().HasKey(x => new { x.Date, x.InvestmentProductId });
			modelBuilder.Entity<HealthCheckTarget>().HasKey(x => x.HealthCheckTargetId);
			modelBuilder.Entity<HealthCheckTarget>().Property(x => x.HealthCheckTargetId).ValueGeneratedOnAdd();
			modelBuilder.Entity<HealthCheckResult>().HasKey(x => x.HealthCheckResultId);
			modelBuilder.Entity<HealthCheckResult>().Property(x => x.HealthCheckResultId).ValueGeneratedOnAdd();

			modelBuilder.Entity<InvestmentProductAmount>()
				.HasOne(x => x.InvestmentProduct)
				.WithMany(x => x.InvestmentProductAmounts)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<InvestmentProductAmount>()
				.HasOne(x => x.TradingAccount)
				.WithMany(x => x.InvestmentProductAmounts)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<InvestmentProductRate>()
				.HasOne(x => x.InvestmentProduct)
				.WithMany(x => x.InvestmentProductRates)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<InvestmentProduct>()
				.HasOne(x => x.InvestmentCurrencyUnit)
				.WithMany(x => x.InvestmentProducts)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<InvestmentProductRate>()
				.HasOne(x => x.InvestmentProduct)
				.WithMany(x => x.InvestmentProductRates)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<TradingAccountCategory>()
				.HasOne(x => x.TradingAccount)
				.WithMany(x => x.TradingAccountCategories)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<DailyAssetProgress>()
				.HasOne(x => x.InvestmentProduct)
				.WithMany(x => x.DailyAssetProgresses)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<HealthCheckResult>()
				.HasOne(x => x.HealthCheckTarget)
				.WithMany(x => x.HealthCheckResults)
				.OnDelete(DeleteBehavior.Restrict);

		}
	}
}