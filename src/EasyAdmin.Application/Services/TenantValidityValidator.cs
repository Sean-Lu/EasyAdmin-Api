using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

public static class TenantValidityValidator
{
    public static void Validate(DateTime? startTime, DateTime? expireTime)
    {
        if (startTime.HasValue && expireTime.HasValue && startTime.Value >= expireTime.Value)
        {
            throw new ExplicitException("生效时间必须早于到期时间");
        }
    }
}
