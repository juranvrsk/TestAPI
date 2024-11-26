using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestAPI.Services;

namespace TestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Создание пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>GUID пользователя</returns>
        [HttpPost(Name = "CreateUser")]
        public Guid CreateUser(string userName)
        {
            return _service.CreateUser(userName);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        private readonly UserService _service;

        public SignInController(UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Результат выполнения</returns>
        [HttpPost(Name = "SignIn")]
        public string SignIn(string userName)
        {
            return _service.SignInUser(userName);
        }
    }
}
