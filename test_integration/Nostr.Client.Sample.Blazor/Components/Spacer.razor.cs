using Microsoft.AspNetCore.Components;

namespace Nostr.Client.Sample.Blazor.Components;

public partial class Spacer
{
    /// <summary>
    /// Gets or sets the width of the spacer (in pixels)
    /// </summary>
    [Parameter]
    public int? Width { get; set; }
}
