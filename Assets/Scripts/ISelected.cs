public interface ISelected
{
    public abstract bool IsSelect { get; }
    public abstract bool IsPreselect { get; }
    public abstract void Select();
    public abstract void Preselect();
}
