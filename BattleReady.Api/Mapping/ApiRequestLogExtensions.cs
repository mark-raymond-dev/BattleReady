using BattleReady.Api.Models.Responses;
using BattleReady.Data.Entities;

namespace BattleReady.Api.Mapping;

public static class ApiRequestLogExtensions
{
    public static ApiRequestLogDto ToDto(this ApiRequestLog log)
    {
        return new ApiRequestLogDto
        {
            Id             = log.Id,
            Timestamp      = log.Timestamp,
            Endpoint       = log.Endpoint,
            RequestBody    = log.RequestBody,
            ResponseBody   = log.ResponseBody,
            ResponseStatus = log.ResponseStatus
        };
    }
}