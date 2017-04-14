using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HollandPub.Admin.Startup))]
namespace HollandPub.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
