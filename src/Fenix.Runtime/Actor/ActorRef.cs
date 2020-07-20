using Fenix.Common.Rpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenix
{
    public partial class ActorRef
    {
        protected Actor fromActor;

        protected uint toActorId;

        public static ActorRef Create(uint toActorId, Actor fromActor)
        {
            var refType = Global.TypeManager.GetRefType(fromActor.GetType());
            var obj = (ActorRef)Activator.CreateInstance(refType);
            obj.toActorId = toActorId;
            obj.fromActor = fromActor;
            return obj;
        } 

        public void CallRemoteMethod(uint protocolCode, IMessage msg)
        {
            fromActor.Rpc(protocolCode, fromActor.ContainerId, fromActor.Id, this.toActorId, msg);
        }
    }
}