﻿using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Qwf.Services.Weixin.CustomMessageHandler
{
    public partial class CustomMessageHandler
    {
        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        public override async Task<IResponseMessageBase> OnEvent_ScanRequestAsync(RequestMessageEvent_Scan requestMessage)
        {
            await Task.Delay(1);
            return new SuccessResponseMessage();
        }
    }
}
