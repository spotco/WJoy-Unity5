#import "TestController.h"
#import "GameWiimoteWrapper.h"
#import "Communicator.h"
#import <Wiimote/Wiimote.h>
#import "JSONKit.h" 

@implementation TestController {
	NSMutableArray *_connected_wiimotes;
	BOOL _output_motion_data;
	Communicator *_socket;
}

-(Communicator*) get_socket { return _socket; }

- (void)awakeFromNib {
	_connected_wiimotes = [[NSMutableArray alloc] init];
	_output_motion_data = NO;
	
    [[NSNotificationCenter defaultCenter]
                                    addObserver:self
                                       selector:@selector(discoveryBegin)
                                           name:WiimoteBeginDiscoveryNotification
                                         object:nil];
	
    [[NSNotificationCenter defaultCenter]
                                    addObserver:self
                                       selector:@selector(discoveryEnd)
                                           name:WiimoteEndDiscoveryNotification
                                         object:nil];
	
	[[NSNotificationCenter defaultCenter]
							addObserver:self
							   selector:@selector(newDeviceConnected:)
								   name:WiimoteConnectedNotification
								 object:nil];
	
	[[NSNotificationCenter defaultCenter]
							addObserver:self
							   selector:@selector(deviceDisconnected:)
								   name:WiimoteDisconnectedNotification
								 object:nil];

	[[WiimoteWatchdog sharedWatchdog] setEnabled:YES];
    [self updateWindowState];
	
	[self unityReconnect:NULL];
}

-(IBAction)unityReconnect:(id)sender {
	if (_socket != NULL) {
		[_socket close];
		[_socket dealloc];
	}
	_unity_connected = NO;
	[self log:@"attempting to connect to unity..."];
	_socket = [[Communicator alloc] init];
	_socket->host = @"127.0.0.1";
	_socket->port = 7001;
	[_socket setup:self];
	[_socket open];
}

static bool _unity_connected = NO;
-(void)message_read:(NSString*)str {
	NSDictionary *json_in = [str objectFromJSONString];
	if (streq([json_in objectForKey:@"action"], @"unity_connect")) {
		[self log:@"connected to unity"];
		for (GameWiimoteWrapper *itr in _connected_wiimotes) {
			[itr send_connection_message];
		}
		_unity_connected = YES;
		[self updateWindowState];
		
	} else if (streq([json_in objectForKey:@"action"],@"unity_disconnect")) {
		[self log:@"unity disconnected"];
		_unity_connected = NO;
		[self updateWindowState];
		
	} else if (streq([json_in objectForKey:@"action"],@"find_wiimote")) {
		[self discovery:NULL];
		
	} else if (streq([json_in objectForKey:@"action"],@"rumble")) {
		int tid = ((NSString*)[json_in objectForKey:@"id"]).intValue;
		float time = ((NSString*)[json_in objectForKey:@"duration"]).floatValue;
		for (GameWiimoteWrapper *itr in _connected_wiimotes) {
			if ([itr get_id] == tid) [itr rumble:time];
		}
	}
}

static bool _discovering = NO;
-(void)set_discovering:(BOOL)val {
	_discovering = val;
	[self updateWindowState];
}

- (void)updateWindowState {
	[m_DiscoveryButton setEnabled:!_discovering];
	[m_ConnectedTextField setStringValue:[NSString stringWithFormat:@"unity(%d) wiimotes(%d) discover(%d)",_unity_connected,(int)[_connected_wiimotes count],_discovering]];
}

-(IBAction)toggle_output_motion_data:(id)sender {
	_output_motion_data = ([sender state] == NSOnState);
}
-(BOOL)get_output_motion_data { return _output_motion_data; }

- (IBAction)discovery:(id)sender {
    [Wiimote beginDiscovery];
}

-(void)discoveryBegin {
	[self log:@"discovery begin"];
	[self set_discovering:YES];
}

-(void)discoveryEnd {
	[self log:@"discovery end"];
	[self set_discovering:NO];
}

-(void)newDeviceConnected:(NSNotification*)notification {
    Wiimote *target = [notification object];
	GameWiimoteWrapper *new_wiimote = [[GameWiimoteWrapper alloc] initWithWiimote:target controller:self led:_connected_wiimotes.count == 0?WiimoteLEDFlagOne:WiimoteLEDFlagTwo];
	[new_wiimote send_connection_message];
	[_connected_wiimotes addObject:new_wiimote];
	[self log:[NSString stringWithFormat:@"new device connected %d",[new_wiimote get_id]]];
	[self updateWindowState];
}

-(void)deviceDisconnected:(NSNotification*)notification {
    Wiimote *target = [notification object];
	NSMutableArray *to_remove = [NSMutableArray array];
	for (GameWiimoteWrapper *itr in _connected_wiimotes) {
		if ([itr is_wrapper_for_wiimote:target]) {
			[itr send_disconnection_message];
			[to_remove addObject:itr];
			[self log:[NSString stringWithFormat:@"device disconnected %d",[itr get_id]]];
			[itr autorelease];
		}
	}
	[_connected_wiimotes removeObjectsInArray:to_remove];
	[to_remove removeAllObjects];
	[self updateWindowState];
}

- (IBAction)clearLog:(id)sender {
	[m_Log setString:@""];
}

-(void)log:(NSString*)str {
	[m_Log setString:[NSString stringWithFormat:@"%@\n%@",str,[m_Log string]]];
}

@end
