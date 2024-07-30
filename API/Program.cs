using Microsoft.EntityFrameworkCore;
using API.Context;
using API.Repositories;
using API.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "SoftGED",
        policy =>
        {
            policy
                .WithOrigins("*", "*")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(origin => true)
                .AllowCredentials();
        });
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "10mOyIm3S1WMbwaCE7";
    })
    .AddCookie("10mOyIm3S1WMbwaCE7", options =>
    {
        options.Cookie.Name = "iji87ZZD32rh";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS
        options.Cookie.HttpOnly = true;
    })
    .AddCookie("jfien434YUGfbjjr94", options =>
    {
        options.Cookie.Name = "83nkkr43GR32";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Standard Authorization Bearer (\"bearer {token}\"",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ValidationsHistoryRepository>();
builder.Services.AddScoped<AttachementRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<DocumentRepository>();
builder.Services.AddScoped<DocumentsProcessesRepository>();
builder.Services.AddScoped<DynamicFieldRepository>();
builder.Services.AddScoped<SupplierRepository>();
builder.Services.AddScoped<ProjectDocumentsReceiverRepository>();
builder.Services.AddScoped<TomProConnectionRepository>();
builder.Services.AddScoped<UsersConnectionsHistoryRepository>();
builder.Services.AddScoped<DocumentTypeRepository>();
builder.Services.AddScoped<SiteRepository>();
builder.Services.AddScoped<DocumentAccessesRepository>();
builder.Services.AddScoped<SignatureRepository>();

builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<UserDocumentService>();
builder.Services.AddScoped<MailService>();

builder.Services.AddDbContext<SoftGED_DBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SoftGED_DBContext"),
    sqlServerOptionsAction: sqlOption =>
    {
        sqlOption.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
    });

    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)));
});

var webSocketOptions = new WebSocketOptions
{
    // KeepAliveInterval = TimeSpan.FromMinutes(2)
};
webSocketOptions.AllowedOrigins.Add(builder.Configuration.GetValue<string>("ClientDomain")!);

var app = builder.Build();

app.UseCors("SoftGED");

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

// app.UseHttpsRedirection(); // HTTPS

app.UseAuthentication();

app.UseAuthorization();

app.UseWebSockets(webSocketOptions);

app.MapControllers();

app.Run();
