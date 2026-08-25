namespace APIFootballScout.Infrastructure.Security
{
    public sealed record TokenSubject(
        Guid Id,
        string Email,
        string SecurityStamp,
        IReadOnlyCollection<string> Roles);
}
