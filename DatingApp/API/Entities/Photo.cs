using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Photos")]
public class Photo
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool IsMain { get; set; }
    public string? PublicId { get; set; }

    // Navigation properties
    // The AppUserId is the foreign key and the AppUser is the navigation property
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;
}