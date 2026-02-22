#import<StoreKit/StoreKit.h>
#import <UIKit/UIKit.h>
#import <AudioToolBox/AudioToolBox.h>
@interface BanpoFriNative : NSObject
+(void)callReview;
+(void)openURL:(char *)urlString;
+(char *)getLanguage;
+(char *)getCountry;
@end
