using ContractsStatus = TunnlR.Contract.DTOs.Enums.TunnelStatus;
using DomainStatus = TunnlR.Domain.DTOs.Enums.TunnelStatus;

namespace TunnlR.Application.Mappings;

public static class TunnelStatusMapper
{
    public static DomainStatus ToDomain(ContractsStatus status)
        => status switch
        {
            ContractsStatus.Active => DomainStatus.Active,
            ContractsStatus.Inactive => DomainStatus.Inactive,
            ContractsStatus.Deactivated => DomainStatus.Deactivated,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };

    public static ContractsStatus ToContract(DomainStatus status)
        => status switch
        {
            DomainStatus.Active => ContractsStatus.Active,
            DomainStatus.Inactive => ContractsStatus.Inactive,
            DomainStatus.Deactivated => ContractsStatus.Deactivated,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
}
