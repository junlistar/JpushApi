using System;
using System.Linq;
using System.Collections.Generic;
using LOT.Model;
using LOT.FrameworkCore;
using LOT.Application.SylArticleService.Dtos;
using LOT.Log;
using cn.jpush.api.push.mode;
using cn.jpush.api.push.notification;
using LOT.Application.SnsService;
using cn.jpush.api;
using cn.jpush.api.common.resp;
using System.Configuration;
using System.Timers;
using cn.jpush.api.common;
using LOT.Application.SnsService.Dtos;

namespace LOT.Application.SylArticleService
{
    /// <summary>
    /// SNS数据接口
    /// </summary>
    public class SnsService : ISnsService
    {
        private JPushClient _client;
        private String _rongAppKey;
        private String _rongAppSecret;
        private String _jpushAppKey;
        private String _jpushAppSecret;

        private String _jpushAppApn;  //IOS推送通道

        private string _pushGroupSize = "PUSHUSERCOUNT_PERTIMES"; //推送多人每组长度
        private int _defaultPushGroupSize = 5;

        static SnsCore _core = new SnsCore();

        #region  属性

        private bool JpushAppApn
        {
            get
            {
                return _jpushAppApn == "1" ? true : false;
            }
        }

        private String JpushAppKey
        {
            get
            {
                if (string.IsNullOrEmpty(_jpushAppKey))
                {
                    _jpushAppKey = ConfigurationManager.AppSettings["jpushAppKey"];
                }
                return _jpushAppKey;
            }
        }
        private String JpushAppSecret
        {
            get
            {
                if (string.IsNullOrEmpty(_jpushAppSecret))
                {
                    _jpushAppSecret = ConfigurationManager.AppSettings["jpushAppSecret"];

                }
                return _jpushAppSecret;
            }
        }

        private JPushClient JPushClient
        {
            get
            {
                if (_client == null)
                {
                    _client = new JPushClient(JpushAppKey, JpushAppSecret);
                }
                return _client;
            }
        }
        #endregion

        #region ***jpush推送服务***


        private PushPayload GetPushPayload(SnsRequest request)
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            pushPayload.audience = Audience.all();

            pushPayload.options.apns_production = JpushAppApn; //JpushAppApn
            //pushPayload.notification = new Notification().setAlert(request.PushMsg);

            AndroidNotification androidNotification = new AndroidNotification();
            IosNotification iosNotification = new IosNotification();
            androidNotification.setAlert(request.PushMsg);
            iosNotification.setAlert(request.PushMsg);
            if (request.PushExtras != null)
            {
                foreach (var item in request.PushExtras)
                {
                    androidNotification.AddExtra(item.Key, item.Value);
                    iosNotification.AddExtra(item.Key, item.Value);
                }
            }

            if ((int)request.PushType != 0)
            {
                androidNotification.AddExtra("type", (int)request.PushType);
                iosNotification.AddExtra("type", (int)request.PushType);
            }

            cn.jpush.api.push.mode.Notification noti = new cn.jpush.api.push.mode.Notification();
            noti.setAndroid(androidNotification);
            noti.setIos(iosNotification);
            //pushPayload.notification = new Notification().setAndroid(noty1);
            //pushPayload.notification = new Notification().setIos(noty2);

            pushPayload.notification = noti;
            return pushPayload;
        }

        private void CustPush(object sender, ElapsedEventArgs e)
        {

        }

