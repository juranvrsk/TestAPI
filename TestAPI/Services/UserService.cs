namespace TestAPI.Services
{
    /// <summary>
    /// Вспомогательный класс для заполнения БД
    /// </summary>
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Создаём пользователя, генерируя GUID
        /// </summary>        
        public Guid CreateUser(string userName)
        {
            User user = new User
            { 
                //Id = Guid.NewGuid(),
                UserName = userName
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Guid.NewGuid();
        }

        /// <summary>
        /// Регистрируем пользователя в системе
        /// </summary>        
        public string SignInUser(string userName)
        {
            User user = _context.Users.First(x => x.UserName == userName);
            SignIn signIn = new SignIn
            { 
                UserId = user.Id,
                SignInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.SignIns.Add(signIn);
            _context.SaveChanges();
            return String.Format("User: {0} (GUID: {1}) was signed in at {2}", user.UserName, signIn.UserId, signIn.SignInDate);
        }
    }
}
