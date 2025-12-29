mergeInto(LibraryManager.library, {
    CopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(function() {
                console.log('Copied to clipboard');
            }).catch(function(err) {
                console.error('Failed to copy: ', err);
                // Fallback
                prompt('Copy this text:', text);
            });
        } else {
            // Fallback for older browsers
            prompt('Copy this text:', text);
        }
    },
    
    PasteFromClipboard: function(gameObjectNamePtr, methodNamePtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        
        if (navigator.clipboard && navigator.clipboard.readText) {
            navigator.clipboard.readText().then(function(text) {
                SendMessage(gameObjectName, methodName, text);
            }).catch(function(err) {
                console.error('Failed to paste: ', err);
                var text = prompt('Paste your text here:');
                if (text) {
                    SendMessage(gameObjectName, methodName, text);
                }
            });
        } else {
            // Fallback for older browsers
            var text = prompt('Paste your text here:');
            if (text) {
                SendMessage(gameObjectName, methodName, text);
            }
        }
    }
});
