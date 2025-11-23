using MyApi.Model;
namespace MyApi.DTOs;


public class TodoAdminDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }  // 包含 Secret 字段

    public TodoAdminDTO() { }
    public TodoAdminDTO(Todo todoItem) =>
        (Id, Name, IsComplete, Secret) = (todoItem.Id, todoItem.Name, todoItem.IsComplete, todoItem.Secret);
}
