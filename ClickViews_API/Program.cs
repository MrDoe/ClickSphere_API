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

var userService = new UserService(builder.Configuration, dbService);
builder.Services.AddSingleton(userService);

builder.Services.AddAuthentication().AddBearerToken();
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
