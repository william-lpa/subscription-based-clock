using System.Collections;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace SubscriptionClock.Integration
{
  // This is an integration test-ish/unit test using black box approach to interact with the PUT /v1/register API
  // All external depedencies are mocked, using real classes otherwise.
  public class UpdateFrequencyTaskEndpointTest : IClassFixture<ApiWebApplicationFactory>
  {
    private readonly ITestOutputHelper output;
    private readonly HttpClient client;

    public UpdateFrequencyTaskEndpointTest(ITestOutputHelper output, ApiWebApplicationFactory application)
    {
      this.output = output;
      this.client = application.CreateClient();
    }

    // yields multiple scenarios which one can get BadRequest
    public class BadRequestTestData : IEnumerable<object[]>
    {
      public IEnumerator<object[]> GetEnumerator()
      {
        yield return new object[] { null, new BadRequestResponse { Error = "No body was provided", ErrorCode = ErrorCode.NoBody } };
        yield return new object[] { new SubscriptionConfigDto { Interval = 2, IntervalUnit = IntervalUnit.Minutes },
                                        new BadRequestResponse { Error = "Invalid body format", ErrorCode = ErrorCode.InvalidRequestBody }
                                      };
        yield return new object[] { new SubscriptionConfigDto { CallbackURL="http://url.com", IntervalUnit = IntervalUnit.Minutes },
                                        new BadRequestResponse { Error = "Invalid body format", ErrorCode = ErrorCode.InvalidRequestBody }
                                      };
        yield return new object[] { new SubscriptionConfigDto { Interval=2, CallbackURL="http://url.com", IntervalUnit =(IntervalUnit)(-3) },
                                        new BadRequestResponse { Error = "Invalid body format", ErrorCode = ErrorCode.InvalidRequestBody }
                                      };
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Theory]
    [ClassData(typeof(BadRequestTestData))]
    public async Task PUT_invalid_data(SubscriptionConfigDto reqBody, BadRequestResponse expectedBody)
    {
      var response = await this.client.PutAsJsonAsync<SubscriptionConfigDto>("/v1/register", reqBody);

      var body = await response.Content.ReadFromJsonAsync<BadRequestResponse>();

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      body.Should().BeEquivalentTo(expectedBody);
    }

    [Theory]
    // min is 5 sec
    [InlineData(4, IntervalUnit.Seconds)]
    // 4h and 1 sec
    [InlineData(14401, IntervalUnit.Seconds)]
    // 4h and 1m
    [InlineData(241, IntervalUnit.Minutes)]
    // max is 4 h
    [InlineData(5, IntervalUnit.Hours)]
    public async Task PUT_out_of_bounds_intervals(int interval, IntervalUnit reqInterval)
    {
      var response = await this.client.PutAsJsonAsync<SubscriptionConfigDto>("/v1/register",
       new SubscriptionConfigDto
       {
         Interval = interval,
         CallbackURL = "http://url.com",
         IntervalUnit = reqInterval
       });

      var body = await response.Content.ReadFromJsonAsync<BadRequestResponse>();

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      body.Should().BeEquivalentTo(new BadRequestResponse
      {
        Error = "Frequency should be between 5 seconds and 4 hours",
        ErrorCode = ErrorCode.InvalidFrequency
      });
    }

    [Fact]
    public async Task PUT_invalid_URL()
    {
      var response = await this.client.PutAsJsonAsync<SubscriptionConfigDto>("/v1/register",
       new SubscriptionConfigDto
       {
         Interval = 2,
         CallbackURL = "not-a-valid-URL",
         IntervalUnit = IntervalUnit.Minutes
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
    public async Task PUT_unsubscribe_a_valid_client()
    {

      var reResponse = await this.client.PostAsJsonAsync<SubscriptionConfigDto>("/v1/register",
       new SubscriptionConfigDto
       {
         Interval = 2,
         CallbackURL = "http://localhost:5000",
         IntervalUnit = IntervalUnit.Minutes
       });

      Assert.Equal(HttpStatusCode.Accepted, reResponse.StatusCode);

      var deResponse = await this.client.PutAsJsonAsync<SubscriptionConfigDto>("/v1/register",
      new SubscriptionConfigDto
      {
        Interval = 4,
        CallbackURL = "http://localhost:5000",
        IntervalUnit = IntervalUnit.Minutes
      });

      Assert.Equal(HttpStatusCode.Accepted, deResponse.StatusCode);
    }

    [Fact]
    public async Task PUT_not_found_client_URL()
    {
      var response = await this.client.PutAsJsonAsync<SubscriptionConfigDto>("/v1/register",
       new SubscriptionConfigDto
       {
         Interval = 2,
         CallbackURL = "http://localhost:500",
         IntervalUnit = IntervalUnit.Minutes
       });

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}