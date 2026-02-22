
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Firebase;
using Google;
using Firebase.Auth;
using Firebase.Extensions;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using Facebook.Unity;

public enum LoginState
{
    None,
    Login,
}

public class TpPlatformLoginProp
{
    public enum LoginPlatformType
    {
        None,
        Google,
        Facebook,
        Apple,
    }

    public bool IsLogin { get; private set; } = false;
    public LoginPlatformType LoginPlatform { get; private set; } = LoginPlatformType.None;
    private const string AppleUserIdKey = "AppleUserId";
    private const string LoginPlatformKey = "LoginPlatform";

    private const string GoogleTokenKey = "GoogleTokenKey";
    private const string AppleTokenKey = "AppleTokenKey";

#if UNITY_IOS || UNITY_IPHONE || UNITY_EDITOR
    private const string webClientId = "1044374056454-ovv9etn1o17vg5fiin7kgc5qg77sftbf.apps.googleusercontent.com";
#else
    private const string webClientId = "1044374056454-mvdaidog3pj0r8p0g2d9hh26jr4rr6fj.apps.googleusercontent.com";
#endif
    private IAppleAuthManager _appleAuthManager;
    public static FirebaseUser fUser = null;
    private FirebaseAuth auth = null;
    private GoogleSignInConfiguration configuration;

    public static LoginState state = LoginState.None;

    public string platformToken = string.Empty;

    public static string UserID {
        get { 
            if (fUser == null){ return ""; }
            return fUser.UserId;
        }
    }

