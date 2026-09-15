public static class APIConfig
{
    public static string BaseUrl
    {
        get
        {
#if DEV
            return "https://devserver.spatialgrid.ai/api/v1/";
#elif RND
            return "https://devserver.spatialgrid.ai/api/v1/";
#else
            return "https://devserver.spatialgrid.ai/api/v1/";
#endif
        }
    }
}
