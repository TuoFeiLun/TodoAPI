using MyApi.Model.TodoItem;
namespace MyApi.Model.TodoItemDTO;


public class TodoItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TodoItemDTO() { }

    public TodoItemDTO(Todo todoItem)
    {
        Id = todoItem.Id;
        Name = todoItem.Name;
        IsComplete = todoItem.IsComplete;
        CreatedByUserId = todoItem.CreatedByUserId;
        CreatedAt = todoItem.CreatedAt;
        UpdatedAt = todoItem.UpdatedAt;
    }
}
