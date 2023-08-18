namespace Common.Cfg
{
    public class CommonConfig
    {
        public required SwaggerConfig SwaggerConfig { get; init; }
        public required ConnectionStrings ConnectionStrings { get; init; }
    }

    public class ConnectionStrings
    {
        public required string Redis { get; init; }
        public required string RabbitMq { get; set; }
    }
    public class SwaggerConfig
    {
        public required string Title { get; init; }
        public required string Version { get; init; }
    }

    public class Log4Net
    {
        public required string ConfigFile { get; init; }
        public required string ConfigType { get; init; }
    }
}
