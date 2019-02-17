@RestResource(urlMapping='/Echo/*')
global class EchoController {
	
    @HttpPost
    global static String post(String message) {
        if(String.isBlank(message)) {
			RestContext.response.statusCode = 400;
			return 'Error: No message';
        }
        
        return '[POST] ' + message;
	}
    
    @HttpGet
    global static String get() {
        String message = RestContext.request.params.get('message');

        if(String.isBlank(message)) {
			RestContext.response.statusCode = 400;
			return 'Error: No message';
        }
        
        return '[GET] ' + message;
    }
    
    @HttpPatch
    global static String patch(string message) {
        if(String.isBlank(message)) {
			RestContext.response.statusCode = 400;
			return 'Error: No message';
        }
        
        return '[PATCH] ' + message;
    }
    
    @HttpPut
    global static String put(string message) {
        if(String.isBlank(message)) {
			RestContext.response.statusCode = 400;
			return 'Error: No message';
        }
        
        return '[PUT] ' + message;
    }
    
    @HttpDelete
    global static String doDelete() {
        String message = RestContext.request.params.get('message');

        if(String.isBlank(message)) {
			RestContext.response.statusCode = 400;
			return 'Error: No message';
        }
        
        return '[DELETE] ' + message;
    }
}