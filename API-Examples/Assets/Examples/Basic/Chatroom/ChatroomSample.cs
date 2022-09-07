using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NIM;

namespace nim.examples
{
    public class ChatroomSample : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("You need firstly create you app and fetch your APP_KEY.For more information,visit https://doc.yunxin.163.com/messaging/docs/TEwMDY3NzQ?platform=android")]
        public string APP_KEY = "YOUR APP KEY";

        [SerializeField]
        [Tooltip("You need register an account for yourself firstly.And set password for your account.For more information,visit https://doc.yunxin.163.com/messaging/docs/jMwMTQxODk?platform=android")]
        public string YOUR_ACCOUNT = "";
        public string YOUR_PASSWORD = "";

        [SerializeField]
        [Tooltip("You should call server-api to create chat room and retrieve the room ID.For more information,visit https://doc.yunxin.163.com/messaging/docs/jA0MzQxOTI?platform=server")]
        public long CHATROOM_ID = 0;


        [Header("Log Output")]
        public Text _logText;


        Logger _logger;
        bool _isLogining = false;
        bool _isLogouting = false;
        // Start is called before the first frame update
        void Start()
        {
            _logger = new Logger(_logText);

            if (InitSDK())
            {
                DoLogin();
            }
        }

        private bool InitSDK()
        {
            //You can dump the logs of the SDK in you editor
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                NIM.GlobalAPI.SetSdkLogCallback(DumpSDKLog);
            }
            //create a NIMConfig object if need
            var config = new NimUtility.NimConfig();
            config.AppKey = APP_KEY;
            config.CommonSetting = new NimUtility.SdkCommonSetting
            {
                MaxLoginRetry = 3,//relogin times
                PredownloadAttachmentThumbnail = true, //auto download the thumbnails for the image message
                PreloadImageQuality = 100, // the quality of the image thumbnail
                PreloadImageResize = "100x100", //the size of the image thumbnail
                SyncSessionAck = true,
                CustomTimeout = 15,// login timeout,seconds
            };

            //the appDataPath must be writable and readable.Normally,you can use `Application.persistentDataPath` to save log files and cache files.
            //the appInstallPath is invalid,you should set `string.Empty` for it
            //the config can be null.
            //you should call `Init` before you call `login`.
            string appDataPath = Path.Combine(Application.persistentDataPath, "Sample");
            bool result = NIM.ClientAPI.Init(appDataPath, string.Empty, config);
            if (!result)
            {
                _logger.LogError($"NIM.ClientAPI.Init failed");
                return false;
            }
            _logger.Log($"NIM.ClientAPI.Init success!!");

            //Next,you should initialize chat room plugin.
            var chatRoomConfig = new NIMChatRoom.NIMChatRoomConfig();
            chatRoomConfig.AppKey = APP_KEY;
            NIMChatRoom.ChatRoomApi.Init(chatRoomConfig);

            //listen all events you need
            BindEvents();
            return true;
        }

        //listen events
        private void BindEvents()
        {
            //all callbacks isn't called on the main thread.
            //If you want to update UI elements,you must schedule a task to the main thread.

            NIM.ClientAPI.RegDisconnectedCb(OnDisconnected);
            NIM.ClientAPI.RegAutoReloginCb(OnAutoRelogin);
            NIM.ClientAPI.RegMultiSpotLoginNotifyCb(OnMultiLogin);
            NIM.ClientAPI.RegKickoutCb(OnKickedoutCallback);

            NIMChatRoom.ChatRoomApi.LoginHandler += ChatRoomApi_LoginHandler;
            NIMChatRoom.ChatRoomApi.ExitHandler += ChatRoomApi_ExitHandler;
            NIMChatRoom.ChatRoomApi.LinkStateChanged += ChatRoomApi_LinkStateChanged;
            NIMChatRoom.ChatRoomApi.SendMessageHandler += ChatRoomApi_SendMessageHandler;
            NIMChatRoom.ChatRoomApi.ReceiveMessageHandler += ChatRoomApi_ReceiveMessageHandler;
            NIMChatRoom.ChatRoomApi.ReceiveNotificationHandler += ChatRoomApi_ReceiveNotificationHandler;
        }

        private void DoLogin()
        {
            if (string.IsNullOrEmpty(YOUR_ACCOUNT) || string.IsNullOrEmpty(YOUR_PASSWORD))
            {
                _logger.LogWarning($"You should input account and password");
                return;
            }
            _isLogining = true;
            NIM.ClientAPI.Login(APP_KEY, YOUR_ACCOUNT, YOUR_PASSWORD, HandleLoginResult);
        }
        
        private void HandleLoginResult(NIM.NIMLoginResult result)
        {
            _logger.Log($"HandleLoginResult IsRelogin: {result.IsRelogin} ,{result.LoginStep} , {result.Code}");

            Dispatcher.QueueOnMainThread(() =>
            {
                switch (result.LoginStep)
                {
                    case NIMLoginStep.kNIMLoginStepLinking:
                        break;
                    case NIMLoginStep.kNIMLoginStepLink:
                        break;
                    case NIMLoginStep.kNIMLoginStepLogin:
                        _isLogining = false;
                        if (result.Code == NIM.ResponseCode.kNIMResSuccess)
                        {
                            _logger.Log($"HandleLoginResult Login success");
                            Dispatcher.QueueOnMainThread(() => {
                                DoLoginChatroom();
                            });
                        }
                        else
                        {
                            _logger.Log($"HandleLoginResult Login failed for {result.Code}");
                        }
                        break;
                }
            });

        }

        private void OnDisconnected()
        {
            _logger.Log("OnDisconnected");
        }

        private void OnAutoRelogin(NIMLoginResult result)
        {
            _logger.Log($"OnAutoRelogin result £º {result.Serialize()}");
        }

        private void OnMultiLogin(NIMMultiSpotLoginNotifyResult result)
        {
            _logger.Log($"OnMultiLogin result : {result.Serialize()}");
            //result.OtherClients
            if (result.NotifyType == NIMMultiSpotNotifyType.kNIMMultiSpotNotifyTypeImIn)
            {
                //Your account had been logged in the other equipment.
            }
            else if (result.NotifyType == NIMMultiSpotNotifyType.kNIMMultiSpotNotifyTypeImOut)
            {
                //Your account had been logout from the other equipment.
            }
        }

        private void OnKickedoutCallback(NIMKickoutResult result)
        {
            _logger.Log($"OnKickedoutCallback £º ClientType - {result.ClientType}, KickReason - {result.KickReason}");
        }

        private void DumpSDKLog(int level, string log)
        {
            Debug.Log($"[IM SDK] {log}");
        }

        #region Chatroom Events


        private void DoLoginChatroom()
        {
            if(NIM.ClientAPI.GetLoginState() != NIMLoginState.kNIMLoginStateLogin)
            {
                _logger.LogWarning($"You should login IM server.");
                return;
            }

            if (CHATROOM_ID == 0)
            {
                _logger.LogWarning($"CHATROOM_ID is invalid.");
                return;
            }

            //You should request a token before you enter chat room
            NIM.Plugin.ChatRoom.RequestLoginInfo(CHATROOM_ID, (code, result) => {

                if(code != ResponseCode.kNIMResSuccess)
                {
                    _logger.LogError($"RequestLoginInfo failed : {code}");
                    return;
                }
                _logger.LogError($"RequestLoginInfo success");

                //And then, you can enter to chat room.Better schedule a task to main thread.
                Dispatcher.QueueOnMainThread(() =>
                {
                    var loginData = new NIMChatRoom.LoginData()
                    {
                        Nick = "nickName",
                    };
                    NIMChatRoom.ChatRoomApi.Login(CHATROOM_ID, result, loginData);
                });
            });
        }

        private void DoExitChatroom()
        {
            _logger.Log($"DoExitChatroom");
            if (CHATROOM_ID == 0)
            {
                _logger.LogWarning($"CHATROOM_ID is invalid.");
                return;
            }

            NIMChatRoom.ChatRoomApi.Exit(CHATROOM_ID);
            //Better to sleep for a while.
            System.Threading.Thread.Sleep(20);
        }

        private void DoSendMessage()
        {
            _logger.Log($"DoSendMessage");
            if (CHATROOM_ID == 0)
            {
                _logger.LogWarning($"CHATROOM_ID is invalid.");
                return;
            }

            //send a text message
            var msg = new NIMChatRoom.Message();
            msg.MessageType = NIMChatRoom.NIMChatRoomMsgType.kNIMChatRoomMsgTypeText;
            msg.AntiSpamEnabled = false;
            msg.AntiSpamContent = "";
            msg.MessageAttachment = "This is a test message ";
            msg.ClientMsgId = NIM.ToolsAPI.GetUuid();

            NIMChatRoom.ChatRoomApi.SendMessage(CHATROOM_ID, msg);
        }

        private void ChatRoomApi_LoginHandler(NIMChatRoom.NIMChatRoomLoginStep loginStep, ResponseCode errorCode, NIMChatRoom.ChatRoomInfo roomInfo, NIMChatRoom.MemberInfo memberInfo)
        {
            _logger.Log($"ChatRoomApi_LoginHandler loginStep : {loginStep}, errorCode :{errorCode}");

            /*roomInfo and memberInfo maybe null,so you should do it like this:
            *if(roomInfo != null) { 
            *   //do other things
            * }
            */
            if (loginStep == NIMChatRoom.NIMChatRoomLoginStep.kNIMChatRoomLoginStepRoomAuthOver && errorCode == NIM.ResponseCode.kNIMResSuccess)
            {
                //You have entered chat room
                _logger.Log($"ChatRoomApi_LoginHandler success");
                _logger.Log($"roomInfo : \r\n{roomInfo.Serialize()}");
                _logger.Log($"memberInfo : \r\n{memberInfo}");
            }
            if (errorCode != NIM.ResponseCode.kNIMResSuccess)
            {
                //Failure
                _logger.Log($"ChatRoomApi_LoginHandler failed");
            }
        }
        private void ChatRoomApi_LinkStateChanged(long roomId, NIMChatRoom.NIMChatRoomLinkCondition state)
        {
            _logger.Log($"ChatRoomApi_LinkStateChanged roomId :{roomId},state : {state}");

            //if state is `kNIMChatRoomLinkConditionDead`,you should exit room and reenter into chat room.
            if (state == NIMChatRoom.NIMChatRoomLinkCondition.kNIMChatRoomLinkConditionDead)
            {
                Dispatcher.QueueOnMainThread(() => {
                    DoExitChatroom();
                    DoLoginChatroom();
                });
            }
        }
        private void ChatRoomApi_ExitHandler(long roomId, ResponseCode errorCode, NIMChatRoom.NIMChatRoomExitReason reason)
        {
            _logger.Log($"ChatRoomApi_ExitHandler roomId :{roomId},errorCode : {errorCode},reason :{reason}");
        }
        private void ChatRoomApi_SendMessageHandler(long roomId, ResponseCode code, NIMChatRoom.Message message)
        {
            _logger.Log($"ChatRoomApi_SendMessageHandler roomId :{roomId},errorCode : {code}, \r\n message :{message.Serialize()}");
        }

        private void ChatRoomApi_ReceiveMessageHandler(long roomId, NIMChatRoom.Message message)
        {
            _logger.Log($"ChatRoomApi_ReceiveMessageHandler roomId :{roomId}, sender: {message.SenderId},nickName: {message.SenderNickName}\r\n message :{message.Serialize()}");
        }

        private void ChatRoomApi_ReceiveNotificationHandler(long roomId, NIMChatRoom.Notification notification)
        {
            _logger.Log($"ChatRoomApi_ReceiveNotificationHandler type: {notification.Type} \r\n {notification.InnerData}");
        }
        #endregion
        // Update is called once per frame
        void Update()
        {

        }

        public void OnEnterChatroomClicked()
        {
            _logger.Log($"OnEnterChatroomClicked");
            DoLoginChatroom();
        }

        public void OnExitChatroomClicked()
        {
            _logger.Log($"OnExitChatroomClicked");
            DoExitChatroom();
        }

        public void OnSendMessageToChatroom()
        {
            _logger.Log($"OnSendMessageToChatroom");
            DoSendMessage();
        }

        private void OnApplicationQuit()
        {
            //You Must call logout and cleanup resources for IM SDK.
            //In this,you should call synchronous method.
            // `Cleanup` should be called once only 
            //Firstly,you should exit chat room.
            NIMChatRoom.ChatRoomApi.Exit(CHATROOM_ID);
            NIM.ClientAPI.Logout(NIMLogoutType.kNIMLogoutAppExit);

            //Cleanup chat room plugin and then cleanup IM resources.
            NIMChatRoom.ChatRoomApi.Cleanup();
            NIM.ClientAPI.Cleanup();
        }
    }

}
