namespace NostrBot.Web.Utils
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configure configuration from the target section and return immediately.
        /// </summary>
        public static TConfiguration Configure<TConfiguration>(this IConfiguration configuration, IServiceCollection services, string sectionName)
            where TConfiguration : class, new()
        {
            var section = configuration.GetSection(sectionName);
            services.Configure<TConfiguration>(section);

            var settings = new TConfiguration();
            section.Bind(settings, o => o.BindNonPublicProperties = true);

            return settings;
        }
    }
}
