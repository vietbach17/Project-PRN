using Microsoft.Extensions.Configuration;

namespace ManagementHotel;

public static class Config
{
    private static IConfigurationRoot? _config;

    private static IConfigurationRoot Ensure()
    {
        if (_config != null) return _config;
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        _config = builder.Build();
        return _config;
    }

    public static string GetConnectionString()
    {
        var cfg = Ensure();
        return cfg["HotelDb"] ?? string.Empty;
    }
}
