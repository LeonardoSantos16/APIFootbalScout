namespace APIFootballScout.Infrastructure.Security
{
    public sealed record TokenSubject(
        Guid Id,
        string Email,
        string TenantId,
        string SecurityStamp,
        IReadOnlyCollection<string> Roles);
}
