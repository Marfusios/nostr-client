using Microsoft.JSInterop;

namespace NostrDebug.Web.Utils;

public class ClipboardService
{
    private readonly IJSRuntime _jsInterop;
    public ClipboardService(IJSRuntime jsInterop)
    {
        _jsInterop = jsInterop;
    }
    public async Task CopyToClipboard(string? text)
    {
        await _jsInterop.InvokeVoidAsync("navigator.clipboard.writeText", text ?? string.Empty);
    }
}