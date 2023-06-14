namespace xACME.Models.Acme
{
    public enum AuthZStatus
    {
        pending,
        valid,
        invalid,
        deactivated,
        expired,
        revoked,
        ready
    }
}