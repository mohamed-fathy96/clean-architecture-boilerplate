namespace CleanArchitecture.Domain.Interfaces;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }

    public void Undo()
    {
        IsDeleted = false;
    }
}
