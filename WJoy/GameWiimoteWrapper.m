#import "GameWiimoteWrapper.h"
#import "Wiimote.h"
#import "TestController.h"
#import "Communicator.h"
#import "JSONKit.h"

@implementation GameWiimoteWrapper {
	Wiimote *m_Device;
	id<GameWiimoteWrapperDelegate> _controller;
	int _device_id;
}

static int __remote_id_alloc = 0;
-(int)get_id{ return _device_id; }

-(id)initWithWiimote:(Wiimote*)wiimote controller:(id<GameWiimoteWrapperDelegate>)controller led:(WiimoteLEDFlag)led {
    self = [super init];
    if(self == nil) return nil;

	_device_id = __remote_id_alloc++;

	_controller = controller;

    m_Device = wiimote;
    [m_Device setDelegate:self];
	[m_Device playRumbleEffect:0.35f];
    [m_Device setHighlightedLEDMask:led];
	
	
	[[m_Device accelerometer] setEnabled:YES];
    [m_Device setIREnabled:YES];
	[m_Device detectMotionPlus];

	[[NSNotificationCenter defaultCenter]
								addObserver:self
								   selector:@selector(applicationWillTerminateNotification:)
									   name:NSApplicationWillTerminateNotification
									 object:[NSApplication sharedApplication]];
	
    return self;
}

-(BOOL)is_wrapper_for_wiimote:(Wiimote*)wiimote {
	return m_Device == wiimote;
}

-(void)applicationWillTerminateNotification:(NSNotification*)notification {
	[m_Device disconnect];
	m_Device = NULL;
}

-(void)socket_message_send:(NSDictionary*)dict {
	NSMutableDictionary *jsout = [NSMutableDictionary dictionaryWithDictionary:dict];
	jsout[@"id"] = [NSNumber numberWithInt:_device_id];
	
	NSString *jsout_str = [jsout JSONString];
	if ([_controller get_output_motion_data]) {
		[_controller log:jsout_str];
	}
	[[_controller get_socket] writeOut:jsout_str];
}

-(void)wiimote:(Wiimote*)wiimote buttonPressed:(WiimoteButtonType)button{
	NSString *btn_val = NULL;
	switch (button) {
		case WiimoteButtonTypeA:
			btn_val = @"A";
			break;
		case WiimoteButtonTypeB:
			btn_val = @"B";
			break;
		case WiimoteButtonTypeLeft:
			btn_val = @"DL";
			break;
		case WiimoteButtonTypeRight:
			btn_val = @"DR";
			break;
		case WiimoteButtonTypeUp:
			btn_val = @"DU";
			break;
		case WiimoteButtonTypeDown:
			btn_val = @"DD";
			break;
		case WiimoteButtonTypeMinus:
			btn_val = @"-";
			break;
		case WiimoteButtonTypePlus:
			btn_val = @"+";
			break;
		case WiimoteButtonTypeHome:
			btn_val = @"H";
			break;
		case WiimoteButtonTypeOne:
			btn_val = @"1";
			break;
		case WiimoteButtonTypeTwo:
			btn_val = @"2";
			break;
		default:
			break;
	}
	if (btn_val != NULL) {
		[self socket_message_send:@{
			@"t" : @"bp",
			@"b" : btn_val
		}];
	}
}
-(void)wiimote:(Wiimote*)wiimote buttonReleased:(WiimoteButtonType)button{
	NSString *btn_val = NULL;
	switch (button) {
		case WiimoteButtonTypeA:
			btn_val = @"A";
			break;
		case WiimoteButtonTypeB:
			btn_val = @"B";
			break;
		case WiimoteButtonTypeLeft:
			btn_val = @"DL";
			break;
		case WiimoteButtonTypeRight:
			btn_val = @"DR";
			break;
		case WiimoteButtonTypeUp:
			btn_val = @"DU";
			break;
		case WiimoteButtonTypeDown:
			btn_val = @"DD";
			break;
		case WiimoteButtonTypeMinus:
			btn_val = @"-";
			break;
		case WiimoteButtonTypePlus:
			btn_val = @"+";
			break;
		case WiimoteButtonTypeHome:
			btn_val = @"H";
			break;
		case WiimoteButtonTypeOne:
			btn_val = @"1";
			break;
		case WiimoteButtonTypeTwo:
			btn_val = @"2";
			break;
		default:
			break;
	}
	if (btn_val != NULL) {
		[self socket_message_send:@{
			@"t" : @"b",
			@"b" : btn_val
		}];
	}
}
-(void)wiimote:(Wiimote*)wiimote accelerometerChangedGravityX:(double)x y:(double)y z:(double)z{}
-(void)wiimote:(Wiimote*)wiimote accelerometerChangedPitch:(double)pitch roll:(double)roll {
	[self socket_message_send:@{
		@"t" : @"a",
		@"p" : [NSNumber numberWithFloat:pitch],
		@"r" : [NSNumber numberWithFloat:roll]
	}];
}

