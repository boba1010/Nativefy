using System.ComponentModel.DataAnnotations;

namespace Nativefy.Engine;

public sealed class AppStateEntity
{
    [Key]
    public string Id { get; set; } = "Global";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<PlayableItemDto> LikedItems { get; set; } = [];
}
