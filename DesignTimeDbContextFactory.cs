using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping_Tutorial.Repository;
using System.IO;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // Đọc chuỗi kết nối từ appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Đảm bảo đường dẫn đúng
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("ConnectedDb");

        optionsBuilder.UseSqlServer(connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
