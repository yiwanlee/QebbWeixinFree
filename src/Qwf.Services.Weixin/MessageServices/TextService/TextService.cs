using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Qwf.Services.Weixin.MessageServices;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Agents;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Entities.Request;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Org.BouncyCastle.Ocsp;
using Newtonsoft.Json;

namespace Qwf.Services.Weixin.MessageServices
{
    public class TextService
    {
        public async static Task OnRequest(RequestMessageText requestMessage)
        {
            string appId = Yiwan.Core.GlobalContext.Configuration.GetSection("WxConfig:AppId").Value;
            var openId = requestMessage.FromUserName;//获取OpenId
            if (requestMessage.Content.Equals("容错"))
            {
                await Task.Delay(15000);//故意延时6秒，让微信多次发送消息过来，观察返回结果
                CustomApi.SendText(appId, openId, string.Format("测试容错，MsgId：{0}，Ticks：{1}", requestMessage.MsgId, SystemTime.Now.Ticks));
            }
            else if (requestMessage.Content.Equals("OPENID"))
            {
                var userInfo = UserApi.Info(appId, openId, Language.zh_CN);

                string content = string.Format(
                    "您的OpenID为：{0}\r\n昵称：{1}\r\n性别：{2}\r\n地区（国家/省/市）：{3}/{4}/{5}\r\n关注时间：{6}\r\n关注状态：{7}",
                    requestMessage.FromUserName, userInfo.nickname, (WeixinSex)userInfo.sex, userInfo.country, userInfo.province, userInfo.city, DateTimeHelper.GetDateTimeFromXml(userInfo.subscribe_time), userInfo.subscribe);

                CustomApi.SendText(appId, openId, content);
            }
            else if (requestMessage.Content.Equals("领取购买资格"))
            {
                try
                {
                    var (success, data) = await Yiwan.YouzanAPI.UserTags.TagsAdd(openId, "200526对对对[限粉]");
                    if (success)
                    {
                        string gdurl = "https://shop16758627.m.youzan.com/wscgoods/detail/2ou0gljjprowr";
                        CustomApi.SendText(appId, openId, $"购买资格领取成功！\n\n这是<a href=\"{gdurl}\">下单地址</a>\n请尽快支付,商品售完即止则无法支付购买了");
                    }
                    else
                    {
                        CustomApi.SendText(appId, openId, $"购买资格领取失败！请稍后再试");
                    }
                }
                catch (Exception ex)
                {
                    if (openId.Equals("oAtpFwcxvxtIg0MMRMScGAPUncsA")) CustomApi.SendText(appId, openId, JsonConvert.SerializeObject(ex));
                }
            }
        }
    }
}
