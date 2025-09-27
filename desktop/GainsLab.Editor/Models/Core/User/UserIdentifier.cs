namespace GainsLab.Models.Core.User;

public class UserIdentifier : Identifier
{
    
    public UserIdentifier(int? id,string name) : base(id, name)
    {
      
    }

    public string Name { get => UID; set => UID = value; }
    
    
    
}

public class EmptyUserIdentifier : EmptyIdentifier;