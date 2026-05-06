using Dapper;
using Npgsql;
using hikingRepository.Repositories;
using hikingService;
using hikingService.Options;
using hikingService.Services;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowLocalhost", policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var supabaseOptions = builder.Configuration
    .GetSection("Supabase")
    .Get<SupabaseOptions>()!;

builder.Services.AddSingleton(supabaseOptions);
builder.Services.AddHttpClient<StorageService>();
builder.Services.AddScoped<GpxService>();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<TagRepository>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<PhotoRepository>();
builder.Services.AddScoped<PhotoService>();

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(supabaseOptions.ConnectionString)
);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", async (NpgsqlDataSource db) =>
{
    await using var conn = await db.OpenConnectionAsync();
    await using var cmd  = conn.CreateCommand();
    cmd.CommandText = "SELECT 1";
    var result = await cmd.ExecuteScalarAsync();
    return Results.Ok(new { connected = true, result });
});

app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.MapControllers(); 
app.Run();