using System;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Models.DataManagement.Sync;

public static class SyncCursorUtil                                                                                                                                     
{                                                                                                                                                                  
    public static ISyncCursor MinValue { get; } = new InMemorySyncCursor(DateTimeOffset.MinValue, 0);                                                              
                                                                                                                                                                     
    private sealed record InMemorySyncCursor(DateTimeOffset ITs, long ISeq) : ISyncCursor;                                                                         
}                                                                 