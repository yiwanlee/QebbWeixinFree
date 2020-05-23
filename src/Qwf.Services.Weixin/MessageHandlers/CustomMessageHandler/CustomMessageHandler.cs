using Qwf.Services.Weixin.MessageServices;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Agents;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Entities.Request;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Qwf.Services.Weixin.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>  /*如果不需要自定义，可以直接使用：MessageHandler<DefaultMpMessageContext> */
    {
        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEncryptMessage = false)
            : base(inputStream, postModel, maxRecordCount, onlyAllowEncryptMessage)
        {
            GlobalMessageContext.ExpireMinutes = 3;

            //在指定条件下，不使用消息去重
            base.OmitRepeatedMessageFunc = requestMessage =>
            {
                if (requestMessage is RequestMessageText textRequestMessage && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }


        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <param name="requestMessage">请求消息</param>
        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {
            await TextService.OnRequest(requestMessage);

            return new SuccessResponseMessage();
        }

        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        //public override async Task<IResponseMessageBase> OnEventRequestAsync(IRequestMessageEventBase requestMessage)
        //{
        //    var eventResponseMessage = await base.OnEventRequestAsync(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
        //                                                                              //TODO: 对Event信息进行统一操作
        //    return eventResponseMessage;
        //}

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
            * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
            * 只需要在这里统一发出委托请求，如：
            * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            * return responseMessage;
            */
            return new SuccessResponseMessage();
        }
    }
}
