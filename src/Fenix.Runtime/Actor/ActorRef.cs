 
using Fenix.Common;
using Fenix.Common.Message;
using Fenix.Common.Rpc;
using Fenix.Common.Utils;
using Fenix.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Fenix
{
    public partial class ActorRef
    {
        public ActorRef() { }

        public ulong FromHostId => fromHost.Id;

        public Host fromHost { get; set; }

        public Actor fromActor { get; set; }

        public ulong toHostId { get; set; }

        public ulong toActorId { get; set; }

        public IPEndPoint toAddr { get; set; }

        public bool isClient;
         
        public NetworkType NetType => (isClient || Global.Host.IsClientMode) ? Global.Config.ClientNetwork : Global.Config.ServerNetwork;
        
        //{
        //    get
        //    {
        //        netType = (isClient || Global.Host.IsClientMode) ? Global.Config.ClientNetwork : NetworkType.TCP;
        //        return netType;
        //    }
        //    set
        //    {
        //        netType = value;
        //    }
        //}

        public static ActorRef Create(ulong toHostId, ulong toActorId, Type refType, Actor fromActor, Host fromHost, bool isClient, IPEndPoint toPeerEP=null)
        {
            //Ҫ���һ��fromActor.HostId��fromHost.Id�ǲ������
            if(fromActor!=null && fromActor.HostId != fromHost.Id)
            {
                Log.Error(string.Format("actor_host_id_unmatch {0} {1}", fromActor.UniqueName, fromHost.UniqueName));
                return null;
            }

            IPEndPoint toAddr = null;
            if (toPeerEP != null)
            {
                toAddr = toPeerEP;
            }
            else
            {
                if (toHostId != 0)
                    toAddr = Basic.ToAddress(Global.IdManager.GetHostAddr(toHostId));
                else if (toActorId != 0)
                    toAddr = Basic.ToAddress(Global.IdManager.GetHostAddrByActorId(toActorId, isClient));
            }

            if (toAddr == null)
                return null;

            var obj = (ActorRef)Activator.CreateInstance(refType);
            obj.toHostId = toHostId;
            obj.toActorId = toActorId;
            obj.fromActor = fromActor;
            obj.fromHost = fromHost;
            obj.toAddr = toAddr;
            obj.isClient = isClient;
            return obj;
        }

        public void CallRemoteMethod(int protocolCode, IMessage msg, Action<byte[]> cb)
        {
            //���protocode��client_api������kcp
            //������tcp
            //�ݶ����

            //var netType = NetworkType.TCP;
            //if (isClient)
            //    netType = NetworkType.KCP;
            
            //var api = Global.TypeManager.GetRpcType(protocolCode);
            //if (api == Common.Attributes.Api.ClientApi)
            //    this.NetType = Global.Config.ClientNetwork;

            //if (Global.Host.IsClientMode)
            //    netType = NetworkType.KCP;

            if (fromActor != null)
                fromActor.Rpc(protocolCode, FromHostId, fromActor.Id, toHostId, toActorId, toAddr, this.NetType, msg, cb);
            else
                fromHost.Rpc(protocolCode, FromHostId, 0, toHostId, toActorId, toAddr, this.NetType, msg, cb);
        }

        public bool Disconnect()
        {
            var peer = Global.NetManager.GetLocalPeerById(this.toHostId, this.NetType);
            bool result = Global.NetManager.Deregister(peer);
            var peer2 = Global.NetManager.GetRemotePeerById(this.toHostId, this.NetType);
            return result || Global.NetManager.Deregister(peer2);
        }
    }
}