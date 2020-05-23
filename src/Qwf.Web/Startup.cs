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
            // ��ʼ��
            Yiwan.Utilities.Distributed.IdGenerator.Init(1); // ��ʼ��id������
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region �������������Ϊǰ��˷ֿ�����
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(AllowAllOrigins,
            //        builder => builder.AllowAnyOrigin()
            //            .AllowAnyHeader().WithMethods("GET", "POST", "HEAD")
            //    );
            //});
            #endregion

            #region ����Controllers���ص�Json��ʽ,��Ҫ����WebAPI
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            #endregion

            // Add CookieTempDataProvider after AddMvc and include ViewFeatures.
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            // �����ڴ滺��
            services.AddMemoryCache();

            // ���������linuxϵͳ�ϣ���Ҫ������������ã�
            //services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
            // ���������IIS�ϣ���Ҫ������������ã�
            services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);

            // ��ʱ����Ҫ
            //services.AddSignalR();//ʹ�� SignalR

            GlobalContext.Services = services;
            GlobalContext.Configuration = Configuration;

            /*
             * CO2NET �Ǵ� Senparc.Weixin ����ĵײ㹫������ģ�飬�����˳��� 6 ��ĵ����Ż����ȶ��ɿ���
             * ���� CO2NET ��������Ŀ�е�ͨ�����ÿɲο� CO2NET �� Sample��
             * https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
             */
            services.AddSenparcWeixinServices(Configuration); // Senparc.Weixin ע�ᣨ���룩
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            // �����������
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

            #region ע��Senparc΢��

            // ���� CO2NET ȫ��ע�ᣬ���룡
            var registerService = app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister =>
            {
                #region CO2NET ȫ������

                // ��ͬһ���ֲ�ʽ����ͬʱ�����ڶ����վ��Ӧ�ó���أ�ʱ������ʹ�������ռ佫����루�Ǳ��룩
                globalRegister.ChangeDefaultCacheNamespace("QwfCache");

                //����ȫ��ʹ��Redis���棨���裬������
#if DEBUG
                var redisConfigurationStr = Configuration.GetSection("CacheDatabase:ConnectionStringDebug").Value;
#else
                var redisConfigurationStr = Configuration.GetSection("CacheDatabase:ConnectionString").Value;
#endif
                senparcSetting.Value.Cache_Redis_Configuration = redisConfigurationStr; // ����ģ���������
                //Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);
                //���»�������ȫ�ֻ�������Ϊ Redis
                //Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow(); // CsRedis�� ��ֵ�Ի�����ԣ��Ƽ���
                //Senparc.CO2NET.Cache.CsRedis.Register.UseHashRedisNow(); // CsRedis�� HashSet�����ʽ�Ļ������
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr); // StackExchange.Redis��
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); // ��ֵ�Ի�����ԣ��Ƽ���

                #region ע����־�����裬���飩

                // globalRegister.RegisterTraceLog(ConfigTraceLog);//����TraceLog

                #endregion

                // APM ϵͳ����״̬ͳ�Ƽ�¼���� Ĭ���Ѿ�Ϊ�����������Ҫ�رգ�������Ϊ false
                Senparc.CO2NET.APM.Config.EnableAPM = false;

                #endregion
            }, true);
            // ʹ�� Senparc.Weixin SDK
            registerService.UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
            {
                #region ΢���������

                // ΢�Ż��棨���裬����������ÿ�ͷ����ȷ���������������������ע�����ʹ����ȷ�����ã�
                // ע�⣺���ʹ�÷Ǳ��ػ��棬����ִ�б���ע����룬�����յ�����ǰ��չ�������û�н���ע�ᡱ���쳣
                // ΢�ŵ� Redis ���棬�����ʹ����ע�͵�������ǰ���뱣֤������Ч��������״�
                weixinRegister.UseSenparcWeixinCacheRedis(); // StackExchange.Redis
                if (!AccessTokenContainer.CheckRegistered(Configuration.GetSection("WxConfig:AppId").Value))
                {
                    // ע�ṫ�ںţ���ע������
                    weixinRegister.RegisterMpAccount(
                    Configuration.GetSection("WxConfig:AppId").Value,
                    Configuration.GetSection("WxConfig:Secret").Value,
                    "������ѡ");
                }

                #endregion
            });
            #endregion

            app.UseAuthorization(); // ��Ҫ��ע��΢�� SDK ֮��ִ��

            #region ��ʼ��Redis����
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
