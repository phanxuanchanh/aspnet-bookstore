using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CuaHangSach.Startup))]
namespace CuaHangSach
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
