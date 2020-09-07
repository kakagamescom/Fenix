﻿/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Mqtt.Packets
{
    using DotNetty.Buffers;
    using DotNetty.Common;

    public sealed class PublishPacket : PacketWithId, IByteBufferHolder
    {
        private readonly QualityOfService _qos;
        private readonly bool _duplicate;
        private readonly bool _retainRequested;

        public PublishPacket(QualityOfService qos, bool duplicate, bool retain)
        {
            _qos = qos;
            _duplicate = duplicate;
            _retainRequested = retain;
        }

        public override PacketType PacketType => PacketType.PUBLISH;

        public override bool Duplicate => _duplicate;

        public override QualityOfService QualityOfService => _qos;

        public override bool RetainRequested => _retainRequested;

        public string TopicName { get; set; }

        public IByteBuffer Payload { get; set; }

        public int ReferenceCount => Payload.ReferenceCount;

        public IReferenceCounted Retain()
        {
            _ = Payload.Retain();
            return this;
        }

        public IReferenceCounted Retain(int increment)
        {
            _ = Payload.Retain(increment);
            return this;
        }

        public IReferenceCounted Touch()
        {
            _ = Payload.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = Payload.Touch(hint);
            return this;
        }

        public bool Release() => Payload.Release();

        public bool Release(int decrement) => Payload.Release(decrement);

        IByteBuffer IByteBufferHolder.Content => Payload;

        public IByteBufferHolder Copy() => Replace(Payload.Copy());

        public IByteBufferHolder Replace(IByteBuffer content)
        {
            return new PublishPacket(_qos, _duplicate, _retainRequested)
            {
                TopicName = TopicName,
                Payload = content
            };
        }

        IByteBufferHolder IByteBufferHolder.Duplicate() => Replace(Payload.Duplicate());

        public IByteBufferHolder RetainedDuplicate() => Replace(Payload.RetainedDuplicate());
    }
}