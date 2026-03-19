using Amazon.Lambda;
using Amazon.Lambda.Model;
using NetTopologySuite.IO;
using PLATEAU.Snap.Models;
using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Models.Settings;

namespace PLATEAU.Snap.Server.Services;

internal class InvokerService : IInvokerService
{
    private readonly IAmazonLambda lambda;

    private readonly LambdaSettings lambdaSettings;

    public InvokerService(IAmazonLambda lambda, LambdaSettings lambdaSettings)
    {
        this.lambda = lambda;
        this.lambdaSettings = lambdaSettings;
    }

    public async Task<LambdaTransformResponse> TransformAsync(LambdaTransformRequest request)
    {
        var response = await InvokeLambdaAsync<LambdaTransformRequest, LambdaTransformResponse>(this.lambdaSettings.TransformFunctionName, request);

        ValidateCoordinates(response.Coordinates);

        return response;
    }

    public async Task<LambdaRoofExtractionResponse> RoofExtractionAsync(LambdaRoofExtractionRequest request)
    {
        var response = await InvokeLambdaAsync<LambdaRoofExtractionRequest, LambdaRoofExtractionResponse>(this.lambdaSettings.RoofExtractionFunctionName, request);

        ValidateCoordinates(response.Coordinates);

        return response;
    }

    public async Task<LambdaApplyTextureResponse> ApplyTextureAsync(LambdaApplyTextureRequest request)
    {
        var response = await InvokeLambdaAsync<LambdaApplyTextureRequest, LambdaApplyTextureResponse>(this.lambdaSettings.ApplyTextureFunctionName, request);

        ValidateCoordinates(response.TextureCoordinates);

        return response;
    }

    public async Task ExportBuildingAsync(LambdaExportBuildingRequest request)
    {
        await InvokeLambdaAsync(this.lambdaSettings.ExportBuildingFunctionName, request);
    }

    public async Task ExportMeshAsync(LambdaExportMeshRequest request)
    {
        await InvokeLambdaAsync(this.lambdaSettings.ExportMeshFunctionName, request);
    }

    private async Task<TResponse> InvokeLambdaAsync<TRequest, TResponse>(string functionName, TRequest request) where TResponse : LambdaResponseBody
    {
        try
        {
            var invokeRequest = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = Util.Serialize(request),
                InvocationType = InvocationType.RequestResponse
            };

            var invokeResponse = await this.lambda.InvokeAsync(invokeRequest);
            if (invokeResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                using var errorReader = new StreamReader(invokeResponse.Payload);
                var errorJson = await errorReader.ReadToEndAsync();
                var errorResponse = Util.Deserialize<LambdaErrorResponse>(errorJson);
                throw new LambdaOperationException($"Lambda function invocation failed: [{invokeResponse.HttpStatusCode}]{errorResponse?.ErrorMessage ?? "Unknown error"}");
            }

            using var reader = new StreamReader(invokeResponse.Payload);
            var json = await reader.ReadToEndAsync();
            var response = Util.Deserialize<LambdaResponse>(json);
            if (response is null || response.Body is null)
            {
                throw new LambdaOperationException($"Failed to deserialize Lambda response: {json}");
            }

            var responseBody = Util.Deserialize<TResponse>(response.Body);
            if (responseBody is null)
            {
                throw new LambdaOperationException($"Failed to deserialize Lambda response body: {response.Body}");
            }
            if (response.StatusCode != 200)
            {
                throw new LambdaOperationException($"Lambda function returned an error: {responseBody?.Message ?? string.Empty}");
            }

            return responseBody;
        }
        catch (Exception ex)
        {
            throw new LambdaOperationException($"Failed to invoke Lambda function '{functionName}': {ex.Message}", ex);
        }
    }

    private async Task InvokeLambdaAsync<TRequest>(string functionName, TRequest request)
    {
        try
        {
            var invokeRequest = new InvokeRequest
            {
                FunctionName = functionName,
                // .NETのLambdaはCamelCaseでシリアライズ/デシリアライズされるため
                Payload = Util.SerializeCamelCase(request),
                InvocationType = InvocationType.Event
            };

            var invokeResponse = await this.lambda.InvokeAsync(invokeRequest);
            if (invokeResponse.HttpStatusCode != System.Net.HttpStatusCode.Accepted)
            {
                using var errorReader = new StreamReader(invokeResponse.Payload);
                var errorJson = await errorReader.ReadToEndAsync();
                var errorResponse = Util.DeserializeCamelCase<LambdaErrorResponse>(errorJson);
                throw new LambdaOperationException($"Lambda function invocation failed: [{invokeResponse.HttpStatusCode}]{errorResponse?.ErrorMessage ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            throw new LambdaOperationException($"Failed to invoke Lambda function '{functionName}': {ex.Message}", ex);
        }
    }

    private void ValidateCoordinates(string coordinates)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(coordinates))
            {
                throw new LambdaOperationException($"{nameof(coordinates)} cannot be null or empty.");
            }

            var reader = new WKTReader();
            var geometry = reader.Read(coordinates);
            if (geometry is null || !geometry.IsValid)
            {
                throw new LambdaOperationException($"{nameof(coordinates)} is not a valid geometry: {coordinates}");
            }
        }
        catch (Exception ex)
        {
            throw new LambdaOperationException($"Failed to parse {nameof(coordinates)}: {ex.Message}", ex);
        }
    }
}
