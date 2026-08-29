namespace TaskApi.Core.Entities;
public class TaskEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Done { get; set; } = false;
}