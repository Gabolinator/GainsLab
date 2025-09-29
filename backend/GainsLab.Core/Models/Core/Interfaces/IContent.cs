using System.Collections.Generic;

namespace GainsLab.Models.Core.Interfaces;

public interface INestedContent<TNestedContent>
{
    public TNestedContent? NestedContent { get; }

}