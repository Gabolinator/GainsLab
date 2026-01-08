namespace GainsLab.Contracts;

    public enum UpsertOutcome { Created, Updated, Failed }
    public enum UpdateRequest { DontUpdate, Update}
    public enum UpdateOutcome { NotUpdated, Updated, NotRequested ,Failed}
