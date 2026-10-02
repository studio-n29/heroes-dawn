namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Fichier Remote Config (.rc) déployable par la fenêtre Deployment : une clé JSON « balance ».</summary>
    public static class RemoteConfigFile
    {
        public static string Build(string balanceJson)
        {
            var indented = balanceJson.Replace("\n", "\n    ");
            return "{\n" +
                   "  \"$schema\": \"https://ugs-config-schemas.unity3d.com/v1/remote-config.schema.json\",\n" +
                   "  \"entries\": {\n" +
                   "    \"balance\": " + indented + "\n" +
                   "  },\n" +
                   "  \"types\": {\n" +
                   "    \"balance\": \"JSON\"\n" +
                   "  }\n" +
                   "}\n";
        }
    }
}
