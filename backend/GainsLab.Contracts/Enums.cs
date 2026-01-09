namespace GainsLab.Contracts;


    public enum UpdateRequest { DontUpdate, Update}

    public enum CreateRequest { Create, DontCreate}
    public enum UpsertOutcome { Created, Updated, Failed }

    public enum UpdateOutcome { NotUpdated, Updated, NotRequested ,Failed}
    public enum DeleteOutcome { Deleted,Canceled,Failed}
    public enum CreateOutcome { Created, AlreadyExist ,Canceled,Failed}
