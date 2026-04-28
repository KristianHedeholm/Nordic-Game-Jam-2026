namespace RawPowerLabs.DynamicAI
{
    public sealed class CloudTextModuleOptions
    {
        public string AifactoryUrl { get; set; } = string.Empty;
        public string DesignmoduleUrl { get; set; } = string.Empty;
        public string ZitadelUrl { get; set; } = string.Empty;
        public string ZitadelProjectId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
