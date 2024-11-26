using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestAPI.Services
{
    /// <summary>
    /// Асинхронная обработка запросов
    /// </summary>
    public class QueryService
    {
        //Используем ContextFactory для работы с контекстом в других потоках
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly Settings _settings;
        public QueryService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IOptions<Settings> settings)
        {
            _dbContextFactory = dbContextFactory;
            _settings = settings.Value;
        }
        /// <summary>
        /// Метод, предназначенный для выполнения в отдельном потоке. Тянет время и подсчитывает число входов для пользовательской статистки
        /// </summary>
        public async Task Process(Guid queryGuid)
        {
            using (ApplicationDbContext _context = _dbContextFactory.CreateDbContext())
            {
                Query? query = await _context.Queries.FindAsync(queryGuid);
                if (query == null)
                {
                    throw new Exception($"Query with GUID {queryGuid} not found.");
                }

                //Тянем время
                int start = query.Progress * _settings.TimeOut / 100;
                for (int i = start; i <= _settings.TimeOut; i += _settings.TimeStep)
                {
                    await Task.Delay(_settings.TimeStep);
                    query.Progress = (i * 100) / _settings.TimeOut;
                    _context.Queries.Update(query);
                    await _context.SaveChangesAsync();
                }

                //Вычисляем число входов за время
                Task<int> signInCount = _context.SignIns.Select(
                    x => (x.UserId == query.UserGuid &&
                    x.SignInDate >= query.DateFrom &&
                    x.SignInDate <= query.DateTo)).CountAsync();
                query.SignInCount = signInCount.Result;
                _context.Queries.Update(query);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Создание запроса и запуск асинхронной обработки
        /// </summary>        
        public Guid StartOperation(Guid userGuid, DateOnly dateFrom, DateOnly dateTo) 
        {
            using (ApplicationDbContext _context = _dbContextFactory.CreateDbContext())
            {
                Query query = new Query
                {
                    UserGuid = userGuid,
                    Id = Guid.NewGuid(),
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Progress = 0
                };
                _context.Queries.Add(query);
                _context.SaveChanges();
                Task.Run(() => Process(query.Id));
                return query.Id;
            }
        }

        /// <summary>
        /// Создание объекта передачи данных для результата обработки запроса
        /// </summary>        
        private static QueryResultDto? QueryResultToDTO(Query query)
        {
            if (query != null)
            {
                return new QueryResultDto
                {
                    UserID = query.UserGuid,
                    SignInCount = query.SignInCount
                };
            }
            return null;
        }

        /// <summary>
        /// Получение информации о запросе
        /// </summary>        
        public async Task<QueryDto?> GetResultAsync(Guid queryGuid)
        {
            using (ApplicationDbContext _context = _dbContextFactory.CreateDbContext())
            {
                //Query? query = _context.Statistics.FirstOrDefault(x => x.QueryID == queryGuid);                
                //Task<Query?> query = _context.Statistics.Include(q => q.Result).FirstOrDefaultAsync(x => x.QueryID == queryGuid);
                var query = _context.Queries.FindAsync(queryGuid);
                if (query.Result != null)
                {
                    return await Task.FromResult(
                        new QueryDto
                        {
                            QueryID = query.Result.Id,
                            Progress = query.Result.Progress,
                            Result = QueryResultToDTO(query.Result)
                        });
                }
                return await Task.FromResult<QueryDto?>(result: null);
            }
        }

    }
}
