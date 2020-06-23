using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Qwf.Services.Weixin.CustomMessageHandler;
using Senparc.CO2NET.AspNet.HttpUtility;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MvcExtension;
using Yiwan.Core;

namespace Qwf.Web.Controllers
{
    public class WeixinController : Controller
    {
        // readonly Func<string> _getRandomFileName = () => SystemTime.Now.ToString("yyyyMMdd-HHmmss") + "_Async_" + Guid.NewGuid().ToString("n").Substring(0, 6);

        [HttpGet, Route("Weixin/{appId}")]
        public async Task<IActionResult> Get(PostModel postModel, string echostr, string appId)
        {
            return await Task.Run(() =>
            {
                string token = GlobalContext.Configuration.GetSection("WxConfig:Token").Value;
                if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, token))
                {
                    return Content(echostr); // 返回随机字符串则表示验证通过
                }
                else
                {
                    return Content($"failed:{Request.Scheme}" + postModel.Signature + ","
                        + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, token) + "。"
                        + "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。【" + appId + "】");
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Tag()
        {
            var (success, data) = await Yiwan.YouzanAPI.UserTags.TagsAdd("oAtpFwcxvxtIg0MMRMScGAPUncsA", "200526对对对[限粉]");
            return Content(data.ToString());
        }

        [HttpPost, Route("Weixin/{appId}")]
        public async Task<IActionResult> Post(string appId, PostModel postModel)
        {
            try
            {
                string token = GlobalContext.Configuration.GetSection("WxConfig:Token").Value;
                string encodingAESKey = GlobalContext.Configuration.GetSection("WxConfig:EncodingAESKey").Value;

                if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, token)) return new WeixinResult("");

                // 打包 PostModel 信息，根据自己后台的设置保持一致
                postModel.Token = token;
                postModel.EncodingAESKey = encodingAESKey;
                postModel.AppId = appId;

                //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
                int maxRecordCount = 2;

                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                CustomMessageHandler messageHandler = new CustomMessageHandler(Request.GetRequestMemoryStream(), postModel, maxRecordCount);

                // messageHandler.SaveRequestMessageLog();//记录 Request 日志（可选）
                var cancellationToken = new CancellationToken(); // 给异步方法使用 
                _ = messageHandler.ExecuteAsync(cancellationToken); // 执行微信处理过程（关键）
                await Task.Delay(1); // messageHandler.SaveResponseMessageLog();//记录 Response 日志（可选）
                return new WeixinResult("");
            }
            catch (Exception)
            {
                //NLogger.Current.Error(ex);
                return new WeixinResult("");
            }
        }

        [HttpGet, Route("Weixin/Qr/{key}")]
        public async Task<IActionResult> Qr(string key)
        {
            string appId = GlobalContext.Configuration.GetSection("WxConfig:AppId").Value;
            Senparc.Weixin.MP.AdvancedAPIs.QrCode.CreateQrCodeResult qrCode = await Senparc.Weixin.MP.AdvancedAPIs.QrCodeApi.CreateAsync(appId,
                    60 * 60 * 24 * 365, 0, QrCode_ActionName.QR_LIMIT_STR_SCENE, key);
            string qrLink = Senparc.Weixin.MP.AdvancedAPIs.QrCodeApi.GetShowQrCodeUrl(qrCode.ticket);
            return Content(qrLink);
        }
    }
}