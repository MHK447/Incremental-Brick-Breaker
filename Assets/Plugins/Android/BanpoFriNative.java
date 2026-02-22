package com.unity3d.player;
import com.unity3d.player.UnityPlayer;
import android.util.Log;
import android.content.Intent;
import android.content.Context;
import java.util.Locale;
import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import java.util.Calendar;
import android.os.Build;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.graphics.Color;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.content.pm.SigningInfo;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import android.os.PowerManager;
import android.content.IntentFilter;
import android.os.BatteryManager;

public class BanpoFriNative {
    private static NotificationChannel notificationChannel;
    //get Android Country Code
    public static String nativeGetLanguage(){
        String str = "";
        Locale locale = UnityPlayer.currentActivity.getResources().getConfiguration().locale;
        str = locale.getLanguage();


        Log.d("iso Locale getLanguage", locale.getLanguage());
        Log.d("iso Locale getCountry",locale.getCountry());

        if(str.equals("zh")){
            String country = locale.getCountry();
            if(country.toLowerCase().contains("tw")) return "tw";
        }

        return str;
    }

    public static String nativeGetCountry(){
        String str = "";
        Locale locale = UnityPlayer.currentActivity.getResources().getConfiguration().locale;
        str = locale.getCountry();

        Log.d("iso Locale getCountry",str);

        return str;
    }

    public static void createNotificationChannel()
    {
        // if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.O){
        //     NotificationManager notificationManager = (NotificationManager)UnityPlayer.currentActivity.getSystemService(UnityPlayer.currentActivity.NOTIFICATION_SERVICE);
        //     notificationChannel = new NotificationChannel(
        //             "BanpoFri Notifi Channel",
        //             "BanpoFri Channel",
        //             NotificationManager.IMPORTANCE_DEFAULT
        //     );

        //     notificationChannel.setDescription("BanpoFri Push Notification");
        //     notificationChannel.enableLights(true);
        //     notificationChannel.setLightColor(Color.GREEN);
        //     notificationChannel.enableVibration(true);
        //     notificationChannel.setVibrationPattern(new long[]{100, 200, 100, 200});
        //     notificationChannel.setLockscreenVisibility(Notification.VISIBILITY_PRIVATE);
        //     notificationManager.createNotificationChannel(notificationChannel);

        // }
    }

    public static void callNotification(int notiId, String notiTitle, String notiDesc, int timeTerm)
    {
        // Log.d("LOCAL_PUSH_TEST", "SendLocalNotifiaction : " + notiId + "- after " + timeTerm);
        // Calendar updateTime = Calendar.getInstance();
        // long futureInMillis = updateTime.getTimeInMillis() + (timeTerm * 1000);
        // Intent intent = new Intent(UnityPlayer.currentActivity.getApplicationContext(), LocalNotificationPublisher.class);
        // intent.putExtra(LocalNotificationPublisher.NOTIFICATION_ID, notiId);
        // intent.putExtra(LocalNotificationPublisher.NOTIFICATION_Msg, notiDesc);
        // intent.putExtra(LocalNotificationPublisher.NOTIFICATION_Title, notiTitle);
        // PendingIntent p1 = PendingIntent.getBroadcast(UnityPlayer.currentActivity.getApplicationContext(), notiId, intent, PendingIntent.FLAG_UPDATE_CURRENT);
        // AlarmManager a = (AlarmManager) UnityPlayer.currentActivity.getSystemService(Context.ALARM_SERVICE);
        // if(Build.VERSION.SDK_INT >= 23){
        //     a.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, futureInMillis, p1);
        // } else if (Build.VERSION.SDK_INT >= 19) {
        //     a.setExact(AlarmManager.RTC_WAKEUP, futureInMillis, p1);
        // } else {
        //     a.set(AlarmManager.RTC_WAKEUP, futureInMillis, p1);
        // }
    }

    public static void clearNotification(int[] push_unique_ids)
    {
        // AlarmManager alarmManager = (AlarmManager) UnityPlayer.currentActivity.getSystemService(Context.ALARM_SERVICE);
        // for(int i = 0; i < push_unique_ids.length; ++i){
        //     PendingIntent pendingIntent = PendingIntent.getBroadcast(UnityPlayer.currentActivity.getApplicationContext(), push_unique_ids[i], new Intent(UnityPlayer.currentActivity.getApplicationContext(), LocalNotificationPublisher.class), 0);
        //     alarmManager.cancel(pendingIntent);
        // }
        // NotificationManager nMgr = (NotificationManager) UnityPlayer.currentActivity.getSystemService(Context.NOTIFICATION_SERVICE);
        // nMgr.cancelAll();
    }

    public static void Exit()
    {
        UnityPlayer.currentActivity.finishAffinity();
        java.lang.System.exit(0);
    }

