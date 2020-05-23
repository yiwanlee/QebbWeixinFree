using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin.Cache.Redis;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Containers;
using System;
using Yiwan.Core;
using Yiwan.Utilities.Cache;

namespace Qwf.Test.Consoles
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 初始化Configuration 和 Redis
            GlobalContext.Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
#if DEBUG
            var redisConfigurationStr = GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionStringDebug").Value;
#else
            var redisConfigurationStr =  GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionString").Value;
#endif
            RedisConnectionHelper.Initialize(redisConfigurationStr, "QEBB:");
            #endregion 

            #region 注释掉的
            //            #region 初始化微信
            //            var builder = new ConfigurationBuilder()
            //                .SetBasePath(AppContext.BaseDirectory)
            //                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //                .AddEnvironmentVariables();
            //            GlobalContext.Configuration = builder.Build();
            //            string appId = GlobalContext.Configuration.GetSection("WxConfig:AppId").Value;
            //            string secret = GlobalContext.Configuration.GetSection("WxConfig:Secret").Value;
            //            Console.WriteLine(appId + "@" + secret);
            //            // 开始注册
            //            SenparcSetting senparcSetting = new SenparcSetting();
            //            IRegisterService register = RegisterService.Start(senparcSetting).UseSenparcGlobal();
            //            // 当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
            //            register.ChangeDefaultCacheNamespace("QwfCache");
            //            //配置全局使用Redis缓存（按需，独立）
            //#if DEBUG
            //            var redisConfigurationStr = GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionStringDebug").Value;
            //#else
            //                var redisConfigurationStr =  GlobalContext.Configuration.GetSection("CacheDatabase:ConnectionString").Value;
            //#endif
            //            senparcSetting.Cache_Redis_Configuration = redisConfigurationStr; // 必须的，否则会错误
            //            Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr); // StackExchange.Redis库
            //            Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow(); // 键值对缓存策略（推荐）
            //            Senparc.CO2NET.APM.Config.EnableAPM = false;
            //            // 微信相关配置
            //            // 微信缓存（按需，必须放在配置开头，以确保其他可能依赖到缓存的注册过程使用正确的配置）
            //            // 注意：如果使用非本地缓存，而不执行本块注册代码，将会收到“当前扩展缓存策略没有进行注册”的异常
            //            // 微信的 Redis 缓存，如果不使用则注释掉（开启前必须保证配置有效，否则会抛错）
            //            register.UseSenparcWeixinCacheRedis(); // StackExchange.Redis

            //            if (!AccessTokenContainer.CheckRegistered(appId))
            //            {
            //                register.RegisterMpAccount(
            //                GlobalContext.Configuration.GetSection("WxConfig:AppId").Value,
            //                GlobalContext.Configuration.GetSection("WxConfig:Secret").Value,
            //                "茵茵优选");
            //            }
            //            var token = AccessTokenContainer.GetAccessToken(appId);
            //            Console.WriteLine("Token：" + token);
            //            #endregion
            //            #region 创建参数二维码
            //            Console.WriteLine("是否创建参数二维码(Y/N)");
            //            if (Console.ReadLine().ToLower().Equals("y"))
            //            {
            //                Console.WriteLine("请输入二维码的KEY");
            //                var qrkey = Console.ReadLine();
            //                Console.WriteLine(qrkey);
            //                Senparc.Weixin.MP.AdvancedAPIs.QrCode.CreateQrCodeResult qrCode = Senparc.Weixin.MP.AdvancedAPIs.QrCodeApi.Create(appId,
            //                    60 * 60 * 24 * 365, 0, QrCode_ActionName.QR_LIMIT_STR_SCENE, qrkey);

            //                string qrLink = Senparc.Weixin.MP.AdvancedAPIs.QrCodeApi.GetShowQrCodeUrl(qrCode.ticket);
            //                Console.WriteLine(qrLink);
            //            }
            //            #endregion
            #endregion

            string token = Yiwan.YouzanAPI.YozClient.GetToken().Result;
            var (success, data) = Yiwan.YouzanAPI.UserTags.TagsAdd("oAtpFwcxvxtIg0MMRMScGAPUncsA", "测试标签").Result;
            Console.WriteLine(success.ToString());
            Console.WriteLine(token);
            Console.WriteLine(JsonConvert.SerializeObject(data));
        }
        public class Student
        {
            public string Name { set; get; }
            public int Age { set; get; }
        }
    }
}
