namespace GainsLab.WebLayer.Model;

public sealed class DropdownConfig
{
    public IEnumerable<string> Items { get; set; } = new List<string>{"Items 1",  "Items 2", "Items 3", "Items 4"};
    public string? Placeholder { get; set; } = "Select…";
    public string CssClass { get; set; } = "form-select";
    public bool Disabled { get; set; }
}