using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using UnifiedPOS.API.Infrastructure;
using UnifiedPOS.Application;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Infrastructure;
using UnifiedPOS.Infrastructure.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options => options.Filters.Add(new ProducesAttribute("application/json")))
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
        {
            Title = "Request validation failed.", Status = StatusCodes.Status400BadRequest
        }));

builder.Services.AddOptions<JwtSettings>().Bind(builder.Configuration.GetSection("Jwt")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<PosSettings>().Bind(builder.Configuration.GetSection("Pos")).ValidateDataAnnotations().ValidateOnStart();
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        NameClaimType = JwtClaims.UserId, RoleClaimType = JwtClaims.Role, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ =>
            new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddFixedWindowLimiter("authentication", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddOptions<BootstrapOptions>().Bind(builder.Configuration.GetSection("Bootstrap"))
    .Validate(options => options.IsValid(), "Bootstrap requires organization name, uppercase three-letter currency, owner username, and a password of 12–128 characters.")
    .Validate(options => !options.Enabled || builder.Environment.IsDevelopment() || options.AllowProduction,
        "Bootstrap is restricted to Development unless Bootstrap:AllowProduction is enabled.")
    .ValidateOnStart();
builder.Services.AddScoped<OwnerProvisioner>();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BootstrapOptions>>().Value;
    await scope.ServiceProvider.GetRequiredService<OwnerProvisioner>().ProvisionAsync(options, app.Environment.IsDevelopment());
}
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();
app.MapOpenApi("/openapi/{documentName}.json");
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Unified POS API v1"));
}
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();
app.Run();

public partial class Program;
