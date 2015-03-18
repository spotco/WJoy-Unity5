//
//  TestController.h
//  Wiimote
//
//  Created by alxn1 on 09.10.12.
//  Copyright 2012 alxn1. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "Delegates.h"
@class Communicator;

@interface TestController : NSObject <CommunicatorDelegate,GameWiimoteWrapperDelegate>
{
    @private
        IBOutlet NSTextView     *m_Log;
        IBOutlet NSButton       *m_DiscoveryButton;
        IBOutlet NSTextField    *m_ConnectedTextField;
}

-(IBAction)toggle_output_motion_data:(id)sender;
-(IBAction)discovery:(id)sender;
-(IBAction)clearLog:(id)sender;
-(IBAction)unityReconnect:(id)sender;

@end
