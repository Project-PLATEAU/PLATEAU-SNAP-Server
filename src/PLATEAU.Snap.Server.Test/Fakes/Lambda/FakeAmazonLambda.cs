using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.Runtime.Endpoints;
using PLATEAU.Snap.Models;
using PLATEAU.Snap.Models.Lambda;
using System.Text;

namespace PLATEAU.Snap.Server.Test.Fakes.Lambda;

internal class FakeAmazonLambda : IAmazonLambda
{
    public bool IsNotValidHttpStatusCode { get; set; }

    public bool IsNotValidStatusCode { get; set; }

    public bool IsNotValidResponse { get; set; }

    public bool IsNotValidBody { get; set; }

    public bool IsEmptyCoordinates { get; set; }

    public bool IsWktParseFailed { get; set; }

    public bool IsNotValidCoordinates { get; set; }

    public ILambdaPaginatorFactory Paginators => throw new NotImplementedException();

    public IClientConfig Config => throw new NotImplementedException();

    public async Task<InvokeResponse> InvokeAsync(InvokeRequest request, CancellationToken cancellationToken = default)
    {
        var statusCode = !IsNotValidHttpStatusCode ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.NotFound;
        if (IsNotValidResponse)
        {
            return await Task.FromResult(new InvokeResponse
            {
                HttpStatusCode = statusCode,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"test\": 1}"))
            });
        }

        string coordinates;
        if (IsEmptyCoordinates)
        {
            coordinates = string.Empty;
        }
        else if (IsWktParseFailed)
        {
            coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10))";
        }
        else if (IsNotValidCoordinates)
        {
            coordinates = "POLYGON((10 10, 10 20, 20 10, 20 20, 10 10))";
        }
        else
        {
            coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        }

        switch (request.FunctionName)
        {
            case "transform":
                return await Task.FromResult(new InvokeResponse
                {
                    HttpStatusCode = statusCode,
                    Payload = new MemoryStream(Encoding.UTF8.GetBytes(CreateResponse(new LambdaTransformResponse()
                    {
                        Path = "s3://temp/transform.png",
                        Coordinates = coordinates
                    })))
                });
            case "roof_extraction":
                return await Task.FromResult(new InvokeResponse
                {
                    HttpStatusCode = statusCode,
                    Payload = new MemoryStream(Encoding.UTF8.GetBytes(CreateResponse(new LambdaRoofExtractionResponse()
                    {
                        Path = "s3://temp/roof_extraction.png",
                        Coordinates = coordinates
                    })))
                });
            case "apply_texture":
                return await Task.FromResult(new InvokeResponse
                {
                    HttpStatusCode = statusCode,
                    Payload = new MemoryStream(Encoding.UTF8.GetBytes(CreateResponse(new LambdaApplyTextureResponse()
                    {
                        Path = "s3://temp/apply_texture.png",
                        TextureCoordinates = coordinates
                    })))
                });
            default:
                throw new NotImplementedException($"Function {request.FunctionName} is not implemented in FakeAmazonLambda.");
        }
    }

    private string CreateResponse(LambdaTransformResponse response)
    {
        return CreateResponse(Util.Serialize(response));
    }

    private string CreateResponse(LambdaRoofExtractionResponse response)
    {
        return CreateResponse(Util.Serialize(response));
    }

    private string CreateResponse(LambdaApplyTextureResponse response)
    {
        return CreateResponse(Util.Serialize(response));
    }

    private string CreateResponse(string body)
    {
        var statusCode = !IsNotValidStatusCode ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest;
        var responseBody = !IsNotValidBody ? body : "{\"test\": 1}";
        var lambdaResponse = new LambdaResponse
        {
            StatusCode = (int)statusCode,
            Body = responseBody,
        };
        return Util.Serialize(lambdaResponse);
    }

    public Task<AddLayerVersionPermissionResponse> AddLayerVersionPermissionAsync(AddLayerVersionPermissionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CreateAliasResponse> CreateAliasAsync(CreateAliasRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CreateCodeSigningConfigResponse> CreateCodeSigningConfigAsync(CreateCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CreateEventSourceMappingResponse> CreateEventSourceMappingAsync(CreateEventSourceMappingRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunctionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CreateFunctionUrlConfigResponse> CreateFunctionUrlConfigAsync(CreateFunctionUrlConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteAliasResponse> DeleteAliasAsync(DeleteAliasRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteCodeSigningConfigResponse> DeleteCodeSigningConfigAsync(DeleteCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteEventSourceMappingResponse> DeleteEventSourceMappingAsync(DeleteEventSourceMappingRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionResponse> DeleteFunctionAsync(string functionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionResponse> DeleteFunctionAsync(DeleteFunctionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionCodeSigningConfigResponse> DeleteFunctionCodeSigningConfigAsync(DeleteFunctionCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionConcurrencyResponse> DeleteFunctionConcurrencyAsync(DeleteFunctionConcurrencyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionEventInvokeConfigResponse> DeleteFunctionEventInvokeConfigAsync(DeleteFunctionEventInvokeConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteFunctionUrlConfigResponse> DeleteFunctionUrlConfigAsync(DeleteFunctionUrlConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteLayerVersionResponse> DeleteLayerVersionAsync(DeleteLayerVersionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DeleteProvisionedConcurrencyConfigResponse> DeleteProvisionedConcurrencyConfigAsync(DeleteProvisionedConcurrencyConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Endpoint DetermineServiceOperationEndpoint(AmazonWebServiceRequest request)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<GetAccountSettingsResponse> GetAccountSettingsAsync(GetAccountSettingsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetAliasResponse> GetAliasAsync(GetAliasRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetCodeSigningConfigResponse> GetCodeSigningConfigAsync(GetCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetEventSourceMappingResponse> GetEventSourceMappingAsync(GetEventSourceMappingRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionResponse> GetFunctionAsync(string functionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionResponse> GetFunctionAsync(GetFunctionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionCodeSigningConfigResponse> GetFunctionCodeSigningConfigAsync(GetFunctionCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionConcurrencyResponse> GetFunctionConcurrencyAsync(GetFunctionConcurrencyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionConfigurationResponse> GetFunctionConfigurationAsync(string functionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionConfigurationResponse> GetFunctionConfigurationAsync(GetFunctionConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionEventInvokeConfigResponse> GetFunctionEventInvokeConfigAsync(GetFunctionEventInvokeConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionRecursionConfigResponse> GetFunctionRecursionConfigAsync(GetFunctionRecursionConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetFunctionUrlConfigResponse> GetFunctionUrlConfigAsync(GetFunctionUrlConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetLayerVersionResponse> GetLayerVersionAsync(GetLayerVersionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetLayerVersionByArnResponse> GetLayerVersionByArnAsync(GetLayerVersionByArnRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetLayerVersionPolicyResponse> GetLayerVersionPolicyAsync(GetLayerVersionPolicyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetPolicyResponse> GetPolicyAsync(GetPolicyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetProvisionedConcurrencyConfigResponse> GetProvisionedConcurrencyConfigAsync(GetProvisionedConcurrencyConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GetRuntimeManagementConfigResponse> GetRuntimeManagementConfigAsync(GetRuntimeManagementConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeWithResponseStreamResponse> InvokeWithResponseStreamAsync(InvokeWithResponseStreamRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListAliasesResponse> ListAliasesAsync(ListAliasesRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListCodeSigningConfigsResponse> ListCodeSigningConfigsAsync(ListCodeSigningConfigsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListEventSourceMappingsResponse> ListEventSourceMappingsAsync(ListEventSourceMappingsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListFunctionEventInvokeConfigsResponse> ListFunctionEventInvokeConfigsAsync(ListFunctionEventInvokeConfigsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListFunctionsResponse> ListFunctionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListFunctionsResponse> ListFunctionsAsync(ListFunctionsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListFunctionsByCodeSigningConfigResponse> ListFunctionsByCodeSigningConfigAsync(ListFunctionsByCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListFunctionUrlConfigsResponse> ListFunctionUrlConfigsAsync(ListFunctionUrlConfigsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListLayersResponse> ListLayersAsync(ListLayersRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListLayerVersionsResponse> ListLayerVersionsAsync(ListLayerVersionsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListProvisionedConcurrencyConfigsResponse> ListProvisionedConcurrencyConfigsAsync(ListProvisionedConcurrencyConfigsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListTagsResponse> ListTagsAsync(ListTagsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ListVersionsByFunctionResponse> ListVersionsByFunctionAsync(ListVersionsByFunctionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PublishLayerVersionResponse> PublishLayerVersionAsync(PublishLayerVersionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PublishVersionResponse> PublishVersionAsync(PublishVersionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutFunctionCodeSigningConfigResponse> PutFunctionCodeSigningConfigAsync(PutFunctionCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutFunctionConcurrencyResponse> PutFunctionConcurrencyAsync(PutFunctionConcurrencyRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutFunctionEventInvokeConfigResponse> PutFunctionEventInvokeConfigAsync(PutFunctionEventInvokeConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutFunctionRecursionConfigResponse> PutFunctionRecursionConfigAsync(PutFunctionRecursionConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutProvisionedConcurrencyConfigResponse> PutProvisionedConcurrencyConfigAsync(PutProvisionedConcurrencyConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PutRuntimeManagementConfigResponse> PutRuntimeManagementConfigAsync(PutRuntimeManagementConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RemoveLayerVersionPermissionResponse> RemoveLayerVersionPermissionAsync(RemoveLayerVersionPermissionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TagResourceResponse> TagResourceAsync(TagResourceRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UntagResourceResponse> UntagResourceAsync(UntagResourceRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateAliasResponse> UpdateAliasAsync(UpdateAliasRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateCodeSigningConfigResponse> UpdateCodeSigningConfigAsync(UpdateCodeSigningConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateEventSourceMappingResponse> UpdateEventSourceMappingAsync(UpdateEventSourceMappingRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateFunctionCodeResponse> UpdateFunctionCodeAsync(UpdateFunctionCodeRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateFunctionConfigurationResponse> UpdateFunctionConfigurationAsync(UpdateFunctionConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateFunctionEventInvokeConfigResponse> UpdateFunctionEventInvokeConfigAsync(UpdateFunctionEventInvokeConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateFunctionUrlConfigResponse> UpdateFunctionUrlConfigAsync(UpdateFunctionUrlConfigRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
