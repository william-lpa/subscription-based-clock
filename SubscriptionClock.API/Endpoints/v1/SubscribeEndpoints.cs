using System.Reflection;

public static class SubscribeEndpoints
{
  // This is where we map the controlers in C# using the new minimal API
  public static void MapSubscriptionsEndpoints(this WebApplication app)
  {
    app.MapPost("/v1/register", SubscribeTask)
         .WithName("RegisterTask")
         .WithTags("Subscriber")
         .Produces(StatusCodes.Status400BadRequest)
         .Produces(StatusCodes.Status202Accepted);

    app.MapPut("/v1/register", UpdateTaskFrequency)
         .WithName("UpdateTask")
         .WithTags("Subscriber")
         .Produces(StatusCodes.Status404NotFound)
         .Produces(StatusCodes.Status400BadRequest)
         .Produces(StatusCodes.Status202Accepted);

    app.MapPost("/v1/deregister", UnSubscribeTask)
    .WithName("DeregisterTask")
    .WithTags("Subscriber")
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status202Accepted);
  }

  // This is where we register services for DI in C# using the new minimal API
  public static void AddSubscriptionServices(this IServiceCollection service)
  {
    // register db as singleton
    service.AddSingleton<IDataSource, InMemoryDataSource>();
    service.AddScoped<ILogger, Logger>();
    service.AddHttpClient<INotifier, HttpNotifier>();
    service.AddTransient<ITaskScheduler, TaskScheduler>();
    service.AddTransient<IHttpSubscriptionService, HttpSubscriptionService>();

    //Registering Swagger middleware to easily see the api docs
    service.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1",
            new Microsoft.OpenApi.Models.OpenApiInfo
            {
              Title = "Clock Subscription service",
              Description = @"This is a code challenge. This service allows you to subscribe a specific url to start receiving notifications in a also given frequency. <br>
              Please notice that the same full qualified url can subscribe to this service just once <br><br>
              **Valid different URLs:** <br>
              <ul>
                <li>http://httpstat.us/200?sleep=5000</li>
                <li>http://httpstat.us/200?sleep=3000</li>
                <li>http://ptsv2.com/t/i6neh-1668368893/post</li>
                <li>http://localhost:5000</li>
              </ul>               
              **Invalid repeated URLs:** <br>
              <ul>
                <li>http://httpstat.us/200</li>
                <li>http://httpstat.us/200</li>
              </ul>               
              **Invalid URLs:** <br>
              <ul>
                <li>httpstat.us/200</li>
                <li>localhost:5000</li>
              </ul>           
              **`intervalUnit` is an enum with values 0, 1, 2**<br><br>
              `0 = Seconds`<br>
              `1 = Minutes`<br>
              `2 = Hours`<br><br>
              **`interval` is a number that combined with `intervalUnit` sets the frequency of the scheduler.**<br><br>
              `Valid frequency has to be between 5 seconds and 4 hours`.<br><br>
              Requests not satifying this criterion will be rejected with 400. Also, if attempted to deregister a url that was never register, the request will also get rejected.<br><br> 
              When sucessfully subscribed a client, this service will start sending POST requests to the consumer with the following JSON body:
              `{""notificationTime"":""2022-11-13T19:49:21.6146749+00:00""}`
              ",

              Contact = new Microsoft.OpenApi.Models.OpenApiContact()
              {
                Name = "William Pegler",
                Url = new Uri("https://github.com/william-lpa"),
              },
              Version = "v1"
            }
        );
      });
  }

  internal static async Task<IResult> SubscribeTask(SubscriptionConfigDto? newSubs, ILogger logger, IHttpSubscriptionService subsService)
  {
    logger.WriteLine("Starting handler. received body: {0}", newSubs);

    var validation = Utilities.ValidateRequest(newSubs, logger);
    if (!validation.Valid) { return validation.Outcome; }

    logger.WriteLine("Saving task");
    var result = await subsService.SubscribeTaskAsync(newSubs);

    if (result == RequestOutcome.Conflict) { return Results.Conflict(); }

    logger.WriteLine("Resolving the request");
    return Results.Accepted();
  }

  internal static async Task<IResult> UpdateTaskFrequency(SubscriptionConfigDto? newSubs, ILogger logger, IHttpSubscriptionService subsService)
  {
    logger.WriteLine("Starting handler. received body: {0}", newSubs);

    var validation = Utilities.ValidateRequest(newSubs, logger);
    if (!validation.Valid) { return validation.Outcome; }

    logger.WriteLine("Updating task");
    var result = await subsService.UpdateTaskAsync(newSubs);

    if (result == RequestOutcome.NonExisting) { return Results.NotFound(); }

    logger.WriteLine("Resolving the request");
    return Results.Accepted();
  }

  internal static async Task<IResult> UnSubscribeTask(DeleteSubscriptionConfigDto? subs, ILogger logger, IHttpSubscriptionService subsService)
  {
    logger.WriteLine("Starting handler. received body: {0}", subs);

    if (subs is null)
    {
      return Results.BadRequest(new BadRequestResponse
      {
        ErrorCode = ErrorCode.NoBody,
        Error = "No body was provided"
      });
    }

    logger.WriteLine("Validating callbackurl format");

    if (!Utilities.ValidURI(subs.CallbackURL))
    {
      return Results.BadRequest(new BadRequestResponse
      {
        Error = "Provided URL is not in a valid format",
        ErrorCode = ErrorCode.InvalidURLFormat
      });
    }

    logger.WriteLine("Saving actions");
    var result = await subsService.UnScribeTaskAsync(subs);

    if (result == RequestOutcome.NonExisting) { return Results.NotFound(); }

    logger.WriteLine("Resolving the request");
    return Results.Accepted();
  }
}