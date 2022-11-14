var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSubscriptionServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapSubscriptionsEndpoints();


app.UseSwagger();
app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Subscription Service V1");
      c.RoutePrefix = "";
    });
app.UseRouting();


app.Run();

public partial class Program { }

