using MyApi.Model.User;

namespace MyApi.Model.TodoItem;

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }

    // Foreign key to User - records who created this todo
    public int CreatedByUserId { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
