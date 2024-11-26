namespace TestAPI
{
    /// <summary>
    /// Запрос. Каждый запрос сохраняем в БД для возможности возврата к выполнению при прерывании работы приложения
    /// </summary>
    public class Query
    {
        //public int Id { get; set; }
        public Guid Id { get; set; }
        public Guid UserGuid { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        ///<value>Процент выполнения запроса</value>
        public int Progress { get; set; }
        ///<value>Число входов за выбранный период</value>
        public int SignInCount { get; set; }
    }
    /// <summary>
    /// Объект передачи данных для запроса
    /// </summary>
    public class QueryDto
    {
        public Guid QueryID { get; set; }
        public int Progress {  set; get; }
        public QueryResultDto? Result { get; set; }
    }
    /// <summary>
    /// Объект передачи данных для результата запроса
    /// </summary>
    public class QueryResultDto
    { 
        public Guid UserID { get; set; }
        public int SignInCount {  set; get; }
    }
}
