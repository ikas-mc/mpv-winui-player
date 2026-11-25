namespace mpv_winui.Modules.Language
{
    public class LangInfo
    {
        public string Name { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public static LangInfo Create(string name, string fileName, string displayName, bool isDefault)
        {
            return new LangInfo
            {
                Name = name,
                FileName = fileName,
                DisplayName = displayName,
                IsDefault = isDefault
            };
        }
    }
}
