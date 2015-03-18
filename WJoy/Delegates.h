//
//  Delegates.h
//  Wiimote
//
//  Created by spotco on 19/02/2015.
//
//

#import <Foundation/Foundation.h>
@class Wiimote;
@class TestController;
@class Communicator;

@protocol GameWiimoteWrapperDelegate <NSObject>
-(void)log:(NSString*)str;
-(BOOL)get_output_motion_data;
-(Communicator*)get_socket;
@end

@protocol CommunicatorDelegate <NSObject>
-(void)message_read:(NSString*)str;
@end

#define streq(a,b) [a isEqualToString:b]