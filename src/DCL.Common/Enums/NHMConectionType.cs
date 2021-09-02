namespace DCL.Common.Enums
{
    public enum DCLConectionType
    {
        NONE,
        STRATUM_TCP,
        STRATUM_SSL,
        LOCKED, // inhouse Decloud that are locked on NH (our eqm)
        SSL
    }
}
