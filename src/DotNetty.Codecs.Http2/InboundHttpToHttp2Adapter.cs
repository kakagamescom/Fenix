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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http2
{
    using DotNetty.Codecs.Http;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Translates HTTP/1.x object reads into HTTP/2 frames.
    /// </summary>
    public class InboundHttpToHttp2Adapter : ChannelHandlerAdapter
    {
        private readonly IHttp2Connection _connection;
        private readonly IHttp2FrameListener _listener;

        public InboundHttpToHttp2Adapter(IHttp2Connection connection, IHttp2FrameListener listener)
        {
            _connection = connection;
            _listener = listener;
        }

        private static int GetStreamId(IHttp2Connection connection, HttpHeaders httpHeaders)
        {
            return httpHeaders.GetInt(HttpConversionUtil.ExtensionHeaderNames.StreamId,
                                      connection.Remote.IncrementAndGetNextStreamId);
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (message is IFullHttpMessage fullHttpMessage)
            {
                Handle(ctx, _connection, _listener, fullHttpMessage);
            }
            else
            {
                _ = ctx.FireChannelRead(message);
            }
        }

        // note that this may behave strangely when used for the initial upgrade
        // message when using h2c, since that message is ineligible for flow
        // control, but there is not yet an API for signaling that.
        internal static void Handle(IChannelHandlerContext ctx, IHttp2Connection connection,
            IHttp2FrameListener listener, IFullHttpMessage message)
        {
            try
            {
                int streamId = GetStreamId(connection, message.Headers);
                IHttp2Stream stream = connection.Stream(streamId);
                if (stream is null)
                {
                    stream = connection.Remote.CreateStream(streamId, false);
                }
                _ = message.Headers.Set(HttpConversionUtil.ExtensionHeaderNames.Scheme, HttpScheme.Http.Name);
                IHttp2Headers messageHeaders = HttpConversionUtil.ToHttp2Headers(message, true);
                var hasContent = message.Content.IsReadable();
                var hasTrailers = !message.TrailingHeaders.IsEmpty;
                listener.OnHeadersRead(ctx, streamId, messageHeaders, 0, !(hasContent || hasTrailers));
                if (hasContent)
                {
                    _ = listener.OnDataRead(ctx, streamId, message.Content, 0, !hasTrailers);
                }
                if (hasTrailers)
                {
                    IHttp2Headers headers = HttpConversionUtil.ToHttp2Headers(message.TrailingHeaders, true);
                    listener.OnHeadersRead(ctx, streamId, headers, 0, true);
                }
                _ = stream.CloseRemoteSide();
            }
            finally
            {
                _ = message.Release();
            }
        }
    }
}
