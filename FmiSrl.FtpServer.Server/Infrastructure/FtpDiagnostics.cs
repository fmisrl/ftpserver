using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// Provides diagnostic tools for tracing and metrics.
/// </summary>
public static class FtpDiagnostics
{
    /// <summary>
    /// The prefix used for all traces and metrics.
    /// </summary>
    public const string ServiceName = "FmiSrl.FtpServer";

    /// <summary>
    /// The <see cref="ActivitySource"/> for tracing.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName);

    /// <summary>
    /// The <see cref="Meter"/> for metrics.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName);
    
    /// <summary>
    /// Counter for commands executed.
    /// </summary>
    public static readonly Counter<long> CommandsExecutedCounter = Meter.CreateCounter<long>("ftp.commands.executed", description: "Total number of FTP commands executed");

    /// <summary>
    /// Histogram for command execution duration.
    /// </summary>
    public static readonly Histogram<double> CommandDurationHistogram = Meter.CreateHistogram<double>("ftp.commands.duration", unit: "ms", description: "Duration of FTP command execution");
}
