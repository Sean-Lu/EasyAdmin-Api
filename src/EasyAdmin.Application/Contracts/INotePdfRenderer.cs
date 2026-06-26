namespace EasyAdmin.Application.Contracts;

public interface INotePdfRenderer
{
    Task<byte[]> RenderAsync(string html);
}
