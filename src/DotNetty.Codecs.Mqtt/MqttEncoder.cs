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

namespace DotNetty.Codecs.Mqtt
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Codecs.Mqtt.Packets;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public sealed class MqttEncoder : MessageToMessageEncoder<Packet>
    {
        public static readonly MqttEncoder Instance = new MqttEncoder();
        private const int PacketIdLength = 2;
        private const int StringSizeLength = 2;
        private const int MaxVariableLength = 4;

        protected override void Encode(IChannelHandlerContext context, Packet message, List<object> output) => DoEncode(context.Allocator, message, output);

        public override bool IsSharable => true;

        /// <summary>
        ///     This is the main encoding method.
        ///     It's only visible for testing.
        ///     @param bufferAllocator Allocates ByteBuf
        ///     @param packet MQTT packet to encode
        ///     @return ByteBuf with encoded bytes
        /// </summary>
        internal static void DoEncode(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            switch (packet.PacketType)
            {
                case PacketType.CONNECT:
                    EncodeConnectMessage(bufferAllocator, (ConnectPacket)packet, output);
                    break;
                case PacketType.CONNACK:
                    EncodeConnAckMessage(bufferAllocator, (ConnAckPacket)packet, output);
                    break;
                case PacketType.PUBLISH:
                    EncodePublishMessage(bufferAllocator, (PublishPacket)packet, output);
                    break;
                case PacketType.PUBACK:
                case PacketType.PUBREC:
                case PacketType.PUBREL:
                case PacketType.PUBCOMP:
                case PacketType.UNSUBACK:
                    EncodePacketWithIdOnly(bufferAllocator, (PacketWithId)packet, output);
                    break;
                case PacketType.SUBSCRIBE:
                    EncodeSubscribeMessage(bufferAllocator, (SubscribePacket)packet, output);
                    break;
                case PacketType.SUBACK:
                    EncodeSubAckMessage(bufferAllocator, (SubAckPacket)packet, output);
                    break;
                case PacketType.UNSUBSCRIBE:
                    EncodeUnsubscribeMessage(bufferAllocator, (UnsubscribePacket)packet, output);
                    break;
                case PacketType.PINGREQ:
                case PacketType.PINGRESP:
                case PacketType.DISCONNECT:
                    EncodePacketWithFixedHeaderOnly(bufferAllocator, packet, output);
                    break;
                default:
                    ThrowHelper.ThrowArgumentException(packet); break;
            }
        }

        static void EncodeConnectMessage(IByteBufferAllocator bufferAllocator, ConnectPacket packet, List<object> output)
        {
            int payloadBufferSize = 0;

            // Client id
            string clientId = packet.ClientId;
            if (clientId is null) Util.ValidateClientId();
            byte[] clientIdBytes = EncodeStringInUtf8(clientId);
            payloadBufferSize += StringSizeLength + clientIdBytes.Length;

            byte[] willTopicBytes;
            IByteBuffer willMessage;
            if (packet.HasWill)
            {
                // Will topic and message
                string willTopic = packet.WillTopicName;
                willTopicBytes = EncodeStringInUtf8(willTopic);
                willMessage = packet.WillMessage;
                payloadBufferSize += StringSizeLength + willTopicBytes.Length;
                payloadBufferSize += 2 + willMessage.ReadableBytes;
            }
            else
            {
                willTopicBytes = null;
                willMessage = null;
            }

            string userName = packet.Username;
            byte[] userNameBytes;
            if (packet.HasUsername)
            {
                userNameBytes = EncodeStringInUtf8(userName);
                payloadBufferSize += StringSizeLength + userNameBytes.Length;
            }
            else
            {
                userNameBytes = null;
            }

            byte[] passwordBytes;
            if (packet.HasPassword)
            {
                string password = packet.Password;
                passwordBytes = EncodeStringInUtf8(password);
                payloadBufferSize += StringSizeLength + passwordBytes.Length;
            }
            else
            {
                passwordBytes = null;
            }

            // Fixed header
            byte[] protocolNameBytes = EncodeStringInUtf8(Util.ProtocolName);
            int variableHeaderBufferSize = StringSizeLength + protocolNameBytes.Length + 4;
            int variablePartSize = variableHeaderBufferSize + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                _ = buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);

                _ = buf.WriteShort(protocolNameBytes.Length);
                _ = buf.WriteBytes(protocolNameBytes);

                _ = buf.WriteByte(Util.ProtocolLevel);
                _ = buf.WriteByte(CalculateConnectFlagsByte(packet));
                _ = buf.WriteShort(packet.KeepAliveInSeconds);

                // Payload
                _ = buf.WriteShort(clientIdBytes.Length);
                _ = buf.WriteBytes(clientIdBytes, 0, clientIdBytes.Length);
                if (packet.HasWill)
                {
                    _ = buf.WriteShort(willTopicBytes.Length);
                    _ = buf.WriteBytes(willTopicBytes, 0, willTopicBytes.Length);
                    _ = buf.WriteShort(willMessage.ReadableBytes);
                    if (willMessage.IsReadable())
                    {
                        _ = buf.WriteBytes(willMessage);
                    }
                    _ = willMessage.Release();
                    willMessage = null;
                }
                if (packet.HasUsername)
                {
                    _ = buf.WriteShort(userNameBytes.Length);
                    _ = buf.WriteBytes(userNameBytes, 0, userNameBytes.Length);

                    if (packet.HasPassword)
                    {
                        _ = buf.WriteShort(passwordBytes.Length);
                        _ = buf.WriteBytes(passwordBytes, 0, passwordBytes.Length);
                    }
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
                willMessage?.SafeRelease();
            }
        }

        static int CalculateConnectFlagsByte(ConnectPacket packet)
        {
            int flagByte = 0;
            if (packet.HasUsername)
            {
                flagByte |= 0x80;
            }
            if (packet.HasPassword)
            {
                flagByte |= 0x40;
            }
            if (packet.HasWill)
            {
                flagByte |= 0x04;
                flagByte |= ((int)packet.WillQualityOfService & 0x03) << 3;
                if (packet.WillRetain)
                {
                    flagByte |= 0x20;
                }
            }
            if (packet.CleanSession)
            {
                flagByte |= 0x02;
            }
            return flagByte;
        }

        static void EncodeConnAckMessage(IByteBufferAllocator bufferAllocator, ConnAckPacket message, List<object> output)
        {
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(4);
                _ = buffer.WriteByte(CalculateFirstByteOfFixedHeader(message));
                _ = buffer.WriteByte(2); // remaining length
                if (message.SessionPresent)
                {
                    _ = buffer.WriteByte(1); // 7 reserved 0-bits and SP = 1
                }
                else
                {
                    _ = buffer.WriteByte(0); // 7 reserved 0-bits and SP = 0
                }
                _ = buffer.WriteByte((byte)message.ReturnCode);


                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static void EncodePublishMessage(IByteBufferAllocator bufferAllocator, PublishPacket packet, List<object> output)
        {
            IByteBuffer payload = packet.Payload ?? Unpooled.Empty;

            string topicName = packet.TopicName;
            if (0u >= (uint)topicName.Length) Util.ValidateTopicName();
            if (topicName.IndexOfAny(Util.TopicWildcards) > 0) Util.ValidateTopicName(topicName);
            byte[] topicNameBytes = EncodeStringInUtf8(topicName);

            int variableHeaderBufferSize = StringSizeLength + topicNameBytes.Length +
                (packet.QualityOfService > QualityOfService.AtMostOnce ? PacketIdLength : 0);
            int payloadBufferSize = payload.ReadableBytes;
            int variablePartSize = variableHeaderBufferSize + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;

            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                _ = buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);
                _ = buf.WriteShort(topicNameBytes.Length);
                _ = buf.WriteBytes(topicNameBytes);
                if (packet.QualityOfService > QualityOfService.AtMostOnce)
                {
                    _ = buf.WriteShort(packet.PacketId);
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
            }

            if (payload.IsReadable())
            {
                output.Add(payload.Retain());
            }
        }

        static void EncodePacketWithIdOnly(IByteBufferAllocator bufferAllocator, PacketWithId packet, List<object> output)
        {
            int msgId = packet.PacketId;

            const int VariableHeaderBufferSize = PacketIdLength; // variable part only has a packet id
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(fixedHeaderBufferSize + VariableHeaderBufferSize);
                _ = buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buffer, VariableHeaderBufferSize);
                _ = buffer.WriteShort(msgId);

                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static void EncodeSubscribeMessage(IByteBufferAllocator bufferAllocator, SubscribePacket packet, List<object> output)
        {
            const int VariableHeaderSize = PacketIdLength;
            int payloadBufferSize = 0;

            ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

            IByteBuffer buf = null;
            try
            {
                foreach (SubscriptionRequest topic in packet.Requests)
                {
                    byte[] topicFilterBytes = EncodeStringInUtf8(topic.TopicFilter);
                    payloadBufferSize += StringSizeLength + topicFilterBytes.Length + 1; // length, value, QoS
                    encodedTopicFilters.Add(topicFilterBytes);
                }

                int variablePartSize = VariableHeaderSize + payloadBufferSize;
                int fixedHeaderBufferSize = 1 + MaxVariableLength;

                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                _ = buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);

                // Variable Header
                _ = buf.WriteShort(packet.PacketId); // todo: review: validate?

                // Payload
                for (int i = 0; i < encodedTopicFilters.Count; i++)
                {
                    var topicFilterBytes = (byte[])encodedTopicFilters[i];
                    _ = buf.WriteShort(topicFilterBytes.Length);
                    _ = buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
                    _ = buf.WriteByte((int)packet.Requests[i].QualityOfService);
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
                encodedTopicFilters.Return();
            }
        }

        static void EncodeSubAckMessage(IByteBufferAllocator bufferAllocator, SubAckPacket message, List<object> output)
        {
            int payloadBufferSize = message.ReturnCodes.Count;
            int variablePartSize = PacketIdLength + payloadBufferSize;
            int fixedHeaderBufferSize = 1 + MaxVariableLength;
            IByteBuffer buf = null;
            try
            {
                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                _ = buf.WriteByte(CalculateFirstByteOfFixedHeader(message));
                WriteVariableLengthInt(buf, variablePartSize);
                _ = buf.WriteShort(message.PacketId);
                foreach (QualityOfService qos in message.ReturnCodes)
                {
                    _ = buf.WriteByte((byte)qos);
                }

                output.Add(buf);
                buf = null;

            }
            finally
            {
                buf?.SafeRelease();
            }
        }

        static void EncodeUnsubscribeMessage(IByteBufferAllocator bufferAllocator, UnsubscribePacket packet, List<object> output)
        {
            const int VariableHeaderSize = 2;
            int payloadBufferSize = 0;

            ThreadLocalObjectList encodedTopicFilters = ThreadLocalObjectList.NewInstance();

            IByteBuffer buf = null;
            try
            {
                foreach (string topic in packet.TopicFilters)
                {
                    byte[] topicFilterBytes = EncodeStringInUtf8(topic);
                    payloadBufferSize += StringSizeLength + topicFilterBytes.Length; // length, value
                    encodedTopicFilters.Add(topicFilterBytes);
                }

                int variablePartSize = VariableHeaderSize + payloadBufferSize;
                int fixedHeaderBufferSize = 1 + MaxVariableLength;

                buf = bufferAllocator.Buffer(fixedHeaderBufferSize + variablePartSize);
                _ = buf.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                WriteVariableLengthInt(buf, variablePartSize);

                // Variable Header
                _ = buf.WriteShort(packet.PacketId); // todo: review: validate?

                // Payload
                for (int i = 0; i < encodedTopicFilters.Count; i++)
                {
                    var topicFilterBytes = (byte[])encodedTopicFilters[i];
                    _ = buf.WriteShort(topicFilterBytes.Length);
                    _ = buf.WriteBytes(topicFilterBytes, 0, topicFilterBytes.Length);
                }

                output.Add(buf);
                buf = null;
            }
            finally
            {
                buf?.SafeRelease();
                encodedTopicFilters.Return();
            }
        }

        static void EncodePacketWithFixedHeaderOnly(IByteBufferAllocator bufferAllocator, Packet packet, List<object> output)
        {
            IByteBuffer buffer = null;
            try
            {
                buffer = bufferAllocator.Buffer(2);
                _ = buffer.WriteByte(CalculateFirstByteOfFixedHeader(packet));
                _ = buffer.WriteByte(0);

                output.Add(buffer);
                buffer = null;
            }
            finally
            {
                buffer?.SafeRelease();
            }
        }

        static int CalculateFirstByteOfFixedHeader(Packet packet)
        {
            int ret = 0;
            ret |= (int)packet.PacketType << 4;
            if (packet.Duplicate)
            {
                ret |= 0x08;
            }
            ret |= (int)packet.QualityOfService << 1;
            if (packet.RetainRequested)
            {
                ret |= 0x01;
            }
            return ret;
        }

        static void WriteVariableLengthInt(IByteBuffer buffer, int value)
        {
            do
            {
                int digit = value % 128;
                value /= 128;
                if (value > 0)
                {
                    digit |= 0x80;
                }
                _ = buffer.WriteByte(digit);
            }
            while (value > 0);
        }

        static byte[] EncodeStringInUtf8(string s)
        {
            // todo: validate against extra limitations per MQTT's UTF-8 string definition
            return TextEncodings.UTF8NoBOM.GetBytes(s);
        }
    }
}