namespace SharePointPagesTranslation
{
    public static class Constants
    {
        #region General application settings
        public static readonly string TenantURL = "TenantURL";
        public static readonly string TenantID = "TenantID";
        public static readonly string AADReaderClientID = "AADReaderClientID";
        public const string EnvironmentBlobStorageConnectionString = "EnvironmentBlobStorageConnectionString";
        #endregion

        #region AI Application Settings
        public static readonly string AITranslatorKey = "AITranslatorKey";
        public static readonly string AIEndpoint = "AITranslatorEndpoint";
        public static readonly string AIResourcelocation = "AIResourcelocation";
        public static readonly string AITranslateService = "translate";
        public static readonly string AIDetectService = "detect";
        public static readonly string AIAPIVersion = "AIAPIVersion";
        #endregion

        #region Site pages library field names
        public static readonly string IDFieldName = "ID";
        public static readonly string FileRefFieldName = "FileRef";
        public static readonly string FileLeafRefFieldName = "FileLeafRef";
        public static readonly string EditorFieldName = "Editor";
        public static readonly string ModifiedFieldName = "Modified";
        public static readonly string ApprovedFieldName = "Approved";
        public static readonly string AutomateTranslation = "AutomateTranslation";
        #endregion
    }
}
