mergeInto(LibraryManager.library, {
    DownloadTextFile: function(fileNamePtr, textPtr) {
        var fileName = UTF8ToString(fileNamePtr);
        var text = UTF8ToString(textPtr);

        try {
            var blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
            var url = URL.createObjectURL(blob);
            var anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = fileName || 'map.txt';
            anchor.style.display = 'none';
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            setTimeout(function() { URL.revokeObjectURL(url); }, 1000);
            console.log('Download started: ' + anchor.download);
        } catch (err) {
            console.error('Failed to start download: ', err);
            prompt('Save this text manually:', text);
        }
    },
    
    OpenTextFilePicker: function(gameObjectNamePtr, methodNamePtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);

        var input = document.createElement('input');
        input.type = 'file';
        input.accept = '.txt,.map,text/plain';
        input.style.display = 'none';

        input.addEventListener('change', function() {
            if (!input.files || input.files.length === 0) {
                document.body.removeChild(input);
                return;
            }

            var file = input.files[0];
            var reader = new FileReader();
            reader.onload = function() {
                SendMessage(gameObjectName, methodName, reader.result);
                document.body.removeChild(input);
            };
            reader.onerror = function(err) {
                console.error('Failed to read file: ', err);
                document.body.removeChild(input);
            };
            reader.readAsText(file);
        });

        document.body.appendChild(input);
        input.click();
    }
});
