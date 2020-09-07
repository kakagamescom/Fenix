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

#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
namespace DotNetty.Handlers.Tls
{
    using System;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Configures a <see cref="IChannelPipeline"/> depending on the application-level protocol negotiation result of
    /// <see cref="TlsHandler"/>.  For example, you could configure your HTTP pipeline depending on the result of ALPN:
    /// <code>
    /// public class MyInitializer extends {@link ChannelInitializer}&lt;{@link Channel}&gt; {
    ///     private final {@link SslContext} sslCtx;
    ///
    ///     public MyInitializer({@link SslContext} sslCtx) {
    ///         this.sslCtx = sslCtx;
    ///     }
    ///
    ///     protected void initChannel({@link Channel} ch) {
    ///         {@link ChannelPipeline} p = ch.pipeline();
    ///         p.addLast(sslCtx.newHandler(...)); // Adds {@link SslHandler}
    ///         p.addLast(new MyNegotiationHandler());
    ///     }
    /// }
    ///
    /// public class MyNegotiationHandler extends {@link ApplicationProtocolNegotiationHandler} {
    ///     public MyNegotiationHandler() {
    ///         super({@link ApplicationProtocolNames}.HTTP_1_1);
    ///     }
    ///
    ///     protected void configurePipeline({@link ChannelHandlerContext} ctx, String protocol) {
    ///         if ({@link ApplicationProtocolNames}.HTTP_2.equals(protocol) {
    ///             configureHttp2(ctx);
    ///         } else if ({@link ApplicationProtocolNames}.HTTP_1_1.equals(protocol)) {
    ///             configureHttp1(ctx);
    ///         } else {
    ///             throw new IllegalStateException("unknown protocol: " + protocol);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class ApplicationProtocolNegotiationHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ApplicationProtocolNegotiationHandler>();
        readonly SslApplicationProtocol fallbackProtocol;

        /// <summary>
        /// Creates a new instance with the specified fallback protocol name.
        /// </summary>
        /// <param name="protocol">the name of the protocol to use when
        /// ALPN/NPN negotiation fails or the client does not support ALPN/NPN</param>
        public ApplicationProtocolNegotiationHandler(string protocol)
        {
            if (protocol is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.protocol); }
            this.fallbackProtocol = new SslApplicationProtocol(protocol);
        }

        /// <summary>
        /// Creates a new instance with the specified fallback protocol name.
        /// </summary>
        /// <param name="fallbackProtocol">the name of the protocol to use when
        /// ALPN/NPN negotiation fails or the client does not support ALPN/NPN</param>
        public ApplicationProtocolNegotiationHandler(SslApplicationProtocol fallbackProtocol)
        {
            this.fallbackProtocol = fallbackProtocol;
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is TlsHandshakeCompletionEvent handshakeEvent)
            {
                try
                {
                    if (handshakeEvent.IsSuccessful)
                    {
                        var sslHandler = ctx.Pipeline.Get<TlsHandler>();
                        if (sslHandler is null) { ThrowInvalidOperationException(); }

                        var protocol = sslHandler.NegotiatedApplicationProtocol;
                        this.ConfigurePipeline(ctx, !protocol.Protocol.IsEmpty ? protocol : fallbackProtocol);
                    }
                    else
                    {
                        this.HandshakeFailure(ctx, handshakeEvent.Exception);
                    }
                }
                catch (Exception exc)
                {
                    this.ExceptionCaught(ctx, exc);
                }
                finally
                {
                    var pipeline = ctx.Pipeline;
                    if (pipeline.Context(this) is object)
                    {
                        pipeline.Remove(this);
                    }
                }
            }

            ctx.FireUserEventTriggered(evt);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException(
                    "cannot find an SslHandler in the pipeline (required for application-level protocol negotiation)");
            }
        }

        /// <summary>
        /// Invoked on successful initial SSL/TLS handshake. Implement this method to configure your pipeline
        /// for the negotiated application-level protocol.
        /// </summary>
        /// <param name="ctx">the context</param>
        /// <param name="protocol">the name of the negotiated application-level protocol, or
        /// the fallback protocol name specified in the constructor call if negotiation failed or the client
        /// isn't aware of ALPN/NPN extension</param>
        protected abstract void ConfigurePipeline(IChannelHandlerContext ctx, SslApplicationProtocol protocol);

        /// <summary>
        /// Invoked on failed initial SSL/TLS handshake.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cause"></param>
        protected virtual void HandshakeFailure(IChannelHandlerContext ctx, Exception cause)
        {
            if (Logger.WarnEnabled) { Logger.TlsHandshakeFailure(ctx, cause); }
            ctx.CloseAsync();
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (Logger.WarnEnabled) Logger.FailedToSelectAppProtocol(ctx, cause);
            ctx.FireExceptionCaught(cause);
            ctx.CloseAsync();
        }
    }
}
#endif