using ClickViews_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// custom services
var dbService = new DbService(builder.Configuration);
builder.Services.AddSingleton(dbService);

var userService = new UserService(dbService);
builder.Services.AddSingleton(userService);

// read token expiration from configuration
var config = builder.Configuration.GetSection("BearerToken");
TimeSpan expireHours = config["ExpireHours"] != null ?
    TimeSpan.FromHours(Convert.ToDouble(config["ExpireHours"])) : TimeSpan.FromHours(24);

// add authentication and authorization
builder.Services.AddAuthentication().AddBearerToken(config =>
{
    config.BearerTokenExpiration = expireHours;
    config.ClaimsIssuer = "ClickViews_API";
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapControllers();
app.Run();
