@RestResource(urlMapping='/EchoObject/*')
global class EchoObjectController {
	
    global class Message {
        public string Method {get; set;}
        public string Value {get; set;} 
    }
    
    @HttpPost
    global static Message post(Message message) {
        if(message == null) {
            RestContext.response.statusCode = 400;
        }
        else {
        	message.Method = 'POST';
        }
        return message;
	}
    
    @HttpGet
    global static Message get() {
        Message result = new Message();
        result.Method = 'GET';
        result.Value = RestContext.request.params.get('message');
        
        if(String.isBlank(result.Value)) {
			RestContext.response.statusCode = 400;
        }
        
        return result;
    }
    
    @HttpPatch
    global static Message patch(Message message) {
        if(message == null) {
			RestContext.response.statusCode = 400;
        }
        else {
        	message.Method = 'PATCH';
        }
        
        return message;
    }
    
    @HttpPut
    global static Message put(Message message) {
        if(message == null) {
			RestContext.response.statusCode = 400;
        }
        else {
        	message.Method = 'PUT';
        }
        
        return message;
    }
    
    @HttpDelete
    global static Message doDelete() {
        Message result = new Message();
        result.Method = 'DELETE';
        result.Value = RestContext.request.params.get('message');
        
        if(String.isBlank(result.Value)) {
			RestContext.response.statusCode = 400;
        }
        
        return result;
    }
}