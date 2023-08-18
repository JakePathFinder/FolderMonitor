namespace Common.Services
{
    public class UtilitiesService : IUtilitiesService
    {
        public bool IsValidFolder(string folder)
        {
            return !string.IsNullOrEmpty(folder) && Directory.Exists(folder);
        }
    }
}
