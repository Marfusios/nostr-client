namespace NostrBot.Web.Utils;

public static class TaskUtils
{
    public static async Task DelaySafely(TimeSpan delay, CancellationToken? cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken ?? CancellationToken.None);
        }
        catch (OperationCanceledException e)
        {
            // ignore
        }
    }
}
