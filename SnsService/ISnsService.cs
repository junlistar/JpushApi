
using LOT.Application.SnsService.Dtos;
using LOT.Application.SylArticleService.Dtos;
using LOT.Model;
using System.Collections.Generic;

namespace LOT.Application.SnsService
{
    /// <summary>
    /// 用户文章数据接口
    /// </summary>
    public interface ISnsService : IDependency
    {
        #region ***jpush推送服务***
        /// <summary>
        /// 推送消息给所有APP端
        /// </summary>
        /// <param name="request">request.PushMsg</param>
        /// <returns></returns>
        SnsResponse JpushSendToAll(SnsRequest request);

        /// <summary>
        /// 推送消息给别名用户
        /// </summary>
        /// <param name="request">request.PushUsers 以,分隔的多个别名</param>
        /// <returns></returns>
        SnsResponse JpushSendToAlias(SnsRequest request);

        /// <summary>
        /// 推送消息给安卓客户端
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        SnsResponse JpushSendToAndroid(SnsRequest request);

        /// <summary>
        /// 推送消息给Ios客户端
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        SnsResponse JpushSendToIos(SnsRequest request);

        /// <summary>
        /// 推送消息给标签用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        SnsResponse JpushSendToTag(SnsRequest request);

        /// <summary>
        /// 检测某条消息是否推送成功
        /// </summary>
        /// <param name="request">request.PushMsgId</param>
        /// <returns></returns>
        SnsResponse JpushSendCheck(SnsRequest request);
        #endregion

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="isToAll">类型</param>
        /// <param name="pageIndex">分页下标</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="totalCount">总数量</param>
        /// <returns></returns>
        List<SnsDTO> PageList(string name, int isToAll, int pageIndex, int pageSize, out int totalCount);
    }
}
