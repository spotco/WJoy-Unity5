#import <Foundation/Foundation.h>
#import "Delegates.h"
@interface Communicator : NSObject <NSStreamDelegate> {
	@public
	
	NSString *host;
	int port;
}

- (void)setup:(id<CommunicatorDelegate>)delegate;
- (void)open;
- (void)close;
- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)event;
- (void)readIn:(NSString *)s;
- (void)writeOut:(NSString *)s;

@end
