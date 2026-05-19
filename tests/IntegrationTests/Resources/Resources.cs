namespace HypermediaEngine.IntegrationTests.Resources;

internal static class Resources
{
    [Before(Assembly)]
    public static void InitializeAssembly(AssemblyHookContext _)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", false);
    }
}
