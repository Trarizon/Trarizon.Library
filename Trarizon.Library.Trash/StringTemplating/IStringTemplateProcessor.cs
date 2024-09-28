namespace Trarizon.Library.Trash.StringTemplating;
public interface IStringTemplateProcessor<TResult>
{
    TResult Process(StringTemplate template);
}
