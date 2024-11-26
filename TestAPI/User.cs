namespace TestAPI
{
    /// <summary>
    /// В классе "Пользователь" предполагем, что каждый пользователь идентифицируется по GUID
    /// </summary>
    public class User 
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
    }

    /// <summary>
    /// Журнал регистрации входов пользователей
    /// </summary>
    public class SignIn
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateOnly SignInDate { get; set; }
    }
}
