
using MyApi.Model.TodoItem;
namespace MyApi.Model.TodoAdminDTO;


public class TodoAdminDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }  // include secret field
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TodoAdminDTO() { }
    public TodoAdminDTO(Todo todoItem) =>
        (Id, Name, IsComplete, Secret, CreatedByUserId, CreatedAt, UpdatedAt) =
        (todoItem.Id, todoItem.Name, todoItem.IsComplete, todoItem.Secret,
        todoItem.CreatedByUserId, todoItem.CreatedAt, todoItem.UpdatedAt);
}
