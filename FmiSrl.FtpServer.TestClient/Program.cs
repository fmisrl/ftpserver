using System.Globalization;
using FmiSrl.FtpServer.Server;
using FmiSrl.FtpServer.Server.DependencyInjection;
using FmiSrl.FtpServer.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    Log.Information("Starting FTP Server via Dependency Injection...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.Configure<PhysicalFileSystemProviderOptions>(opt =>
    {
        opt.RootDirectory = "./ftp_root";
    });

    builder.Services.Configure<SimpleAuthenticationProviderOptions>(opt =>
    {
        opt.Username = "admin";
        opt.Password = "password";
    });

    builder.Services.AddFtpServer(options =>
        {
            options.ServerName = "FmiSrl Test FTP Server";
            options.FtpPort = 21;
            options.PasvMinPort = 50000;
            options.PasvMaxPort = 51000;
        })
        .UseFileSystemProvider<PhysicalFileSystemProvider>()
        .UseAuthenticationProvider<SimpleAuthenticationProvider>();

    var host = builder.Build();

    var ftpServer = host.Services.GetRequiredService<FtpServer>();

    await ftpServer.StartAsync();

    Log.Information("FTP Server started successfully. Press Ctrl+C to stop.");

    await host.WaitForShutdownAsync();

    await ftpServer.StopAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return -1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
