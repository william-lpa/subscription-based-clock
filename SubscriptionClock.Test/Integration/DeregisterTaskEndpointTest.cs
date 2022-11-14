using System.Collections;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace SubscriptionClock.Integration
{

  // This is an integration test-ish/unit test using black box approach to interact with the POST /v1/deregister API
  // All external depedencies are mocked, using real classes otherwise.
  public class DeregisterTaskEndpointTest : IClassFixture<ApiWebApplicationFactory>
  {
    private readonly ITestOutputHelper output;
    private readonly HttpClient client;

    public DeregisterTaskEndpointTest(ITestOutputHelper output, ApiWebApplicationFactory application)
    {
      this.output = output;
      this.client = application.CreateClient();
    }

    // yields multiple scenarios which one can get BadRequest.
    public class BadRequestTestData : IEnumerable<object[]>
    {
      public IEnumerator<object[]> GetEnumerator()
      {
        yield return new object[] { null, new BadRequestResponse { Error = "No body was provided", ErrorCode = ErrorCode.NoBody } };
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Theory]
    [ClassData(typeof(BadRequestTestData))]
    public async Task POST_no_body(DeleteSubscriptionConfigDto reqBody, BadRequestResponse expectedBody)
    {
      var response = await this.client.PostAsJsonAsync<DeleteSubscriptionConfigDto>("/v1/deregister", reqBody);

      var body = await response.Content.ReadFromJsonAsync<BadRequestResponse>();

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      body.Should().BeEquivalentTo(expectedBody);
    }



    [Fact]
    public async Task Post_invalid_url()
    {
      var response = await this.client.PostAsJsonAsync<DeleteSubscriptionConfigDto>("/v1/deregister",
       new DeleteSubscriptionConfigDto
       {
         CallbackURL = "invalid-valid-URL",
       });

      var body = await response.Content.ReadFromJsonAsync<BadRequestResponse>();

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      body.Should().BeEquivalentTo(new BadRequestResponse
      {
        Error = "Provided URL is not in a valid format",
        ErrorCode = ErrorCode.InvalidURLFormat
      });
    }

    [Fact]
    public async Task POST_unsubscribe_a_valid_client()
    {

      var reResponse = await this.client.PostAsJsonAsync<SubscriptionConfigDto>("/v1/register",
       new SubscriptionConfigDto
       {
         Interval = 2,
         CallbackURL = "http://localhost:5000",
         IntervalUnit = IntervalUnit.Minutes
       });

      Assert.Equal(HttpStatusCode.Accepted, reResponse.StatusCode);

      var deResponse = await this.client.PostAsJsonAsync<DeleteSubscriptionConfigDto>("/v1/deregister",
      new DeleteSubscriptionConfigDto { CallbackURL = "http://localhost:5000" });

      Assert.Equal(HttpStatusCode.Accepted, deResponse.StatusCode);
    }

    [Fact]
    public async Task POST_unsubscribe_a_non_existing_client()
    {
      var deResponse = await this.client.PostAsJsonAsync<DeleteSubscriptionConfigDto>("/v1/deregister",
      new DeleteSubscriptionConfigDto { CallbackURL = "http://localhost:5000" });

      Assert.Equal(HttpStatusCode.NotFound, deResponse.StatusCode);
    }

  }
}