        /// <summary>
        /// 推送消息给别名用户
        /// </summary>
        /// <param name="request">request.PushUsers 以,分隔的多个别名</param>
        /// <returns></returns>
        public SnsResponse JpushSendToAlias(SnsRequest request)
        {
            PushPayload pushPayload = GetPushPayload(request);
            var userlist = request.PushUsers.Split(',');
             
            //写记录
            System.Threading.Tasks.Task.Run(() =>
            {
                Sns _model = new Sns();
                _model.CreateTime = DateTime.Now;
                _model.CreatePersonId = request.UserId;
                _model.IsToAll = 0;
                _model.PushUserId = request.PushUsers;
                _model.PushMsg = request.PushMsg;
                if (request.PushExtras!=null)
                {
                    _model.ParamString = request.PushExtras.ToString();
                } 
                _core.Insert(_model); 
            });

            SnsResponse response = new SnsResponse();


            int splitSize = _defaultPushGroupSize;//分割的块大小  
            Object[] subAry = LOT.Common.StringToolsHelper.splitAry(userlist, splitSize);//分割后的子块数组  

            //分批次推送操作
            for (int i = 0; i < subAry.Length; i++)
            {
                string[] aryItem = (string[])subAry[i];
                var itemStr = string.Join(",", aryItem);
                try
                {

                    pushPayload.audience = Audience.s_alias(aryItem);

                    var result = JPushClient.SendPush(pushPayload);
                    response.JpushMsgId = result.msg_id;

                    #region 推送日志

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        //写日志  
                        Logger.Error("SnsService———>JpushSendToAlias：" + string.Format("认证用户发送jpush用户ID列表:{0}", itemStr));
                    });

                    #endregion

                }
                catch (Exception e)
                {
                    Logger.Error("SnsService———>JpushSendToAlias：" + string.Format("认证用户发送jpush:{0},提供的错误信息：{1},id列表：{2}", e.Message, ((cn.jpush.api.common.APIRequestException)e).ErrorMessage, itemStr));

                }
                //休息一秒 避免：Request times of the app_key exceed the limit of current time window
                System.Threading.Thread.Sleep(100);
            }

            return response;
        }

        /// <summary>
        /// 推送消息给特定标签用户
        /// </summary>
        /// <param name="request">request.PushUsers 以,分隔的多个标签</param>
        /// <returns></returns>
        public SnsResponse JpushSendToTag(SnsRequest request)
        {
            PushPayload pushPayload = GetPushPayload(request);
            var userlist = request.PushUsers.Split(',');
            pushPayload.audience = Audience.s_tag(userlist);

            var result = JPushClient.SendPush(pushPayload);
            SnsResponse response = new SnsResponse();
            response.JpushMsgId = result.msg_id;
            return response;
        }

        /// <summary>
        /// 推送消息给所有APP端
        /// </summary>
        /// <param name="request">request.PushMsg</param>
        /// <returns></returns>
        public SnsResponse JpushSendToAll(SnsRequest request)
        {
            PushPayload pushPayload = GetPushPayload(request);
             
            //写记录
            System.Threading.Tasks.Task.Run(() =>
            {
                Sns _model = new Sns();
                _model.CreateTime = DateTime.Now;
                _model.CreatePersonId = request.UserId;
                _model.PushMsg = request.PushMsg;
                _model.IsToAll = 1;
                if (request.PushExtras != null)
                {
                    _model.ParamString = request.PushExtras.ToString();
                }
                _core.Insert(_model);
            });

            var result = JPushClient.SendPush(pushPayload);
            SnsResponse response = new SnsResponse();
            response.JpushMsgId = result.msg_id;
            return response;
        }

        /// <summary>
        /// 推送消息给安卓客户端
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SnsResponse JpushSendToAndroid(SnsRequest request)
        {
            PushPayload pushPayload = GetPushPayload(request);
            pushPayload.platform = Platform.android();

            var result = JPushClient.SendPush(pushPayload);
            SnsResponse response = new SnsResponse();
            response.JpushMsgId = result.msg_id;
            return response;
        }

        /// <summary>
        /// 推送消息给Ios客户端
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SnsResponse JpushSendToIos(SnsRequest request)
        {
            PushPayload pushPayload = GetPushPayload(request);
            pushPayload.platform = Platform.ios();

            var result = JPushClient.SendPush(pushPayload);
            SnsResponse response = new SnsResponse();
            response.JpushMsgId = result.msg_id;
            return response;
        }



        /// <summary>
        /// 检测某条消息是否推送成功
        /// </summary>
        /// <param name="request">request.PushMsgId</param>
        /// <returns></returns>
        public SnsResponse JpushSendCheck(SnsRequest request)
        {
            SnsResponse response = new SnsResponse();
            try
            {
                //如需查询上次推送结果执行下面的代码
                //var apiResult = _client.getReceivedApi(result.msg_id.ToString());
                //var apiResultv3 = _client.getReceivedApi_v3(result.msg_id.ToString());
                //如需查询某个messageid的推送结果执行下面的代码 
                var querResultWithV3 = _client.getReceivedApi_v3(request.PushMsgId);
            }
            catch (APIRequestException e)
            {
                //response.Message.Result = MessageResult.FAILED;
                //response.Message.Content = string.Format("Http Status:{0} Error Message:{1}", e.Status, e.ErrorMessage);
                //response.Message.MessageCode = e.ErrorCode.ToString();
                //response.Message.MessageID = request.PushMsgId;
            }
            catch (APIConnectionException e)
            {
                //response.Message.Result = MessageResult.FAILED;
                //response.Message.Content = string.Format("APIConnectionException Error Message:{0}", e.Message);
                //response.Message.MessageID = request.PushMsgId;
            }

            return response;
        }

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
        public List<SnsDTO> PageList(string name, int isToAll, int pageIndex, int pageSize, out int totalCount)
        {
            //返回数据总数
            totalCount = 0;
            try
            {
                //拼接查询SQL
                string strWhere = " where 1=1  ";
                if (!string.IsNullOrWhiteSpace(name))
                    strWhere += string.Format(" and a.PushMsg like '%{0}%'", name);
                //查询SQL
                string strSql = string.Format(@"SELECT a.* FROM Sns a {0} ORDER BY a.CreateTime desc", strWhere);
                //返回分页数据集合

                var list = _core.DbContext.SqlQuery<SnsDTO>(strSql).Skip(pageIndex).Take(pageSize).ToList();

                //查询数据总数SQL
                string totalCountSql = string.Format(@"SELECT count(1) FROM Sns {0}", strWhere);

                //执行查询数据总数
                totalCount = _core.DbContext.SqlQuery<int>(totalCountSql).First();

                //返回分页数据集合
                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("SnsService———>PageList：" + ex.ToString());
                return null;
            }

        }
    }
}
