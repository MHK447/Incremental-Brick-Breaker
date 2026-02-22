#import "BanpoFriNative.h"
#import <Foundation/Foundation.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

@implementation BanpoFriNative

+(void)callReview{
    if(@available(iOS 10.3, *)){
        [SKStoreReviewController requestReview];
    }
}

+(void)openURL:(char *)urlString {
    NSString *nsUrlString = [NSString stringWithUTF8String:urlString];

    // 문자열을 NSURL 객체로 변환
    NSURL *url = [NSURL URLWithString:nsUrlString];
    if (!url) {
        //NSLog(@"잘못된 URL 형식입니다: %@", urlString);
        return;
    }
    
    // URL 열 수 있는지 확인
    if (![[UIApplication sharedApplication] canOpenURL:url]) {
        //NSLog(@"이 URL을 열 수 없습니다: %@", urlString);
        return;
    }
    
    // iOS 버전에 따라 분기 처리
    if (@available(iOS 10.0, *)) {
        [[UIApplication sharedApplication] openURL:url 
                                           options:@{} 
                                 completionHandler:^(BOOL success) {
            if (success) {
                //NSLog(@"URL 열기 성공: %@", urlString);
            } else {
                //NSLog(@"URL 열기 실패: %@", urlString);
            }
        }];
    } else {
        // iOS 9 이하 대응
        BOOL success = [[UIApplication sharedApplication] openURL:url];
        if (success) {
            //NSLog(@"(iOS 9 이하) URL 열기 성공: %@", urlString);
        } else {
            //NSLog(@"(iOS 9 이하) URL 열기 실패: %@", urlString);
        }
    }
}

+(char *)getLanguage
{
    NSString* lang = [[NSLocale preferredLanguages] firstObject];
    
    NSDictionary *languageDic = [NSLocale componentsFromLocaleIdentifier:lang];
    NSString *languageCode = [languageDic objectForKey:@"kCFLocaleLanguageCodeKey"];

    if([languageCode rangeOfString:@"zh"].location != NSNotFound){
        if([lang rangeOfString:@"Hant"].location != NSNotFound){
            languageCode = @"tw";
        }
    }
    
    const char* nsStringUtf8 = [languageCode UTF8String];
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);
    
    return cString;
}

+(char *)getCountry
{
    NSString *countryCode = [[NSLocale currentLocale] objectForKey:NSLocaleCountryCode];

    const char* nsStringUtf8 = [countryCode UTF8String];
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);
    
    return cString;
}

@end
extern "C" {
    const void IOSCallReview()
    {
        [BanpoFriNative callReview];
    }

    const void IOSOpenURL(char* url)
    {
        [BanpoFriNative openURL:url];
    }

    const void vibrate()
    {
        // 1520: 중간 세기, 1521: 강한 세기
        AudioServicesPlaySystemSound(1520);
    }
}
extern "C" char* getDeviceLanguage(){
    return [BanpoFriNative getLanguage];
}

extern "C" char* getDeviceCountry(){
    return [BanpoFriNative getCountry];
}
/*extern "C" {
    void _FBAdSetting_setAdvertiserTrackingEnabled() {
        
        if (@available(iOS 14.5, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                NSString* result = @"";
                
                switch (status) {
                    case ATTrackingManagerAuthorizationStatusAuthorized: result = @"ATTrackingManagerAuthorizationStatusAuthorized"; break;
                    case ATTrackingManagerAuthorizationStatusDenied: result =  @"ATTrackingManagerAuthorizationStatusDenied"; break;
                    case ATTrackingManagerAuthorizationStatusRestricted : result =  @"ATTrackingManagerAuthorizationStatusDenied" ; break;
                    case ATTrackingManagerAuthorizationStatusNotDetermined : result = @"ATTrackingManagerAuthorizationStatusDenied"; break;
                }
                
                NSLog(@"[FacebookAdSettings] status = %@", result );
                [FBAdSettings setAdvertiserTrackingEnabled: status == ATTrackingManagerAuthorizationStatusAuthorized ? YES : NO];
                
            }];
        } else {
            // Fallback on earlier versions
        }
        
    }
}
extern "C" {
    void _FBAdSetting_setAdvertiserEnabled(bool value) {
        
        if (@available(iOS 14.5, *)) {
            if(value == true)
            {
                [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                    NSString* result = @"";
                    
                    switch (status) {
                        case ATTrackingManagerAuthorizationStatusAuthorized: result = @"ATTrackingManagerAuthorizationStatusAuthorized"; break;
                        case ATTrackingManagerAuthorizationStatusDenied: result =  @"ATTrackingManagerAuthorizationStatusDenied"; break;
                        case ATTrackingManagerAuthorizationStatusRestricted : result =  @"ATTrackingManagerAuthorizationStatusDenied" ; break;
                        case ATTrackingManagerAuthorizationStatusNotDetermined : result = @"ATTrackingManagerAuthorizationStatusDenied"; break;
                    }
                    
                    NSLog(@"[FacebookAdSettings] status = %@", result );
                    [FBAdSettings setAdvertiserTrackingEnabled: status == ATTrackingManagerAuthorizationStatusAuthorized ? YES : NO];
                    
                }];
            }else{
                [FBAdSettings setAdvertiserTrackingEnabled: NO];
            }
        }else{
            
        }
    }
}

extern "C" {
    void _Goto_TrackingSetting(){
        NSLog(@"==================Open Setting ===============");
        NSURL *url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL:url options:@{}completionHandler:nil];
    }
}

extern "C" int _GetATTStatus()
{
    if (@available(iOS 14, *)) {
        if([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]) {
            return 0;
        }else{
            return -1;
        }
    }else{
        return 0;
    }
}*/
