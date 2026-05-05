namespace ORVWiki.Application.Auth;

public static class Roles
{
    public const string Admin = "admin";
    public const string Editor = "editor";
    public const string Reader = "reader";

    public static readonly IReadOnlyList<string> All = [Admin, Editor, Reader];
}
