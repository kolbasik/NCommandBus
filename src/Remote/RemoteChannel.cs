using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;

namespace kolbasik.NCommandBus.Remote
{
    public sealed class RemoteChannel : IDisposable
    {
        private IChannel channel;

        public RemoteChannel(Func<IChannel> channelFactory)
        {
            if (channelFactory == null)
                throw new ArgumentNullException(nameof(channelFactory));
            this.channel = channelFactory();
            ChannelServices.RegisterChannel(channel, false);
        }

        public TService CreateProxy<TService>(string endpoint)
        {
            return (TService) Activator.GetObject(typeof(TService), endpoint);
        }

        public void Dispose()
        {
            ChannelServices.UnregisterChannel(channel);
        }

        public static RemoteChannel Http(int? port = null)
        {
            return new RemoteChannel(() => port.HasValue ? new HttpChannel(port.Value) : new HttpChannel());
        }

        public static RemoteChannel Tcp(int? port = null)
        {
            return new RemoteChannel(() => port.HasValue ? new TcpChannel(port.Value) : new TcpChannel());
        }
    }
}