using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class AppCodeDto : DtoBase
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public CommonState State { get; set; }
}

public class AppCodeAddDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}

public class AppCodeUpdateDto : DtoIdBase
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public CommonState State { get; set; }
}

public class AppCodePageReqDto : PageRequestBase
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}