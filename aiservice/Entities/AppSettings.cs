using System;
using System.Collections.Generic;
using System.Text;

namespace AIService.Entities
{
    public class AppSettings
    {
        public string SymmetricKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Environment { get; set; }
        public string CDNService { get; set; }
        public List<Environments> Environments { get; set; }
        public LogSettings LogSettings { get; set; }
        public List<LogSettings> LogPaths { get; set; }
        public Dictionary<string, string> Queries { get; set; }
        public WatsonServices WatsonServices { get; set; }
        public List<CDNServices> CDNServices { get; set; }
        public List<User> Users { get; set; }
        public SmoochSettings SmoochSettings { get; set; }
        public CulqiSettings CulqiSettings { get; set; }
        public PrestashopSettings PrestashopSettings { get; set; }
        public PCloudSettings PCloudSettings { get; set; }
        public TwitterSettings TwitterSettings { get; set; }
    }

    public class Environments
    {
        public string Label { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public ExternalPlatforms ExternalPlatforms { get; set; }
        public string FtpAttachment { get; set; }
    }

    public class ConnectionStrings
    {
        public string PostgresSQL { get; set; }
    }

    public class ExternalPlatforms
    {
        public string App { get; set; }
        public string Api { get; set; }
        public string STTAudioFilePath { get; set; }
        public string STTAudioFileUrl { get; set; }
        public string AudioToWavUrl { get; set; }
    }

    public class LogSettings
    {
        public bool Debug { get; set; }
        public bool Info { get; set; }
        public bool Error { get; set; }
        public string Label { get; set; }
        public LogPaths Parameters { get; set; }
    }
    public class LogPaths
    {
        public bool Enabled { get; set; }
        public string Path { get; set; }
    }

    public enum LogEnum
    {
        DEBUG,
        INFO,
        ERROR
    }

    public class WatsonServices
    {
        public WatsonSettings Discovery { get; set; }
        public WatsonSettings LanguageTranslator { get; set; }
        public WatsonSettings NaturalLanguageClassifier { get; set; }
        public WatsonSettings NaturalLanguageUnderstanding { get; set; }
        public WatsonSettings SpeechToText { get; set; }
        public WatsonSettings TextToSpeech { get; set; }
        public ToneAnalyzer ToneAnalyzer { get; set; }
        public WatsonAssistant WatsonAssistant { get; set; }
    }

    public class WatsonSettings
    {
        public bool IsActive { get; set; }
        public string Cliente { get; set; }
        public string Organization { get; set; }
        public string Space { get; set; }
        public string ApiKey { get; set; }
        public string ApiUrlBase { get; set; }
        public string Version { get; set; }
    }

    public class ToneAnalyzer : WatsonSettings
    {
        public string ContentLanguage { get; set; }
        public string AcceptLanguage { get; set; }
        public bool Sentences { get; set; }
    }

    public class WatsonAssistant : WatsonSettings
    {
        public string ConversationServiceName { get; set; }
        public string WorkspaceName { get; set; }
        public string WorkspaceId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string WatsonApiUrlBase { get; set; }
    }

    public class CDNServices
    {
        public Uploadcare Uploadcare { get; set; }
        public Cloudinary Cloudinary { get; set; }
    }

    public class Uploadcare
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
    public class Cloudinary
    {
        public string CloudName { get; set; }
        public string APIKey { get; set; }
        public string APISecret { get; set; }
        public string EnvironmetVariable { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SmoochSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CulqiSettings
    {
        public string UrlApiBase { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
    }

    public class PrestashopSettings
    {
        public string UrlApiBase { get; set; }
        public string ApiKey { get; set; }
        public string LoginUrl { get; set; }
    }

    public class PCloudSettings
    {
        public string UrlApiBase { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class TwitterSettings
    {
        public string UrlApiBase { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string BearerToken { get; set; }
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
    }
}
