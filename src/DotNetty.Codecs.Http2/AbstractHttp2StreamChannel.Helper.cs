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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    partial class AbstractHttp2StreamChannel
    {
        private static readonly Action<Task, object> WindowUpdateFrameWriteListenerAction = (t, s) => WindowUpdateFrameWriteListener(t, s);
        private static void WindowUpdateFrameWriteListener(Task future, object state)
        {
            WindowUpdateFrameWriteComplete(future, (AbstractHttp2StreamChannel)state);
        }

        private static void WindowUpdateFrameWriteComplete(Task future, IChannel streamChannel)
        {
            var cause = future.Exception.Unwrap();
            if (cause is object)
            {
                Exception unwrappedCause;
                // Unwrap if needed
                if (cause is Http2FrameStreamException && ((unwrappedCause = cause.InnerException) is object))
                {
                    cause = unwrappedCause;
                }

                // Notify the child-channel and close it.
                _ = streamChannel.Pipeline.FireExceptionCaught(cause);
                streamChannel.Unsafe.Close(streamChannel.Unsafe.VoidPromise());
            }
        }

        private void IncrementPendingOutboundBytes(long size, bool invokeLater)
        {
            if (0ul >= (ulong)size) { return; }

            long newWriteBufferSize = Interlocked.Add(ref v_totalPendingSize, size);
            if (newWriteBufferSize > _config.WriteBufferHighWaterMark)
            {
                SetUnwritable(invokeLater);
            }
        }

        private void DecrementPendingOutboundBytes(long size, bool invokeLater)
        {
            if (0ul >= (ulong)size) { return; }

            long newWriteBufferSize = Interlocked.Add(ref v_totalPendingSize, -size);
            // Once the totalPendingSize dropped below the low water-mark we can mark the child channel
            // as writable again. Before doing so we also need to ensure the parent channel is writable to
            // prevent excessive buffering in the parent outbound buffer. If the parent is not writable
            // we will mark the child channel as writable once the parent becomes writable by calling
            // trySetWritable() later.
            if (newWriteBufferSize < _config.WriteBufferLowWaterMark && Parent.IsWritable)
            {
                SetWritable(invokeLater);
            }
        }

        private void TrySetWritable()
        {
            // The parent is writable again but the child channel itself may still not be writable.
            // Lets try to set the child channel writable to match the state of the parent channel
            // if (and only if) the totalPendingSize is smaller then the low water-mark.
            // If this is not the case we will try again later once we drop under it.
            if (Volatile.Read(ref v_totalPendingSize) < _config.WriteBufferLowWaterMark)
            {
                SetWritable(false);
            }
        }

        private void SetWritable(bool invokeLater)
        {
            int currValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = currValue;
                int newValue = oldValue & ~1;
                currValue = Interlocked.CompareExchange(ref v_unwritable, newValue, oldValue);

                if (oldValue == currValue)
                {
                    if (oldValue != 0 && newValue == 0)
                    {
                        FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        private void SetUnwritable(bool invokeLater)
        {
            int currValue = Volatile.Read(ref v_unwritable);
            while (true)
            {
                int oldValue = currValue;
                int newValue = oldValue | 1;
                currValue = Interlocked.CompareExchange(ref v_unwritable, newValue, oldValue);

                if (oldValue == currValue)
                {
                    if (oldValue == 0 && newValue != 0)
                    {
                        FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        private void FireChannelWritabilityChanged(bool invokeLater)
        {
            var pipeline = _pipeline;
            if (invokeLater)
            {
                var task = _fireChannelWritabilityChangedTask;
                if (task is null)
                {
                    _fireChannelWritabilityChangedTask = task = InvokeFireChannelWritabilityChangedAction;
                }
                EventLoop.Execute(task, pipeline);
            }
            else
            {
                _ = pipeline.FireChannelWritabilityChanged();
            }
        }

        private static readonly Action<object> InvokeFireChannelWritabilityChangedAction = s => InvokeFireChannelWritabilityChanged(s);
        private static void InvokeFireChannelWritabilityChanged(object s)
        {
            _ = ((IChannelPipeline)s).FireChannelWritabilityChanged();
        }
    }
}
