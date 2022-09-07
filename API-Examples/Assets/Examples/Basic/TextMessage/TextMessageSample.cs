using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NIM;

namespace nim.examples
{
    public class TextMessageSample : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("You need firstly create you app and fetch your APP_KEY.For more information,visit https://doc.yunxin.163.com/messaging/docs/TEwMDY3NzQ?platform=android")]
        public string APP_KEY = "YOUR APP KEY";

        [SerializeField]
        [Tooltip("You need register an account for yourself firstly.And set password for your account.For more information,visit https://doc.yunxin.163.com/messaging/docs/jMwMTQxODk?platform=android")]
        public string YOUR_ACCOUNT = "";
        public string YOUR_PASSWORD = "";

        [SerializeField]
        [Tooltip("You should specify an account you want to send message")]
        public string YOUR_ROSTER_ACCOUNT = "";


        [Header("Log Output")]
        public Text _logText;


        Logger _logger;
        private bool _isLogining = false;
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
#if UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID
                PushCerName = "", //You should input the push certification name of your app if you need to receive messages when your app is in the background or off line. Detail see to https://doc.yunxin.163.com/messaging/docs/jg2Mzk4MzE?platform=unity
#endif
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

            //listen all events you need
            BindEvents();
            return true;
        }

        //listen events
        private void BindEvents()
        {
            /* All events are not called on main thread.If you want to update UI£¬you should schedule a task to the main thread.
            * You can do it like this:
            * private void HandleLoginResult(NIM.NIMLoginResult result) {
            *   Dispatcher.QueueOnMainThread(() => { 
            *       //update UI here
            *   });
            * }
            */
            NIM.ClientAPI.RegDisconnectedCb(OnDisconnected);
            NIM.ClientAPI.RegAutoReloginCb(OnAutoRelogin);
            NIM.ClientAPI.RegMultiSpotLoginNotifyCb(OnMultiLogin);
            NIM.ClientAPI.RegKickoutCb(OnKickedoutCallback);

            //listen events about message
            NIM.TalkAPI.OnSendMessageCompleted += OnSendMessageResult;
            NIM.TalkAPI.OnReceiveMessageHandler += OnMessageReceived;
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


        // Update is called once per frame
        void Update()
        {

        }

        public void OnSendTextMessageClicked()
        {
            _logger.Log($"OnSendTextMessage");
            if (string.IsNullOrEmpty(YOUR_ROSTER_ACCOUNT))
            {
                _logger.LogError($"ERROR: YOUR_ROSTER_ACCOUNT is empty");
                return;
            }

            //create a message object
            var msg = new NIMTextMessage();
            msg.ReceiverID = YOUR_ROSTER_ACCOUNT;
            msg.SessionType = NIM.Session.NIMSessionType.kNIMSessionTypeP2P;

            //UNIX timestamp,milliseconds
            msg.TimeStamp = Helper.toTicks(System.DateTime.Now);

            //you should generate a message ID .
            //if not, SDK will generate a random ID automatically.
            msg.ClientMsgID = NimUtility.Utilities.GenerateGuid();
            msg.TextContent = "This is a text message";

            //other settings
            msg.Roaming = true;
            msg.SavedOffline = true;
            msg.ServerSaveHistory = true;
            
            //send
            NIM.TalkAPI.SendMessage(msg, null);
        }
        void OnSendMessageResult(object sender, MessageArcEventArgs args)
        {
            _logger.Log($"OnSendMessageResult : {args.ArcInfo.MsgId}, {args.ArcInfo.Response}");
        }
        void OnMessageReceived(object sender, NIMReceiveMessageEventArgs args)
        {
            if (args != null && args.Message != null)
            {
                //You can handle all messages in this.thi callback is not called on the main thread.
                if (args.Message.MessageContent.MessageType == NIMMessageType.kNIMMessageTypeText)
                {
                    var msg = args.Message.MessageContent as NIMTextMessage;
                    _logger.Log($"OnMessageReceived : {msg.SenderID},{msg.SenderNickname},{msg.ClientMsgID},{msg.TimeStamp},\r\n {msg.TextContent}");
                }
                else if (args.Message.MessageContent.MessageType == NIMMessageType.kNIMMessageTypeAudio)
                {
                    //TODO:
                }
                else if (args.Message.MessageContent.MessageType == NIMMessageType.kNIMMessageTypeVideo)
                {
                    //TODO:
                }
                else if (args.Message.MessageContent.MessageType == NIMMessageType.kNIMMessageTypeImage)
                {
                    //TODO:
                }
                else
                {
                    //TODO:
                }
            }
        }

        private void OnApplicationQuit()
        {
            //You Must call logout and cleanup resources for IM SDK.
            //In this,you should call synchronous method.
            // `Cleanup` should be called once only 
            NIM.ClientAPI.Logout(NIMLogoutType.kNIMLogoutAppExit);
            NIM.ClientAPI.Cleanup();
        }
    }

}