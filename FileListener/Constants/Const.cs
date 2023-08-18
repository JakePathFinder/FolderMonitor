namespace FileListener.Constants
{
    public static class Const
    {
        public const string AppConfigCfgName = "AppConfig";
        public static readonly NotifyFilters NotifyFilters = NotifyFilters.CreationTime | NotifyFilters.LastWrite |
                                                             NotifyFilters.FileName | NotifyFilters.Attributes;

        public static class FileSysWatcher
        {
            public const int MinBufferSizeKb = 4;
            public const int MaxBufferSizeKb = 64;
            public const int BufferMultiplierKb = 4;
        }

    }
}
