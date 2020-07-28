using Newtonsoft.Json;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP;

namespace Qwf.Services.Weixin.MessageServices
{
    public class Event_ScanService
    {
        public async static Task OnRequest(string eventKey, string openId, string orgId, Event eventType)
        {
            string appId = Yiwan.Core.GlobalContext.Configuration.GetSection("WxConfig:AppId").Value;
            //var openId = requestMessage.FromUserName;//获取OpenId
            if (openId.Equals("oAtpFwcxvxtIg0MMRMScGAPUncsA")) CustomApi.SendText(appId, openId, $"类型：{eventType}\n事件：{eventKey}\n账户：" + orgId);
            if (eventKey.Equals("200618ZPW"))
            {
                try
                {
                    var (success, data) = await Yiwan.YouzanAPI.UserTags.TagsAdd(openId, "200618ZPW[限粉]");
                    if (success)
                    {
                        string gdurl = "https://shop16758627.m.youzan.com/wscgoods/detail/277mzxl98kgbf";
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
            else if (eventKey.Equals("200526DDD"))
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
            else if (eventKey.Equals("200720HT"))
            {
                try
                {
                    var (success, data) = await Yiwan.YouzanAPI.UserTags.TagsAdd(openId, "200720海苔[限粉]");
                    if (success)
                    {
                        string gdurl = "https://shop16758627.m.youzan.com/wscgoods/detail/3eo0pgmz97l1n";
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
