using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Senparc.CO2NET.AspNet;
using Senparc.Weixin;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.MP;
using Yiwan.Core;
using Senparc.Weixin.MP.Containers;
using Yiwan.Utilities.Cache;

namespace Qwf.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // 初始化
            Yiwan.Utilities.Distributed.IdGenerator.Init(1); // 初始化id生成器
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 设置允许跨域，因为前后端分开部署
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(AllowAllOrigins,
            //        builder => builder.AllowAnyOrigin()
            //            .AllowAnyHeader().WithMethods("GET", "POST", "HEAD")
            //    );
            //});
            #endregion

            #region 配置Controllers返回的Json格式,主要用于WebAPI
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            #endregion

            // Add CookieTempDataProvider after AddMvc and include ViewFeatures.
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            // 启用内存缓存
            services.AddMemoryCache();

            // 如果部署在linux系统上，需要加上下面的配置：
            //services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
            // 如果部署在IIS上，需要加上下面的配置：
            services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);

            // 暂时不需要
            //services.AddSignalR();//使用 SignalR

            GlobalContext.Services = services;
            GlobalContext.Configuration = Configuration;

            /*
             * CO2NET 是从 Senparc.Weixin 分离的底层公共基础模块，经过了长达 6 年的迭代优化，稳定可靠。
             * 关于 CO2NET 在所有项目中的通用设置可参考 CO2NET 的 Sample：
             * https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
             */
            services.AddSenparcWeixinServices(Configuration); // Senparc.Weixin 注册（必须）
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            // 启用请求回退
            //app.UseEnableRequestRewind();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            #region 注册Senparc微信

            // 启动 CO2NET 全局注册，必须！
            var registerService = app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister =>
            {
                #region CO2NET 全局配置

                // 当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
                globalRegister.ChangeDefaultCacheNamespace("QwfCache");

                //配置全局使用Redis缓存（按需，独立）
#if DEBUG
                var redisConfigurationStr = Configuration.GetSection("CacheDatabase:ConnectionStringDebug").Value;
#else
                var redisConfigurationStr = Configuration.GetSection("CacheDatabase:ConnectionString").Value;
#endif
                senparcSetting.Value.Cache_Redis_Configuration = redisConfigurationStr; // 必须的，否则会错误
                //Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);
                //以下会立即将全局缓存设置为 Redis
                //Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow(); // CsRedis库 键值对缓存策略（推荐）
                //Senparc.CO2NET.Cache.CsRedis.Register.UseHashRedisNow(); // CsRedis库 HashSet储存格式的缓存策略
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr); // StackExchange.Redis库
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); // 键值对缓存策略（推荐）

                #region 注册日志（按需，建议）

                // globalRegister.RegisterTraceLog(ConfigTraceLog);//配置TraceLog

                #endregion

                // APM 系统运行状态统计记录配置 默认已经为开启，如果需要关闭，则设置为 false
                Senparc.CO2NET.APM.Config.EnableAPM = false;

                #endregion
            }, true);
            // 使用 Senparc.Weixin SDK
            registerService.UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
            {
                #region 微信相关配置

                // 微信缓存（按需，必须放在配置开头，以确保其他可能依赖到缓存的注册过程使用正确的配置）
                // 注意：如果使用非本地缓存，而不执行本块注册代码，将会收到“当前扩展缓存策略没有进行注册”的异常
                // 微信的 Redis 缓存，如果不使用则注释掉（开启前必须保证配置有效，否则会抛错）
                weixinRegister.UseSenparcWeixinCacheRedis(); // StackExchange.Redis
                if (!AccessTokenContainer.CheckRegistered(Configuration.GetSection("WxConfig:AppId").Value))
                {
                    // 注册公众号（可注册多个）
                    weixinRegister.RegisterMpAccount(
                    Configuration.GetSection("WxConfig:AppId").Value,
                    Configuration.GetSection("WxConfig:Secret").Value,
                    "茵茵优选");
                }

                #endregion
            });
            #endregion

            app.UseAuthorization(); // 需要在注册微信 SDK 之后执行

            #region 初始化Redis配置
            GlobalData.EntityNamespace = "Qwf.Data.Entity";
#if DEBUG
            var redisConfigurationStr = GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionStringDebug").Value;
#else
            var redisConfigurationStr =  GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionString").Value;
#endif
            RedisConnectionHelper.Initialize(redisConfigurationStr, "QEBB:");
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "weixin_msg",
                    pattern: "Weixin/{appId?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                    name: "areas", "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
