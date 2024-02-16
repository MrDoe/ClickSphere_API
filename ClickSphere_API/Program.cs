using ClickSphere_API.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Define the BearerAuth security scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Bearer",
        BearerFormat = "JWT",
        Scheme = "bearer",
        Description = "Specify the bearer token: {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
    };
    c.AddSecurityDefinition("BearerAuth", securityScheme);

    // Apply the security scheme to all operations
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "BearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

// custom services
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<IUserService, UserService>();

// read token expiration from configuration
var config = builder.Configuration.GetSection("BearerToken");

TimeSpan expireHours;
if (config["ExpireHours"] != null)
    expireHours = TimeSpan.FromHours(Convert.ToDouble(config["ExpireHours"]));
else
    expireHours = TimeSpan.FromHours(24);

// add authentication and authorization
builder.Services.AddAuthentication().AddBearerToken(config =>
{
    config.BearerTokenExpiration = expireHours;
    config.ClaimsIssuer = "ClickSphere_API";
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClickSphere API V1");
    });
}

//app.UseHttpsRedirection();

app.MapControllers();
app.Run();
