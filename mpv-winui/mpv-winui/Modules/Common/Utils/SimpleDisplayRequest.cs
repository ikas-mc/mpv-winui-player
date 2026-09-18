using Windows.System.Display;

namespace mpv_winui.Modules.Common.Utils
{
    public class SimpleDisplayRequest
    {
        private DisplayRequest? _displayRequest;
        private int _requestCount;

        public void Enable()
        {
            if (null == _displayRequest)
            {
                _displayRequest = new DisplayRequest();
                _requestCount = 0;
            }

            try
            {
                _displayRequest.RequestActive();
                _requestCount++;
            }
            catch (System.Exception)
            {
                //
            }
        }

        public void Disable()
        {
            if (null != _displayRequest)
            {
                try
                {
                    for (var i = 0; i < _requestCount; i++)
                    {
                        _displayRequest?.RequestRelease();
                    }
                }
                catch (System.Exception)
                {
                    //
                }
                finally
                {
                    _requestCount = 0;
                }
            }
        }
    }

}