    public void InitPlatformLogin()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this._appleAuthManager = new AppleAuthManager(deserializer);
            this._appleAuthManager.SetCredentialsRevokedCallback(result =>
            {
                BpLog.Log("Received revoked callback " + result);
                PlayerPrefs.DeleteKey(AppleUserIdKey);
            });
        }
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        if (PlayerPrefs.HasKey(LoginPlatformKey))
        {
            Debug.Log("LoginPlatformKey : " + PlayerPrefs.GetString(LoginPlatformKey));

            var prevLogin = PlayerPrefs.GetString(LoginPlatformKey, LoginPlatformType.None.ToString());
            LoginPlatformType type = LoginPlatformType.None;
            if (Enum.TryParse(prevLogin, out type))
            {
                switch (type)
                {
                    case LoginPlatformType.Google:
                        {
                            GoogleSignIn.Configuration = configuration;
                            GoogleSignIn.Configuration.UseGameSignIn = false;
                            GoogleSignIn.Configuration.RequestIdToken = true;
                            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
                                    {
                                        if (enumerator.MoveNext())
                                        {
                                            GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                                            BpLog.LogError("Got Error: " + error.Status + " " + error.Message);
                                        }
                                        else
                                        {
                                            BpLog.LogError("Got Unexpected Exception?!?" + task.Exception);
                                        }
                                    }
                                }
                                else if (task.IsCanceled)
                                {
                                    BpLog.LogError("Canceled");
                                }
                                else
                                {
                                    BpLog.Log("Welcome: " + task.Result.DisplayName + "!");
                                    BpLog.Log("Email = " + task.Result.Email);
                                    BpLog.Log("Google ID Token = " + task.Result.IdToken);
                                    BpLog.Log("Email = " + task.Result.Email);
                                    SignInFirebase(type, user => { }, task.Result.IdToken);

                                    LoginPlatform = LoginPlatformType.Google;
                                    platformToken = task.Result.IdToken;
                                    IsLogin = true;
                                    _actionQueue.Enqueue(() =>
                                    {
                                        PlayerPrefs.SetString(GoogleTokenKey, platformToken);
                                    });
                                }
                            });
                        }
                        break;
                    case LoginPlatformType.Apple:
                        {
                            if (PlayerPrefs.HasKey(AppleUserIdKey))
                            {
                                var storedAppleUserId = PlayerPrefs.GetString(AppleUserIdKey);
                                this._appleAuthManager.GetCredentialState(
                                    storedAppleUserId,
                                    state =>
                                    {
                                        switch (state)
                                        {
                                            // If it's authorized, login with that user id
                                            case CredentialState.Authorized:
                                                IsLogin = true;
                                                LoginPlatform = LoginPlatformType.Apple;
                                                return;

                                            // If it was revoked, or not found, we need a new sign in with apple attempt
                                            // Discard previous apple user id
                                            case CredentialState.Revoked:
                                            case CredentialState.NotFound:
                                                PlayerPrefs.DeleteKey(AppleUserIdKey);
                                                return;
                                        }
                                    },
                                    error =>
                                    {
                                        var authorizationErrorCode = error.GetAuthorizationErrorCode();
                                        BpLog.LogWarning("Error while trying to get credential state " + authorizationErrorCode.ToString() + " " + error.ToString());
                                    });
                            }
                            else
                            {
                                var rawNonce = GenerateRandomString(32);
                                var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

                                var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);
                                // Quick login should succeed if the credential was authorized before and not revoked
                                this._appleAuthManager.QuickLogin(
                                    quickLoginArgs,
                                    credential =>
                                    {
                                        // If it's an Apple credential, save the user ID, for later logins
                                        var appleIdCredential = credential as IAppleIDCredential;
                                        if (appleIdCredential != null)
                                        {
                                            var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                                            platformToken = identityToken;
                                            LoginPlatform = LoginPlatformType.Apple;
                                            IsLogin = true;
                                            _actionQueue.Enqueue(() =>
                                            {
                                                PlayerPrefs.SetString(AppleUserIdKey, credential.User);
                                                PlayerPrefs.SetString(AppleTokenKey, platformToken);
                                            });

                                            var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                                            SignInFirebase(LoginPlatformType.Apple, user => { }, identityToken, rawNonce, authorizationCode);
                                        }
                                    },
                                    error =>
                                    {
                                        // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                                        var authorizationErrorCode = error.GetAuthorizationErrorCode();
                                        BpLog.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                                    });
                            }
                        }
                        break;
                    case LoginPlatformType.Facebook:
                        {
                            if (GameRoot.Instance.PluginSystem.IsInitFacebook)
                            {
                                var perms = new List<string>() { "public_profile", "email" };
                                FB.LogInWithReadPermissions(perms, result =>
                                {
                                    if (FB.IsLoggedIn)
                                    {
                                        var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                                        BpLog.Log($"facebook user id : {aToken.UserId}");
                                        SignInFirebase(LoginPlatformType.Facebook, user => { }, aToken.TokenString);
                                    }
                                    else
                                    {
                                        BpLog.LogWarning("facebook Login Failed");
                                    }
                                });
                            }
                        }
                        break;
                }
            }
        }

        auth = FirebaseAuth.DefaultInstance;
        fUser = auth.CurrentUser;
    }

    Queue<Action> _actionQueue = new Queue<Action>();
    public void Update()
    {
        if (this._appleAuthManager != null)
        {
            this._appleAuthManager.Update();
        }

        while (_actionQueue.Count > 0)
        {
            _actionQueue.Dequeue().Invoke();
        }
    }

    public void Login(LoginPlatformType loginType, Action<FirebaseUser> firebaseUserCallback)
    {
        state = LoginState.Login;
        switch (loginType)
        {
            case LoginPlatformType.Google:
                {
                    GoogleSignIn.Configuration = configuration;
                    GoogleSignIn.Configuration.UseGameSignIn = false;
                    GoogleSignIn.Configuration.RequestIdToken = true;
                    GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
                            {
                                if (enumerator.MoveNext())
                                {
                                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                                    BpLog.LogError("Got Error: " + error.Status + " " + error.Message);
                                }
                                else
                                {
                                    BpLog.LogError("Got Unexpected Exception?!?" + task.Exception);
                                }
                            }
                            firebaseUserCallback(null);
                            state = LoginState.None;
                        }
                        else if (task.IsCanceled)
                        {
                            BpLog.LogError("Canceled");
                            firebaseUserCallback(null);
                            state = LoginState.None;
                        }
                        else
                        {
                            BpLog.Log("Welcome: " + task.Result.DisplayName + "!");
                            BpLog.Log("Email = " + task.Result.Email);
                            BpLog.Log("Google ID Token = " + task.Result.IdToken);
                            BpLog.Log("Email = " + task.Result.Email);
                            LoginPlatform = LoginPlatformType.Google;
                            IsLogin = true;
                            SignInFirebase(loginType, firebaseUserCallback, task.Result.IdToken);

                            platformToken = task.Result.IdToken;
                            _actionQueue.Enqueue(() =>
                            {
                                PlayerPrefs.SetString(GoogleTokenKey, platformToken);
                                PlayerPrefs.SetString(LoginPlatformKey, LoginPlatformType.Google.ToString());
                            });
                        }
                    });
                }
                break;
            case LoginPlatformType.Facebook:
                {
                    if (GameRoot.Instance.PluginSystem.IsInitFacebook)
                    {
                        var perms = new List<string>() { "public_profile", "email" };
                        FB.LogInWithReadPermissions(perms, result =>
                        {
                            if (FB.IsLoggedIn)
                            {
                                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                                BpLog.Log($"facebook user id : {aToken.UserId}");
                                SignInFirebase(LoginPlatformType.Facebook, firebaseUserCallback, aToken.TokenString);
                            }
                            else
                            {
                                BpLog.LogWarning("facebook Login Failed");
                            }

                            state = LoginState.None;
                        });
                    }
                }
                break;
            case LoginPlatformType.Apple:
                {
                    var rawNonce = GenerateRandomString(32);
                    var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

                    var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

                    this._appleAuthManager.LoginWithAppleId(
                        loginArgs,
                        credential =>
                        {
                            BpLog.LogWarning("appleconnect ");
                            // If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
                            var appleIdCredential = credential as IAppleIDCredential;
                            if (appleIdCredential != null)
                            {
                                var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                                var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                                SignInFirebase(loginType, firebaseUserCallback, identityToken, rawNonce, authorizationCode);

                                platformToken = identityToken;
                                LoginPlatform = LoginPlatformType.Apple;
                                IsLogin = true;
                                _actionQueue.Enqueue(() =>
                                {
                                    PlayerPrefs.SetString(AppleUserIdKey, credential.User);
                                    PlayerPrefs.SetString(LoginPlatformKey, LoginPlatformType.Apple.ToString());
                                    PlayerPrefs.SetString(AppleTokenKey, platformToken);
                                });
                            }
                        },
                        error =>
                        {
                            var authorizationErrorCode = error.GetAuthorizationErrorCode();
                            BpLog.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                            firebaseUserCallback(null);
                            state = LoginState.None;
                        });
                }
                break;
        }
    }

    private void SignInFirebase(LoginPlatformType loginType, Action<FirebaseUser> firebaseAuthCallback, params object[] args)
    {
        if (auth == null)
        {
            BpLog.LogError("Firebase auth is null! Cannot sign in.");
            firebaseAuthCallback(null);
            state = LoginState.None;
            return;
        }

        Credential credential = null;
        var idToken = args[0] as string;
        
        if (string.IsNullOrEmpty(idToken))
        {
            BpLog.LogError("IdToken is null or empty!");
            firebaseAuthCallback(null);
            state = LoginState.None;
            return;
        }

        switch (loginType)
        {
            case LoginPlatformType.Google:
                credential = GoogleAuthProvider.GetCredential(idToken, null);
                break;
            case LoginPlatformType.Facebook:
                credential = FacebookAuthProvider.GetCredential(idToken);
                break;
            case LoginPlatformType.Apple:
                var rawNonce = args[1] as string;
                var authorizationCode = args[2] as string;
                credential = OAuthProvider.GetCredential("apple.com", idToken, rawNonce, authorizationCode);

                break;
        }


        BpLog.Log($"curauth:{auth.CurrentUser}");
        BpLog.Log($"Starting Firebase SignIn with credential for {loginType}");

        auth.
        SignInWithCredentialAsync(credential)
        .ContinueWithOnMainThread(task => HandleSignInWithUser(task, firebaseAuthCallback));
    }

    private static void HandleSignInWithUser(Task<FirebaseUser> task, Action<FirebaseUser> firebaseUserCallback)
    {
        Debug.Log("handlesgin with");
        fUser = null;
        if (task.IsCanceled)
        {
            BpLog.LogError("Firebase auth was canceled");
            firebaseUserCallback(null);
        }
        else if (task.IsFaulted)
        {
            BpLog.LogError("Firebase auth failed");
            if (task.Exception != null)
            {
                BpLog.LogError($"Firebase auth exception: {task.Exception}");
                if (task.Exception.InnerException != null)
                {
                    BpLog.LogError($"Firebase auth inner exception: {task.Exception.InnerException}");
                }
            }
            firebaseUserCallback(null);
        }
        else
        {
            fUser = task.Result;
            if (fUser != null)
            {
                BpLog.Log("Firebase auth completed | User ID:" + fUser.UserId);
                firebaseUserCallback(fUser);
            }
            else
            {
                BpLog.LogError("Firebase auth completed but fUser is null");
                firebaseUserCallback(null);
            }
        }

        state = LoginState.None;
    }

    public void Logout()
    {
        auth.SignOut();
        fUser = null;

        IsLogin = false;
        state = LoginState.None;
        LoginPlatform = LoginPlatformType.None;
    }

    ////apple login
    private static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }

    public string GetToken()
    {
        if (!IsLogin)
        {
            Debug.Log("is not login");
            return string.Empty;
        }

        var result = string.Empty;

        Debug.Log("login platform : " + LoginPlatform.ToString());

        if (LoginPlatform == LoginPlatformType.Google)
        {
            if (PlayerPrefs.HasKey(GoogleTokenKey))
            {
                result = PlayerPrefs.GetString(GoogleTokenKey);
            }
            else
            {
                Debug.Log("google token is null");
                result = platformToken;
            }
        }
        else if (LoginPlatform == LoginPlatformType.Apple)
        {
            if (PlayerPrefs.HasKey(AppleTokenKey))
            {
                result = PlayerPrefs.GetString(AppleTokenKey);
            }
            else
            {
                Debug.Log("apple token is null");
                result = platformToken;
            }
        }

        return result;
    }
}
