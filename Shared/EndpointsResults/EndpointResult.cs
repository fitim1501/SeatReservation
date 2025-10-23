using System.Reflection;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Shared.EndpointsResults;

public class EndpointResult<TValue> : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;
    
    public EndpointResult(Result<TValue, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorsResult(result.Error);
    }
    
    public EndpointResult(Result<TValue, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorsResult(result.Error);
    }
    
    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);
    
    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);
    
    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);
    
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);
        
        var statusCodes = new[] { 200, 400, 401, 403, 404, 409, 500 };
        
        foreach (var statusCode in statusCodes)
        {
            builder.Metadata.Add(new ProducesResponseTypeMetadata(
                statusCode, 
                typeof(Envelope<TValue>), 
                ["application/json"]));
        }
    }
}