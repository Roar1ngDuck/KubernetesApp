namespace TodoAppBackend;

public class Todo
{
    public int Id { get; set; }
    public string TodoText { get; set; }
    public bool Done { get; set; }

    public Todo(int id, string todoText, bool done)
    {
        Id = id;
        TodoText = todoText;
        Done = done;
    }
}