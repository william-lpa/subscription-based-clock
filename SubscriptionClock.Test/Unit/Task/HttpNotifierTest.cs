using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Xunit.Abstractions;

namespace SubscriptionClock.UnitTests.TaskSchedulerTest
{
  public class HttpNotifierTest
  {
    private readonly ITestOutputHelper output;

    public HttpNotifierTest(ITestOutputHelper output)
    {
      this.output = output;
    }

    private Mock<HttpMessageHandler> MockRequestHandler(HttpResponseMessage fakeMessage, Action<HttpRequestMessage, CancellationToken> reqInterceptor)
    {
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

      handlerMock
         .Protected()
         // Setup the PROTECTED method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
         )
         .Callback<HttpRequestMessage, CancellationToken>(reqInterceptor)
         // prepare the expected response of the mocked http call
         .ReturnsAsync(fakeMessage)
         .Verifiable();

      return (handlerMock);
    }

    private void HasHttpClientBeenCalled(Mock<HttpMessageHandler> httpMock, int times, Uri address)
    {
      httpMock.Protected().Verify(
         "SendAsync",
         Times.Exactly(times), // we expected a single external request
         ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Post  // we expected a POST request
            && req.RequestUri == address // to this uri                    
         ),
         ItExpr.IsAny<CancellationToken>()
      );
    }

    [Fact]
    public async Task Makes_a_post_request_to_subscriber()
    {
      //arrange
      var reqBody = "";
      Action<HttpRequestMessage, CancellationToken> reqInterceptor = (httpRequestMessage, cancellationToken) =>
      {
        // Read the Content here before it's disposed
        reqBody = httpRequestMessage.Content
            .ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

      };
      var handlerMock = MockRequestHandler(new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("Hello sir!"),
      }, reqInterceptor);

      var httpClient = new HttpClient(handlerMock.Object);

      var date = new DateTime(2002, 01, 01);
      var subjectUnderTest = new HttpNotifier(httpClient, new TestLogger(this.output));

      //act
      await subjectUnderTest.Notify("http://test.com/api/test/whatever", date);

      //assert
      // also check the 'http' call was like we expected it
      var expectedUri = new Uri("http://test.com/api/test/whatever");
      //making a POST with the current date
      HasHttpClientBeenCalled(handlerMock, 1, expectedUri);
      Assert.Equal("{\"notificationTime\":\"2002-01-01T00:00:00\"}", reqBody);
    }

    [Fact]
    public async Task Should_not_throw_if_500_when_posting_request_to_subscriber()
    {
      // arrange
      var reqBody = "";
      Action<HttpRequestMessage, CancellationToken> reqInterceptor = (httpRequestMessage, cancellationToken) =>
      {
        // Read the Content here before it's disposed
        reqBody = httpRequestMessage.Content
            .ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

      };
      var handlerMock = MockRequestHandler(new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("Internal Server Error"),
      }, reqInterceptor);

      var httpClient = new HttpClient(handlerMock.Object);

      var date = new DateTime(2002, 01, 01);
      var subjectUnderTest = new HttpNotifier(httpClient, new TestLogger(this.output));

      //act
      await subjectUnderTest.Notify("http://test.com/api/willFail", date);

      // assert
      var expectedUri = new Uri("http://test.com/api/willFail");
      string json = JsonSerializer.Serialize(new { notificationTime = date });

      var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

      //making a POST with the current date
      HasHttpClientBeenCalled(handlerMock, 1, expectedUri);
      Assert.Equal("{\"notificationTime\":\"2002-01-01T00:00:00\"}", reqBody);
    }

  }
}