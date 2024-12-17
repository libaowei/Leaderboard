using Leaderboard.Service.Customers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ICustomerLeaderboardService, CustomerLeaderboardService>();
var app = builder.Build();

#if DEBUG
// init seed data
var service = app.Services.GetRequiredService<ICustomerLeaderboardService>();
service.UpdateScore(15514665, 124);
service.UpdateScore(81546541, 113);
service.UpdateScore(1745431, 100);
service.UpdateScore(76786448, 100);
service.UpdateScore(254814111, 96);
service.UpdateScore(53274324, 95);
service.UpdateScore(6144320, 93);
service.UpdateScore(8009471, 93);
service.UpdateScore(11028481, 93);
service.UpdateScore(38819, 92);
#endif

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();