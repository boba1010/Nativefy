using System.ComponentModel.DataAnnotations;

namespace Nativefy.Engine;

public class SearchResultEntity
{
    [Key]
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public List<PlayableItemDto> Results { get; set; } = [];
}
