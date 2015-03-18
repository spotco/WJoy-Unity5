#import "Communicator.h"

@implementation Communicator {
	CFReadStreamRef readStream;
	CFWriteStreamRef writeStream;

	NSInputStream *inputStream;
	NSOutputStream *outputStream;
	
	id<CommunicatorDelegate> _delegate;
}

- (void)setup:(id<CommunicatorDelegate>)delegate {
	_delegate = delegate;
	CFStreamCreatePairWithSocketToHost(kCFAllocatorDefault, (CFStringRef)self->host, self->port, &readStream, &writeStream);
	
	if(!CFWriteStreamOpen(writeStream)) {
		NSLog(@"Error, writeStream not open");
		return;
	}
	[self open];
	return;
}

- (void)open {
	inputStream = (NSInputStream *)readStream;
	outputStream = (NSOutputStream *)writeStream;
	
	[inputStream retain];
	[outputStream retain];
	
	[inputStream setDelegate:self];
	[outputStream setDelegate:self];
	
	[inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	[outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	
	[inputStream open];
	[outputStream open];
}

- (void)close {
	[inputStream close];
	[outputStream close];
	
	[inputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	[outputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	
	[inputStream setDelegate:nil];
	[outputStream setDelegate:nil];
	
	[inputStream release];
	[outputStream release];
	
	inputStream = nil;
	outputStream = nil;
	_delegate = NULL;
}

- (void)stream:(NSStream *)stream handleEvent:(NSStreamEvent)event {
	switch(event) {
		case NSStreamEventHasBytesAvailable: {
			if(stream == inputStream) {
				uint8_t buf[1024];
				unsigned int len = 0;
				
				len = [inputStream read:buf maxLength:1024];
				
				if(len > 0 && len < 4096) {
					NSMutableData* data=[[NSMutableData alloc] initWithLength:0];
					[data appendBytes: (const void *)buf length:len];
					
					NSString *s = [NSString stringWithUTF8String:data.bytes];
					[self readIn:s];
					[data release];
				}
			} 
			break;
		}
		case NSStreamEventHasSpaceAvailable: {
			break;
		}
		default: {
			break;
		}
	}
}

- (void)readIn:(NSString *)s {
	[_delegate message_read:s];
}

- (void)writeOut:(NSString *)s {
	uint8_t *buf = (uint8_t *)[s UTF8String];
	[outputStream write:buf maxLength:strlen((char *)buf)+1];
}


@end
