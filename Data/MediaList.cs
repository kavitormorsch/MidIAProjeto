using System.ComponentModel.DataAnnotations.Schema;

namespace MidIAProjeto.Data;

public class MediaList
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ListDetails GeneratedList { get; set; }
}

public class ListDetails
{
    public string UserPrompt { get; set; }
    public string GeneratedList { get; set; }
}