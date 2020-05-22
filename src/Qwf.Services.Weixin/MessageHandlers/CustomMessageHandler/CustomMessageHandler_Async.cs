using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qwf.Services.Weixin.CustomMessageHandler
{
    public partial class CustomMessageHandler
    {
        public override async Task OnExecutingAsync(CancellationToken cancellationToken)
        {
            //演示：MessageContext.StorageData

            //var currentMessageContext = await base.GetUnsafeMessageContext();//为了在分布式缓存下提高读写效率，使用此方法，如果需要获取实时数据，应该使用 base.GetCurrentMessageContext()
            //if (currentMessageContext.StorageData == null || !(currentMessageContext.StorageData is int))
            //{
            //    currentMessageContext.StorageData = (int)0;
            //    //await GlobalMessageContext.UpdateMessageContextAsync(currentMessageContext);//储存到缓存
            //}
            await base.OnExecutingAsync(cancellationToken);
        }

        public override async Task OnExecutedAsync(CancellationToken cancellationToken)
        {
            //演示：MessageContext.StorageData

            //var currentMessageContext = await base.GetUnsafeMessageContext();//为了在分布式缓存下提高读写效率，使用此方法，如果需要获取实时数据，应该使用 base.GetCurrentMessageContext()
            //currentMessageContext.StorageData = ((int)currentMessageContext.StorageData) + 1;
            //GlobalMessageContext.UpdateMessageContext(currentMessageContext);//储存到缓存
            await base.OnExecutedAsync(cancellationToken);
        }
    }
}
