namespace GainsLab.WebLayer.Model.Dropdown;

public sealed class DropdownConfig
{
    public IEnumerable<DropdownItem> Items { get; set; } = new List<DropdownItem>();
    public IEnumerable<string> ItemsLabel => Items.Select(x => x.Label);

    public string? InitialValue => InitialSelectedLabel ?? 
                                   (InitialSelectedIndex < 0 || Items.Count() <= InitialSelectedIndex ? 
                                       Placeholder : Items.ElementAt(InitialSelectedIndex).Label);
    
    public string? InitialSelectedLabel { get; set; } = null;

    public int InitialSelectedIndex { get; set; } = -1; //selects placeholder - else selects index

    public string? Placeholder { get; set; } = "Select…";
    public string CssClass { get; set; } = "form-select";
    public bool Disabled { get; set; }
}