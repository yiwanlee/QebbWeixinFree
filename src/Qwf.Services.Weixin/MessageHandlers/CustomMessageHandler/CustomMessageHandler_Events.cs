using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Qwf.Services.Weixin.MessageServices;
using Org.BouncyCastle.Ocsp;

namespace Qwf.Services.Weixin.CustomMessageHandler
{
    public partial class CustomMessageHandler
    {
        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        public override async Task<IResponseMessageBase> OnEvent_ScanRequestAsync(RequestMessageEvent_Scan requestMessage)
        {
            await Event_ScanService.OnRequest(requestMessage.EventKey, requestMessage.FromUserName, requestMessage.ToUserName, requestMessage.Event);
            return new SuccessResponseMessage();
        }

        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_SubscribeRequestAsync(RequestMessageEvent_Subscribe requestMessage)
        {
            string eventKey = requestMessage.EventKey.StartsWith("qrscene_") ? requestMessage.EventKey.Substring(8) : requestMessage.EventKey;
            await Event_ScanService.OnRequest(eventKey, requestMessage.FromUserName, requestMessage.ToUserName, requestMessage.Event);
            return new SuccessResponseMessage();
        }
    }
}
