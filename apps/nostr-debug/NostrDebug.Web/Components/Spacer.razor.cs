using Microsoft.AspNetCore.Components;

namespace NostrDebug.Web.Components;

public partial class Spacer
{
    /// <summary>
    /// Gets or sets the width of the spacer (in pixels)
    /// </summary>
    [Parameter]
    public int? Width { get; set; }
}
