using System.IO.Compression;
using ClickSphere_API.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ClickSphere API", Version = "v1" });
    var filePath = Path.Combine(AppContext.BaseDirectory, "ClickSphere_API.xml");
    c.IncludeXmlComments(filePath);

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
builder.Services.AddScoped<IApiUserService, ApiUserService>();
builder.Services.AddScoped<IApiRoleService, ApiRoleService>();
builder.Services.AddScoped<IApiViewService, ApiViewServices>();
builder.Services.AddScoped<ISqlParser, SqlParser>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IRagService, RagService>();

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
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.WriteIndented = false;
});

// enable compression for Kestrel
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});

builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
           policy =>
            {
               policy.AllowAnyOrigin()
                     .AllowAnyHeader()
                       .AllowAnyMethod();
            });
    });

var app = builder.Build();
app.UseCors();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClickSphere API V1");
        c.EnableTryItOutByDefault();
    });
}

// enable cache-control for static files
app.UseStaticFiles(new StaticFileOptions()
{
  OnPrepareResponse =
  r => {
    if(r == null || r.File == null || r.File.PhysicalPath == null)
        return;
    
    string path = r.File.PhysicalPath;
    if (path.EndsWith(".css") || path.EndsWith(".js") || 
    path.EndsWith(".gif") || path.EndsWith(".jpg") || 
    path.EndsWith(".png") || path.EndsWith(".svg") || path.EndsWith(".webp"))
    {
      TimeSpan maxAge = new TimeSpan(370, 0, 0, 0);
      r.Context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
    }
  },
});

//app.UseHttpsRedirection();

app.MapControllers();
app.Run();
