﻿//AUTOGEN, do not modify it!

using Fenix.Common.Utils;
using Fenix.Common;
using Fenix.Common.Attributes;
using Fenix.Common.Rpc;
using MessagePack; 
using System.ComponentModel;
using System; 

namespace Fenix.Common.Message
{
    [MessageType(OpCode.REGISTER_REQ)]
    [MessagePackObject]
    public class RegisterReq : IMessage
    {
        [Key(0)]
        public global::System.UInt64 hostId { get; set; }

        [Key(1)]
        public global::System.String hostName { get; set; }

        public override byte[] Pack()
        {
            return MessagePackSerializer.Serialize<RegisterReq>(this);
        }

        public new static RegisterReq Deserialize(byte[] data)
        {
            return MessagePackSerializer.Deserialize<RegisterReq>(data);
        }

        public override void UnPack(byte[] data)
        {
            var obj = Deserialize(data);
            Copier<RegisterReq>.CopyTo(obj, this);
        }
    }
}