    public static void vibrate(int milliseconds)
    {
        Vibrator v = (Vibrator) UnityPlayer.currentActivity.getSystemService(Context.VIBRATOR_SERVICE);
        if(v == null)
            return;
        
        v.cancel();
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q)
        {
            // EFFECT_HEAVY_CLICK로 변경하여 강한 진동 생성
            VibrationEffect vibrationEffect = VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK);
            v.vibrate(vibrationEffect);
        }
        else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) 
        {
            // 진동 시간을 50ms로 늘리고 최대 세기(255) 사용
            v.vibrate(VibrationEffect.createOneShot(50, 255));
        } 
        else 
        {
            //deprecated in API 26 
            // 진동 시간을 50ms로 늘림
            v.vibrate(50);
        }
    }

    public static String getCertificateFingerprint(Context context) 
    {
        String TAG = "CertificateFingerprint";
        try {
            // 패키지 매니저 가져오기
            PackageManager pm = context.getPackageManager();
            String packageName = context.getPackageName();

            // API 버전에 따라 플래그 선택
            int flags = (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) 
                ? PackageManager.GET_SIGNING_CERTIFICATES 
                : PackageManager.GET_SIGNATURES;

            // 패키지 정보 가져오기
            PackageInfo packageInfo = pm.getPackageInfo(packageName, flags);

            // 서명 정보 추출
            Signature[] signatures;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                SigningInfo signingInfo = packageInfo.signingInfo;
                if (signingInfo.hasMultipleSigners()) {
                    signatures = signingInfo.getApkContentsSigners();
                } else {
                    signatures = signingInfo.getSigningCertificateHistory();
                }
            } else {
                signatures = packageInfo.signatures;
            }

            if (signatures == null || signatures.length == 0) {
                Log.e(TAG, "No signatures found");
                return null;
            }

            // 첫 번째 서명으로 SHA1 계산
            byte[] cert = signatures[0].toByteArray();
            MessageDigest md = MessageDigest.getInstance("SHA1");
            md.update(cert);
            byte[] digest = md.digest();

            // 16진수 문자열로 변환
            StringBuilder hexString = new StringBuilder();
            for (byte b : digest) {
                String hex = Integer.toHexString(0xFF & b);
                if (hex.length() == 1) {
                    hexString.append('0');
                }
                hexString.append(hex);
            }

            // 콜론으로 구분된 형식으로 변환 (선택사항)
            String fingerprint = hexString.toString().toUpperCase();
            fingerprint = fingerprint.replaceAll("(?<=..)(..)", ":$1");
            return fingerprint;

        } catch (PackageManager.NameNotFoundException e) {
            Log.e(TAG, "Package not found", e);
            return null;
        } catch (NoSuchAlgorithmException e) {
            Log.e(TAG, "SHA1 algorithm not found", e);
            return null;
        }
    }
    
    public static int getThermalStatus() 
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) 
        { 
            PowerManager powerManager = (PowerManager) UnityPlayer.currentActivity.getSystemService(Context.POWER_SERVICE);
            return powerManager.getCurrentThermalStatus();
        }
        return -1; // 지원되지 않는 Android 버전
    }
    
    public static String getThermalStatusString() 
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) 
        {
            PowerManager powerManager = (PowerManager) UnityPlayer.currentActivity.getSystemService(Context.POWER_SERVICE);
            int status = powerManager.getCurrentThermalStatus();
            
            switch (status) {
                case PowerManager.THERMAL_STATUS_NONE:
                    return "NONE";
                case PowerManager.THERMAL_STATUS_LIGHT:
                    return "LIGHT";
                case PowerManager.THERMAL_STATUS_MODERATE:
                    return "MODERATE";
                case PowerManager.THERMAL_STATUS_SEVERE:
                    return "SEVERE";
                case PowerManager.THERMAL_STATUS_CRITICAL:
                    return "CRITICAL";
                case PowerManager.THERMAL_STATUS_EMERGENCY:
                    return "EMERGENCY";
                case PowerManager.THERMAL_STATUS_SHUTDOWN:
                    return "SHUTDOWN";
                default:
                    return "UNKNOWN";
            }
        }
        return "UNSUPPORTED"; // 지원되지 않는 Android 버전
    }
   
    public static float getBatteryTemperature() 
    {
        try 
        {
            IntentFilter ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
            Intent batteryStatus = UnityPlayer.currentActivity.registerReceiver(null, ifilter);
            
            int temperature = batteryStatus.getIntExtra(BatteryManager.EXTRA_TEMPERATURE, 0);
            return temperature;
        } 
        catch (Exception e) 
        {
            e.printStackTrace();
        }
        return 0.0f;
    }
}
