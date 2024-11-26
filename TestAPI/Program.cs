using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TestAPI;
using TestAPI.Controllers;
using TestAPI.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ef => ef.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    ));
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings")); //Подключение настроек из конфигурационного файла

//Добавляем сервисы
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<QueryService>();

var app = builder.Build();

//Восстанавливаем обработку запросов при перезапуске приложения
//Отдельным процессом
_ = Task.Run(() => RestoreProcessingTasks(app.Services));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Поиск незавершённых запросов и запуск их параллельного выполнения
/// </summary>
async Task RestoreProcessingTasks(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();        
        var unfinishedQueries = await context.Queries        
        .Where(q => q.Progress < 100)
        .Select(q => q.Id)
        .ToListAsync();
        var tasks = unfinishedQueries.Select(id => Task.Run(() => ContinueProcessing(services, id)));
        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Выполнение отдельного незавершённого запроса
/// Контекст, используемый в RestoreProcessingTasks может быть освобожден и внутри этой функции недоступен,
/// Поэтому придётся ещё раз создавать scope и контекст, а за одно и извлечь настройки
/// В осталном - функция повторяет QueryService.Process
/// </summary>
async Task ContinueProcessing(IServiceProvider services, Guid queryGuid)
{
    
    using (var scope = services.CreateAsyncScope()) 
    {        
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>().Value;

        //Тянем время
        TestAPI.Query? query = await context.Queries.FindAsync(queryGuid);
        if (query == null)
        {
            throw new Exception($"Query with GUID {queryGuid} not found.");
        }
        int start = query.Progress * settings.TimeOut / 100;
        for (int i = start; i <= settings.TimeOut; i += settings.TimeStep)
        {
            await Task.Delay(settings.TimeStep);
            query.Progress = (i * 100) / settings.TimeOut;
            context.Queries.Update(query);
            await context.SaveChangesAsync();
        }
        //Вычисляем число входов за время
        Task<int> signInCount = context.SignIns.Select(
            x => (x.UserId == query.UserGuid &&
            x.SignInDate >= query.DateFrom &&
            x.SignInDate <= query.DateTo)).CountAsync();
        query.SignInCount = signInCount.Result;
        context.Queries.Update(query);
        await context.SaveChangesAsync();
    }
}