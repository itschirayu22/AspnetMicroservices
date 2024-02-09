using Npgsql;

namespace Discount.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;
            using(var scope = host.Services.CreateScope())
            {
                var service = scope.ServiceProvider;
                var configuration = service.GetRequiredService<IConfiguration>();
                var logger = service.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrating postgressql database.");
                    using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                    connection.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };

                    command.CommandText = "DROP TABLE IF EXISTS Coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
                    command.ExecuteNonQuery();


                    command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
                    command.ExecuteNonQuery();
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occured while migrating the postresql database.");

                    if(retryForAvailability < 50) {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }

            }

            return host;

        }
    }
}
