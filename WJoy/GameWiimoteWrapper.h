//
//  GameWiimoteWrapper.h
//  Wiimote
//
//  Created by spotco on 15/02/2015.
//
//

#import <Foundation/Foundation.h>
#import "Delegates.h" 
#import "Wiimote.h" 
@class Wiimote;
@class TestController;
@class Communicator;


@interface GameWiimoteWrapper : NSObject
-(id)initWithWiimote:(Wiimote*)wiimote controller:(id<GameWiimoteWrapperDelegate>)controller led:(WiimoteLEDFlag)led;
-(int)get_id;
-(BOOL)is_wrapper_for_wiimote:(Wiimote*)wiimote;
-(void)send_connection_message;
-(void)send_disconnection_message;
-(void)rumble:(float)duration;
@end
