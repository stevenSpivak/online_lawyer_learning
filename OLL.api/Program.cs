namespace OLL.api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            string dbConnectionString = (builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
                + "Database=OLLDB;";
            builder.Services.AddSingleton(new DAL.UserDB(dbConnectionString));
            builder.Services.AddSingleton(new DAL.LawCatalog(dbConnectionString));

            var app = builder.Build();

            string serverConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            await DAL.DataBaseInitializer.InitializeAsync(serverConnectionString);

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}