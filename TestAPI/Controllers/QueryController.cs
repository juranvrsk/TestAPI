using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using TestAPI.Services;

namespace TestAPI.Controllers
{
    [Route("report/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly QueryService _queryService;
        public InfoController(QueryService queryService)
        {
            _queryService = queryService;
        }
        /// <summary>
        /// Результат запроса
        /// </summary>
        /// <param name="queryGuid">GUID запроса</param>
        /// <returns>Объект передачи данных с результатом запроса</returns>
        [HttpGet(Name = "info")]
        public async Task<IActionResult> GetInfo(Guid queryGuid)
        {
            var result = await _queryService.GetResultAsync(queryGuid);
            return Ok(result);
        }
    }

    [Route("report/[controller]")]
    [ApiController]
    public class UserStatisticsController : ControllerBase
    {
        private readonly QueryService _queryService;

        public UserStatisticsController(QueryService queryService)
        {
            _queryService = queryService;
        }

        /// <summary>
        /// Запрос статистики пользователя
        /// </summary>
        /// <param name="userGuid">GUID пользователя</param>
        /// <param name="datefrom">Дата с </param>
        /// <param name="dateto">Дата по</param>
        /// <returns>GUID запроса</returns>
        [HttpPost(Name = "user_statistics")]
        public Guid Post(Guid userGuid, DateOnly datefrom, DateOnly dateto)
        {
            Guid queryGuid = _queryService.StartOperation(userGuid, datefrom, dateto);
            return queryGuid;
        }
    }
}
