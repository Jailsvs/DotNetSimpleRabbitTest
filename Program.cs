using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using SimpleRabbitTest.Consumers;
using SimpleRabbitTest.Notification;
using SimpleRabbitTest.Options;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(); 
builder.Services.AddControllers();
builder.Services.AddHostedService<ProcessMessageConsumer>();
builder.Services.AddSingleton<INotificationService>(new NotificationUserConsoleService());

builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQConfig"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors(option => option.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.UseExceptionHandler(
              options =>
              {
                  options.Run(
                      async context =>
                      {
                          context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                          context.Response.ContentType = "application/json";
                          var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
                          if (null != exceptionObject)
                          {
                              var result = JsonConvert.SerializeObject(new { error = exceptionObject.Error.Message });
                              await context.Response.WriteAsync(result).ConfigureAwait(false);
                          }
                      });
              }
          );

app.Run();
