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
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// <see cref="DecoratingHttp2ConnectionEncoder"/> which guards against a remote peer that will trigger a massive amount
    /// of control frames but will not consume our responses to these.
    /// This encoder will tear-down the connection once we reached the configured limit to reduce the risk of DDOS.
    /// </summary>
    sealed class Http2ControlFrameLimitEncoder : DecoratingHttp2ConnectionEncoder
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Http2ControlFrameLimitEncoder>();

        private readonly int _maxOutstandingControlFrames;

        private IHttp2LifecycleManager _lifecycleManager;
        private int _outstandingControlFrames;
        private bool _limitReached;

        public Http2ControlFrameLimitEncoder(IHttp2ConnectionEncoder encoder, int maxOutstandingControlFrames)
            : base(encoder)
        {
            if ((uint)(maxOutstandingControlFrames - 1) > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_Positive(maxOutstandingControlFrames, ExceptionArgument.maxOutstandingControlFrames);
            }
            _maxOutstandingControlFrames = maxOutstandingControlFrames;
        }

        public override void LifecycleManager(IHttp2LifecycleManager lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
            base.LifecycleManager(lifecycleManager);
        }

        public override Task WriteSettingsAckAsync(IChannelHandlerContext ctx, IPromise promise)
        {
            var newPromise = HandleOutstandingControlFrames(ctx, promise);
            if (newPromise is null)
            {
                return promise.Task;
            }
            return base.WriteSettingsAckAsync(ctx, newPromise);
        }

        public override Task WritePingAsync(IChannelHandlerContext ctx, bool ack, long data, IPromise promise)
        {
            // Only apply the limit to ping acks.
            if (ack)
            {
                var newPromise = HandleOutstandingControlFrames(ctx, promise);
                if (newPromise is null)
                {
                    return promise.Task;
                }
                return base.WritePingAsync(ctx, ack, data, newPromise);
            }
            return base.WritePingAsync(ctx, ack, data, promise);
        }

        public override Task WriteRstStreamAsync(IChannelHandlerContext ctx, int streamId, Http2Error errorCode, IPromise promise)
        {
            var newPromise = HandleOutstandingControlFrames(ctx, promise);
            if (newPromise is null)
            {
                return promise.Task;
            }
            return base.WriteRstStreamAsync(ctx, streamId, errorCode, newPromise);
        }

        private IPromise HandleOutstandingControlFrames(IChannelHandlerContext ctx, IPromise promise)
        {
            if (!_limitReached)
            {
                if (_outstandingControlFrames == _maxOutstandingControlFrames)
                {
                    // Let's try to flush once as we may be able to flush some of the control frames.
                    _ = ctx.Flush();
                }
                if (_outstandingControlFrames == _maxOutstandingControlFrames)
                {
                    _limitReached = true;
                    Http2Exception exception = ThrowHelper.GetConnectionError_Maximum_number_of_outstanding_control_frames_reached(_maxOutstandingControlFrames);
                    if (Logger.InfoEnabled)
                    {
                        Logger.Maximum_number_of_outstanding_control_frames_reached(_maxOutstandingControlFrames, ctx, exception);
                    }

                    // First notify the Http2LifecycleManager and then close the connection.
                    _lifecycleManager.OnError(ctx, true, exception);
                    _ = ctx.CloseAsync();
                }
                _outstandingControlFrames++;

                // We did not reach the limit yet, add the listener to decrement the number of outstanding control frames
                // once the promise was completed
                var newPromise = promise is object ? promise.Unvoid() : ctx.NewPromise();
                _ = newPromise.Task.ContinueWith(OutstandingControlFramesListenerAction, this, TaskContinuationOptions.ExecuteSynchronously);
                return newPromise;
            }
            return promise;
        }

        private static readonly Action<Task, object> OutstandingControlFramesListenerAction = (t, s) => OutstandingControlFramesListener(t, s);
        private static void OutstandingControlFramesListener(Task t, object s)
        {
            ((Http2ControlFrameLimitEncoder)s)._outstandingControlFrames--;
        }
    }
}