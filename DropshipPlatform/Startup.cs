using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DropshipPlatform.Startup))]
namespace DropshipPlatform
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
