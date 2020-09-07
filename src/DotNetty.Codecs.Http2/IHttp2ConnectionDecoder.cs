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
    using System;
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Handler for inbound traffic on behalf of <see cref="Http2ConnectionHandler"/>. Performs basic protocol
    /// conformance on inbound frames before calling the delegate <see cref="IHttp2FrameListener"/> for
    /// application-specific processing. Note that frames of an unknown type (i.e. HTTP/2 extensions)
    /// will skip all protocol checks and be given directly to the listener for processing.
    /// </summary>
    public interface IHttp2ConnectionDecoder : IDisposable
    {
        /// <summary>
        /// Sets the lifecycle manager. Must be called as part of initialization before the decoder is used.
        /// </summary>
        /// <param name="lifecycleManager"></param>
        void LifecycleManager(IHttp2LifecycleManager lifecycleManager);

        /// <summary>
        /// Provides direct access to the underlying connection.
        /// </summary>
        IHttp2Connection Connection { get; }

        /// <summary>
        /// Provides the local flow controller for managing inbound traffic.
        /// </summary>
        IHttp2LocalFlowController FlowController { get; }

        /// <summary>
        /// Gets or sets the <see cref="IHttp2FrameListener"/> which will be notified when frames are decoded.
        /// This <c>must</c> be set before frames are decoded.
        /// </summary>
        IHttp2FrameListener FrameListener { get; set; }

        /// <summary>
        /// Called by the <see cref="Http2ConnectionHandler"/> to decode the next frame from the input buffer.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        void DecodeFrame(IChannelHandlerContext ctx, IByteBuffer input, List<object> output);

        /// <summary>
        /// Gets the local settings for this endpoint of the HTTP/2 connection.
        /// </summary>
        Http2Settings LocalSettings { get; }

        /// <summary>
        /// Indicates whether or not the first initial <c>SETTINGS</c> frame was received from the remote endpoint.
        /// </summary>
        bool PrefaceReceived { get; }

        void Close();
    }
}
