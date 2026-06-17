using Amazon;
using Amazon.S3;
using Hellang.Middleware.ProblemDetails;
using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using LegalSystem.Exceptions;
using LegalSystem.Helpers;
using LegalSystem.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddCors(o =>
//{
//    o.AddPolicy("Dev", b =>
//    {
//        b.AllowAnyOrigin()
//         .AllowAnyHeader()
//         .AllowAnyMethod();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
       builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<CrmServiceOld>();
builder.Services.AddScoped<ReferancesService>();
builder.Services.AddScoped<FileStorageService>();

builder.Services.AddHttpClient<IdentityManagmentService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextProvider>();

builder.Services.AddScoped<CrmService>();
builder.Services.Configure<AWSConfiguration>(
    builder.Configuration.GetSection("AWS"));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var aws = sp.GetRequiredService<IOptions<AWSConfiguration>>().Value;

    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.USEast1
    };

    return new AmazonS3Client(aws.AccessKey, aws.SecretKey, config);
});
// --- ProblemDetails Configuration ---
builder.Services.AddProblemDetails(opt =>
{
    //opt.IncludeExceptionDetails = (ctx, ex) => !builder.Environment.IsProduction();
    opt.IncludeExceptionDetails = (ctx, ex) => true;
    opt.Map<CustomException>(exception => new ProblemDetails
    {
        Title = exception.Title,
        Status = exception.Status,
        Detail = exception.Details,
        Type = exception.Type
    });
});

builder.Services.Configure<FileStorageConfiguration>(builder.Configuration.GetSection(nameof(FileStorageConfiguration)));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));

// m-w
var app = builder.Build();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseRouting();
//app.UseCors("Dev");


app.UseAuthorization();

app.MapControllers();

app.Run();
