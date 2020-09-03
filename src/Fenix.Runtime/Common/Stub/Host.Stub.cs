﻿
//AUTOGEN, do not modify it!

using Fenix.Common;
using Fenix.Common.Attributes;
using Fenix.Common.Message;
using Fenix.Common.Rpc;
using Fenix.Common.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Fenix
{
    public partial class Host
    {
#if CLIENT
        [RpcMethod(OpCode.ON_BEFORE_DISCONNECT_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API__Fenix__Host__on_before_disconnect(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (OnBeforeDisconnectNtf)msg;
            this.OnBeforeDisconnect(_msg.reason, () =>
            {
                var cbMsg = new OnBeforeDisconnectNtf.Callback();

                cb.Invoke(cbMsg);
            }, context);
            on_before_disconnect?.Invoke(_msg.reason, () =>
            {
                var cbMsg = new OnBeforeDisconnectNtf.Callback();

                cb.Invoke(cbMsg);
            });
        }

        [RpcMethod(OpCode.ON_SERVER_ACTOR_ENABLE_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API__Fenix__Host__on_server_actor_enable(IMessage msg, RpcContext context)
        {
            var _msg = (OnServerActorEnableNtf)msg;
            this.OnServerActorEnable(_msg.actorName, context);
            on_server_actor_enable?.Invoke(_msg.actorName);
        }

        [RpcMethod(OpCode.RECONNECT_SERVER_ACTOR_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API__Fenix__Host__reconnect_server_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (ReconnectServerActorNtf)msg;
            this.ReconnectServerActor(_msg.hostId, _msg.hostName, _msg.hostIP, _msg.hostPort, _msg.actorId, _msg.actorName, _msg.aTypeName, (code) =>
            {
                var cbMsg = new ReconnectServerActorNtf.Callback();
                cbMsg.code=code;
                cb.Invoke(cbMsg);
            }, context);
        }

        public event Action<global::Fenix.Common.DisconnectReason, global::System.Action> on_before_disconnect;
        [RpcMethod(OpCode.ON_BEFORE_DISCONNECT_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API_NATIVE__Fenix__Host__on_before_disconnect(global::Fenix.Common.DisconnectReason reason, global::System.Action callback, RpcContext context)
        {
            this.OnBeforeDisconnect(reason, callback, context);
            on_before_disconnect?.Invoke(reason, callback);
        }

        public event Action<global::System.String> on_server_actor_enable;
        [RpcMethod(OpCode.ON_SERVER_ACTOR_ENABLE_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API_NATIVE__Fenix__Host__on_server_actor_enable(global::System.String actorName, RpcContext context)
        {
            this.OnServerActorEnable(actorName, context);
            on_server_actor_enable?.Invoke(actorName);
        }

        [RpcMethod(OpCode.RECONNECT_SERVER_ACTOR_NTF, Api.ClientApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void CLIENT_API_NATIVE__Fenix__Host__reconnect_server_actor(global::System.UInt64 hostId, global::System.String hostName, global::System.String hostIP, global::System.Int32 hostPort, global::System.UInt64 actorId, global::System.String actorName, global::System.String aTypeName, global::System.Action<global::Fenix.Common.DefaultErrCode> callback, RpcContext context)
        {
            this.ReconnectServerActor(hostId, hostName, hostIP, hostPort, actorId, actorName, aTypeName, callback, context);
        }

#endif
#if !CLIENT
        [RpcMethod(OpCode.BIND_CLIENT_ACTOR_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API__Fenix__Host__bind_client_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (BindClientActorReq)msg;
            this.BindClientActor(_msg.actorName, (code) =>
            {
                var cbMsg = new BindClientActorReq.Callback();
                cbMsg.code=code;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.REGISTER_CLIENT_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API__Fenix__Host__register_client(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (RegisterClientReq)msg;
            this.RegisterClient(_msg.hostId, _msg.hostName, (code, arg1) =>
            {
                var cbMsg = new RegisterClientReq.Callback();
                cbMsg.code=code;
                cbMsg.arg1=arg1;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.REMOVE_CLIENT_ACTOR_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API__Fenix__Host__remove_client_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (RemoveClientActorReq)msg;
            this.RemoveClientActor(_msg.actorId, _msg.reason, (code) =>
            {
                var cbMsg = new RemoveClientActorReq.Callback();
                cbMsg.code=code;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.SAY_HELLO_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API__Fenix__Host__say_hello(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (SayHelloReq)msg;
            this.SayHello( (code, arg1) =>
            {
                var cbMsg = new SayHelloReq.Callback();
                cbMsg.code=code;
                cbMsg.arg1=arg1;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.CREATE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY__Fenix__Host__create_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (CreateActorReq)msg;
            this.CreateActor(_msg.typename, _msg.name, (code, arg1, arg2) =>
            {
                var cbMsg = new CreateActorReq.Callback();
                cbMsg.code=code;
                cbMsg.arg1=arg1;
                cbMsg.arg2=arg2;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.MIGRATE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY__Fenix__Host__migrate_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (MigrateActorReq)msg;
            this.MigrateActor(_msg.actorId, (code, arg1) =>
            {
                var cbMsg = new MigrateActorReq.Callback();
                cbMsg.code=code;
                cbMsg.arg1=arg1;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.REGISTER_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY__Fenix__Host__register(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (RegisterReq)msg;
            this.Register(_msg.hostId, _msg.hostName, (code, arg1) =>
            {
                var cbMsg = new RegisterReq.Callback();
                cbMsg.code=code;
                cbMsg.arg1=arg1;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.REMOVE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY__Fenix__Host__remove_actor(IMessage msg, Action<IMessage> cb, RpcContext context)
        {
            var _msg = (RemoveActorReq)msg;
            this.RemoveActor(_msg.actorId, (code) =>
            {
                var cbMsg = new RemoveActorReq.Callback();
                cbMsg.code=code;
                cb.Invoke(cbMsg);
            }, context);
        }

        [RpcMethod(OpCode.BIND_CLIENT_ACTOR_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API_NATIVE__Fenix__Host__bind_client_actor(global::System.String actorName, global::System.Action<global::Fenix.Common.DefaultErrCode> callback, RpcContext context)
        {
            this.BindClientActor(actorName, callback, context);
        }

        [RpcMethod(OpCode.REGISTER_CLIENT_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API_NATIVE__Fenix__Host__register_client(global::System.UInt64 hostId, global::System.String hostName, global::System.Action<global::Fenix.Common.DefaultErrCode, global::Fenix.HostInfo> callback, RpcContext context)
        {
            this.RegisterClient(hostId, hostName, callback, context);
        }

        [RpcMethod(OpCode.REMOVE_CLIENT_ACTOR_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API_NATIVE__Fenix__Host__remove_client_actor(global::System.UInt64 actorId, global::Fenix.Common.DisconnectReason reason, global::System.Action<global::Fenix.Common.DefaultErrCode> callback, RpcContext context)
        {
            this.RemoveClientActor(actorId, reason, callback, context);
        }

        [RpcMethod(OpCode.SAY_HELLO_REQ, Api.ServerApi)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_API_NATIVE__Fenix__Host__say_hello(global::System.Action<global::Fenix.Common.DefaultErrCode, global::Fenix.HostInfo> callback, RpcContext context)
        {
            this.SayHello(callback, context);
        }

        [RpcMethod(OpCode.CREATE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY_NATIVE__Fenix__Host__create_actor(global::System.String typename, global::System.String name, global::System.Action<global::Fenix.Common.DefaultErrCode, global::System.String, global::System.UInt64> callback, RpcContext context)
        {
            this.CreateActor(typename, name, callback, context);
        }

        [RpcMethod(OpCode.MIGRATE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY_NATIVE__Fenix__Host__migrate_actor(global::System.UInt64 actorId, global::System.Action<global::Fenix.Common.DefaultErrCode, global::System.Byte[]> callback, RpcContext context)
        {
            this.MigrateActor(actorId, callback, context);
        }

        [RpcMethod(OpCode.REGISTER_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY_NATIVE__Fenix__Host__register(global::System.UInt64 hostId, global::System.String hostName, global::System.Action<global::Fenix.Common.DefaultErrCode, global::Fenix.HostInfo> callback, RpcContext context)
        {
            this.Register(hostId, hostName, callback, context);
        }

        [RpcMethod(OpCode.REMOVE_ACTOR_REQ, Api.ServerOnly)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SERVER_ONLY_NATIVE__Fenix__Host__remove_actor(global::System.UInt64 actorId, global::System.Action<global::Fenix.Common.DefaultErrCode> callback, RpcContext context)
        {
            this.RemoveActor(actorId, callback, context);
        }

#endif
    }
}

