using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NIM;

namespace nim.examples
{
    public class LoginSample : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("You need firstly create you app and fetch your APP_KEY.For more information,visit https://doc.yunxin.163.com/messaging/docs/TEwMDY3NzQ?platform=android")]
        public string APP_KEY = "YOUR APP KEY";

        [SerializeField]
        [Tooltip("You need register an account for yourself firstly.And set password for your account.For more information,visit https://doc.yunxin.163.com/messaging/docs/jMwMTQxODk?platform=android")]
        public string YOUR_ACCOUNT = "";
        public string YOUR_PASSWORD = "";


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
            if(Application.platform == RuntimePlatform.WindowsEditor)
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
            if(!result)
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
        }

        private void DoLogin()
        {
            if(string.IsNullOrEmpty(YOUR_ACCOUNT) || string.IsNullOrEmpty(YOUR_PASSWORD))
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

            Dispatcher.QueueOnMainThread(() => {
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
        public void OnLoginClicked()
        {
            _logger.Log($"OnLoginClicked");
            if (_isLogining)
            {
                _logger.LogWarning($"OnLoginClicked isLogining : {_isLogining}");
                return;
            }

            if (_isLogouting)
            {
                _logger.LogWarning($"OnLoginClicked isLogouting : {_isLogouting}");
                return;
            }

            //You should call `InitSDK` again.
            if (InitSDK())
            {
                DoLogin();
            }
        }

        public void OnLogoutClicked()
        {
            _logger.Log($"OnLogoutClicked");
            // if you want to login with another account, you can do it like this
            if (_isLogouting)
            {
                _logger.Log($"NIM.ClientAPI.Logout is executing");
                return;
            }
            _isLogouting = true;
            NIM.ClientAPI.Logout(NIMLogoutType.kNIMLogoutChangeAccout, OnAppLogoutCompleted);
        }

        private void OnAppLogoutCompleted(NIMLogoutResult result)
        {
            _logger.Log($"switch account completed £º {result.Code}");
            //You can't cleanup in this. You should cleanup when the application will be quit
            //NIM.ClientAPI.Cleanup()

            _isLogouting = false;
            _isLogining = false;
        }
        private void OnApplicationQuit()
        {
            //You Must call `logout` and `cleanup` to release resources.
            //In this,you should call synchronous method.
            // `Cleanup` should be called once only 
            NIM.ClientAPI.Logout(NIMLogoutType.kNIMLogoutAppExit);
            NIM.ClientAPI.Cleanup();
        }
    }

}
