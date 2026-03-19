using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server.Services;
using PLATEAU.Snap.Server.Test.Fakes.Lambda;

namespace PLATEAU.Snap.Server.Test.Services;

public class InvokerServiceTest
{
    [Fact(DisplayName = "正射変換")]
    [Trait("Category", "Unit")]
    public async Task Transform()
    {
        var service = CreateService();

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var response = await service.TransformAsync(request);
        Assert.NotNull(response.Path);
        Assert.NotNull(response.Coordinates);
    }

    [Fact(DisplayName = "屋根面生成")]
    [Trait("Category", "Unit")]
    public async Task RoofExtraction()
    {
        var service = CreateService();

        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaRoofExtractionRequest
        {
            Geometry = geometry,
        };

        var response = await service.RoofExtractionAsync(request);
        Assert.NotNull(response.Path);
        Assert.NotNull(response.Coordinates);
    }

    [Fact(DisplayName = "テクスチャ更新")]
    [Trait("Category", "Unit")]
    public async Task ApplyTexture()
    {
        var service = CreateService();

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaApplyTextureRequest
        {
            Path = path,
            Coordinates = coordinates,
        };

        var response = await service.ApplyTextureAsync(request);
        Assert.NotNull(response.Path);
        Assert.NotNull(response.TextureCoordinates);
    }

    [Fact(DisplayName = "Lambda HttpStatusCodeが200以外")]
    [Trait("Category", "Unit")]
    public async Task LambdaNotValidHttpStatusCode()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsNotValidHttpStatusCode = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda StatusCodeが200以外")]
    [Trait("Category", "Unit")]
    public async Task LambdaNotValidStatusCode()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsNotValidStatusCode = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda Responseの型が不正")]
    [Trait("Category", "Unit")]
    public async Task LambdaNotValidResponse()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsNotValidResponse = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda ResponseのBodyの型が不正")]
    [Trait("Category", "Unit")]
    public async Task LambdaNotValidBody()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsNotValidBody = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda ResponseのCoordinatesが空")]
    [Trait("Category", "Unit")]
    public async Task LambdaEmptyCoordinates()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsEmptyCoordinates = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda ResponseのPOLYGONのパースに失敗")]
    [Trait("Category", "Unit")]
    public async Task LambdaWktParseFail()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsWktParseFailed = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    [Fact(DisplayName = "Lambda ResponseのCoordinatesが不正")]
    [Trait("Category", "Unit")]
    public async Task LambdaNotValidCoordinates()
    {
        var amazonLambda = new FakeAmazonLambda();
        amazonLambda.IsNotValidCoordinates = true;
        var service = CreateService(amazonLambda);

        const string path = "s3://1/2/3.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        const string geometry = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new LambdaTransformRequest
        {
            Path = path,
            Coordinates = coordinates,
            Geometry = geometry,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(LambdaOperationException), exception.GetType());
    }

    private static InvokerService CreateService()
    {
        var amazonLambda = new FakeAmazonLambda();
        var lambdaSettings = new LambdaSettings();
        lambdaSettings.TransformFunctionName = "transform";
        lambdaSettings.RoofExtractionFunctionName = "roof_extraction";
        lambdaSettings.ApplyTextureFunctionName = "apply_texture";

        return new InvokerService(amazonLambda, lambdaSettings);
    }

    private static InvokerService CreateService(FakeAmazonLambda amazonLambda)
    {
        var lambdaSettings = new LambdaSettings();
        lambdaSettings.TransformFunctionName = "transform";
        lambdaSettings.RoofExtractionFunctionName = "roof_extraction";
        lambdaSettings.ApplyTextureFunctionName = "apply_texture";

        return new InvokerService(amazonLambda, lambdaSettings);
    }
}
