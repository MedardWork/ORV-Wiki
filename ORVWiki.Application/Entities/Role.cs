using System.Text.Json;

namespace ORVWiki.Application.Entities;

public class Role
{
    public short Id { get; set; }
    public string Name { get; set; } = null!;
    public JsonDocument? Permissions { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
