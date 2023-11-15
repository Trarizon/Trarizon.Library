namespace Trarizon.TextCommanding;
public interface ITextCommand<TContext>
{
    public void OnExecute(TContext context);
}