-(void)wiimote:(Wiimote*)wiimote motionPlus:(WiimoteMotionPlusExtension*)motionPlus report:(const WiimoteMotionPlusReport*)report {
	[self socket_message_send:@{
		@"t" : @"m",
		@"id" : [NSNumber numberWithInt:_device_id],
		@"r" : [NSNumber numberWithUnsignedInt:report->roll.speed],
		@"rs" : [NSNumber numberWithBool:report->roll.isSlowMode],
		@"p" : [NSNumber numberWithUnsignedInt:report->pitch.speed],
		@"ps" : [NSNumber numberWithBool:report->pitch.isSlowMode],
		@"y" : [NSNumber numberWithUnsignedInt:report->yaw.speed],
		@"ys" : [NSNumber numberWithBool:report->yaw.isSlowMode]
	}];
}

-(void)send_connection_message {
	[self socket_message_send:@{
		@"t" : @"c"
	}];
}

-(void)send_disconnection_message {
	[self socket_message_send:@{
		@"t" : @"d",
	}];
}

-(void)wiimote:(Wiimote*)wiimote irPointPositionChanged:(WiimoteIRPoint*)point {
	[self socket_message_send:@{
		@"t" : @"i",
		@"px" : [NSNumber numberWithInt:(int)[point position].x],
		@"py" : [NSNumber numberWithInt:(int)[point position].y],
		@"ov" : [NSNumber numberWithBool:[point isOutOfView]],
		@"in" : [NSNumber numberWithInt:[point index]]
	}];
}

-(void)wiimote:(Wiimote*)wiimote vibrationStateChanged:(BOOL)isVibrationEnabled{}
-(void)wiimote:(Wiimote*)wiimote highlightedLEDMaskChanged:(NSUInteger)mask{}
-(void)wiimote:(Wiimote*)wiimote batteryLevelUpdated:(double)batteryLevel isLow:(BOOL)isLow{}
-(void)wiimote:(Wiimote*)wiimote extensionConnected:(WiimoteExtension*)extension{}
-(void)wiimote:(Wiimote*)wiimote extensionDisconnected:(WiimoteExtension*)extension{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck buttonPressed:(WiimoteNunchuckButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck buttonReleased:(WiimoteNunchuckButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck stickPositionChanged:(NSPoint)position{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck accelerometerEnabledStateChanged:(BOOL)enabled{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck accelerometerChangedGravityX:(double)x y:(double)y z:(double)z{}
-(void)wiimote:(Wiimote*)wiimote nunchuck:(WiimoteNunchuckExtension*)nunchuck accelerometerChangedPitch:(double)pitch roll:(double)roll{}
-(void)wiimote:(Wiimote*)wiimote classicController:(WiimoteClassicControllerExtension*)classic buttonPressed:(WiimoteClassicControllerButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote classicController:(WiimoteClassicControllerExtension*)classic buttonReleased:(WiimoteClassicControllerButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote classicController:(WiimoteClassicControllerExtension*)classic stick:(WiimoteClassicControllerStickType)stick positionChanged:(NSPoint)position{}
-(void)wiimote:(Wiimote*)wiimote classicController:(WiimoteClassicControllerExtension*)classic analogShift:(WiimoteClassicControllerAnalogShiftType)shift positionChanged:(float)position{}
-(void)wiimote:(Wiimote*)wiimote uProController:(WiimoteUProControllerExtension*)uPro buttonPressed:(WiimoteUProControllerButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote uProController:(WiimoteUProControllerExtension*)uPro buttonReleased:(WiimoteUProControllerButtonType)button{}
-(void)wiimote:(Wiimote*)wiimote uProController:(WiimoteUProControllerExtension*)uPro stick:(WiimoteUProControllerStickType)stick positionChanged:(NSPoint)position{}
-(void)wiimoteDisconnected:(Wiimote*)wiimote{}
-(void)wiimote:(Wiimote*)wiimote accelerometerEnabledStateChanged:(BOOL)enabled{}
-(void)wiimote:(Wiimote*)wiimote irEnabledStateChanged:(BOOL)enabled{}
-(void)wiimote:(Wiimote*)wiimote motionPlus:(WiimoteMotionPlusExtension*)motionPlus subExtensionConnected:(WiimoteExtension*)extension{}
-(void)wiimote:(Wiimote*)wiimote motionPlus:(WiimoteMotionPlusExtension*)motionPlus subExtensionDisconnected:(WiimoteExtension*)extension {}

-(void)rumble:(float)duration {
	[m_Device playRumbleEffect:duration];
}

@end
