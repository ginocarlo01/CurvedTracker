mergeInto(LibraryManager.library, {

	StartWebGLTracker: function(ids, name)
	{
    	window.cTracker.startTracker(UTF8ToString(ids), UTF8ToString(name));
    },
    StopWebGLTracker: function()
	{
    	window.cTracker.stopTracker();
    },
    IsWebGLTrackerReady: function()
    {
        return window.cTracker.FOV != null;
    },
    SetWebGLTrackerSettings: function(settings)
	{
    	window.cTracker.setTrackerSettings(UTF8ToString(settings), "1.0.2.125409");
    },
    GetTrackerFov: function()
    {
        return window.cTracker.FOV;
    },
    UnpauseWebGLCamera: function()
	{
    	window.cTracker.unpauseCamera();
    },
    PauseWebGLCamera: function()
	{
    	window.cTracker.pauseCamera();
    },
    DebugImageTarget: function(id)
    {
        window.cTracker.debugImageTarget(UTF8ToString(id));
    },
    IsWebGLImageTracked: function(id)
    {
        return window.cTracker.isImageTracked(id);
    },
    GetWebGLCameraFrame: function(type)
    {
        var data = window.cTracker.getCameraTexture(UTF8ToString(type));
        var bufferSize = lengthBytesUTF8(data) + 1;
        var buffer =  unityInstance.Module._malloc(bufferSize);
        stringToUTF8(data, buffer, bufferSize);
        return buffer;
    },
    GetWebGLCameraName: function()
    {
        var name = window.cTracker.WEBCAM_NAME;
        var bufferSize = lengthBytesUTF8(name) + 1;
        var buffer =  unityInstance.Module._malloc(bufferSize);
        stringToUTF8(name, buffer, bufferSize);
        return buffer;
    },
    GetWebGLWarpedTexture: function(id)
    {
        var data = window.cTracker.getWarpedTexture(UTF8ToString(id));
        var bufferSize = lengthBytesUTF8(data) + 1;
        var buffer =  unityInstance.Module._malloc(bufferSize);
        stringToUTF8(data, buffer, bufferSize);
        return buffer;
    },
});
