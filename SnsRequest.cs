using System;
using System.Collections.Generic;

namespace LOT.Model
{
    /// <summary>
    /// 推送实体模型
    /// </summary>
    [Serializable]
    public partial class SnsRequest
    { 
        public SnsRequest()
        {
            this.PushType = PushType.Else;
        }
          
        /// <summary>
        /// 创建群组时要添加的用户 场景：所有预约某个路演的用户创建群组时自动加入
        /// </summary>
        public string GroupUsers { get; set; }

        /// <summary>
        /// 查询的好友名称
        /// </summary>
        public string FriendName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 添加的好友ID
        /// </summary>
        public int FriendId { get; set; }

        /// <summary>
        /// 群组ID
        /// </summary>
        public int GroupId { get; set; }


        /// <summary>
        /// 推送的消息类型 1.机会宝小助手 2.活动小助手  3.好友通知  4.其它
        /// </summary>
        public PushType PushType { get; set; }

        /// <summary>
        /// 推送的消息Id
        /// </summary>
        public string PushMsgId { get; set; }

        /// <summary>
        /// 推送的消息
        /// </summary>
        public string PushMsg { get; set; }

        /// <summary>
        /// 推送的额外参数
        /// </summary>
        public Dictionary<string, string> PushExtras { get; set; }

        /// <summary>
        /// 推送的用户，多个以英文,分隔
        /// </summary>
        public string PushUsers { get; set; }

        /// <summary>
        /// 检查好友用户列表
        /// </summary>
        /// <value>
        /// The friends.
        /// </value>
        public int[] Friends { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        /// <value>
        /// The relate identifier.
        /// </value>
        public int ActivityId { get; set; }

    }
     

    public enum PushType
    {   
        /// <summary>
        /// 其它
        /// </summary>
        Else = 9, 
    }

    /// <summary>
    /// 推送响应模型
    /// </summary>
    [Serializable]
    public partial class SnsResponse
    {
        /// <summary>
        /// 融云服务返回token
        /// </summary>
        public String Token { get; set; }
          
        /// <summary>
        /// jpush推送的消息ID，可用于查询是否推送到
        /// </summary>
        public long JpushMsgId { get; set; } 
          
    }
}

