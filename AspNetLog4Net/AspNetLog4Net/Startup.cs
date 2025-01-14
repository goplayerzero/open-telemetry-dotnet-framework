using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AspNetLog4Net.Startup))]
namespace AspNetLog4Net
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
