using System.IO;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class TestOrderHistory
{
    [Fact]
    public async Task Run_Returns_OkObjectResult_With_Name()
    {
        // Arrange
        var request = new Mock<HttpRequest>();
        var context = new DefaultHttpContext();
        request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"Azure\"}")));
        request.Setup(r => r.HttpContext).Returns(context);

        Microsoft.Extensions.Logging.ILogger logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

        // Act
        var response = await Function1.Run(request.Object, logger);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal("Hello, Azure", okResult.Value);
    }

    [Fact]
    public async Task Run_Returns_BadRequestObjectResult_When_Name_Is_Missing()
    {
        // Arrange
        var request = new Mock<HttpRequest>();
        var context = new DefaultHttpContext();
        request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("{}")));
        request.Setup(r => r.HttpContext).Returns(context);

        var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

        // Act
        var response = await Function1.Run(request.Object, logger);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response);
    }
}
