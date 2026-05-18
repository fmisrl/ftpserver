using System.Diagnostics;
using FmiSrl.FtpServer.Server.Abstractions;

namespace FmiSrl.FtpServer.Server.Infrastructure;

/// <summary>
/// A middleware that provides diagnostics for FTP commands.
/// </summary>
public class FtpDiagnosticsMiddleware : IFtpCommandMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(FtpCommandContext context, Func<Task> next)
    {
        using var activity = FtpDiagnostics.ActivitySource.StartActivity($"FTP {context.Verb}");
        activity?.SetTag("ftp.verb", context.Verb);
        activity?.SetTag("ftp.args", context.Arguments);
        activity?.SetTag("ftp.session_id", context.Session.Id);
        
        var sw = Stopwatch.StartNew();
        try
        {
            await next();
            
            if (context.Response != null)
            {
                activity?.SetTag("ftp.response_code", context.Response.Code);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            sw.Stop();
            var responseCode = context.Response?.Code ?? 0;
            
            FtpDiagnostics.CommandsExecutedCounter.Add(1, 
                new KeyValuePair<string, object?>("ftp.verb", context.Verb),
                new KeyValuePair<string, object?>("ftp.response_code", responseCode));
            
            FtpDiagnostics.CommandDurationHistogram.Record(sw.Elapsed.TotalMilliseconds, 
                new KeyValuePair<string, object?>("ftp.verb", context.Verb));
        }
    }
}
