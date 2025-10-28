namespace GainsLab.Core.Models.Core.Interfaces.DB;

public interface ISyncPage<TSyncDto> where TSyncDto : ISyncDto
{
public IReadOnlyList<TSyncDto> ItemsList { get; }
ISyncCursor? NextPage { get; }
DateTimeOffset Time { get; }
}

public interface ISyncDto{}

public interface ISyncCursor
{
  public DateTimeOffset ITs { get;}
  public long ISeq { get;}
}