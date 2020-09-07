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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Buffers
{
    using System.Runtime.CompilerServices;

    static class HeapByteBufferUtil
    {
        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static byte GetByte(byte[] memory, int index) => memory[index];

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static short GetShort(byte[] memory, int index) => 
            unchecked((short)(memory[index] << 8 | memory[index + 1]));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static short GetShortLE(byte[] memory, int index) => 
            unchecked((short)(memory[index] | memory[index + 1] << 8));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetUnsignedMedium(byte[] memory, int index) => 
            unchecked(
                memory[index] << 16 |
                memory[index + 1] << 8 |
                memory[index + 2]);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetUnsignedMediumLE(byte[] memory, int index) => 
            unchecked(
                memory[index] |
                memory[index + 1] << 8 |
                memory[index + 2] << 16);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetInt(byte[] memory, int index) => 
            unchecked(
                memory[index] << 24 |
                memory[index + 1] << 16 |
                memory[index + 2] << 8 |
                memory[index + 3]);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetIntLE(byte[] memory, int index) => 
            unchecked(
                memory[index] |
                memory[index + 1] << 8 |
                memory[index + 2] << 16 |
                memory[index + 3] << 24);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static long GetLong(byte[] memory, int index) => 
            unchecked(
                (long)memory[index] << 56 |
                (long)memory[index + 1] << 48 |
                (long)memory[index + 2] << 40 |
                (long)memory[index + 3] << 32 |
                (long)memory[index + 4] << 24 |
                (long)memory[index + 5] << 16 |
                (long)memory[index + 6] << 8 |
                memory[index + 7]);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static long GetLongLE(byte[] memory, int index) => 
            unchecked(
                memory[index] |
                (long)memory[index + 1] << 8 |
                (long)memory[index + 2] << 16 |
                (long)memory[index + 3] << 24 |
                (long)memory[index + 4] << 32 |
                (long)memory[index + 5] << 40 |
                (long)memory[index + 6] << 48 |
                (long)memory[index + 7] << 56);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetByte(byte[] memory, int index, int value)
        {
            unchecked
            {
                memory[index] = (byte)value;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetShort(byte[] memory, int index, int value)
        {
            unchecked
            {
                memory[index] = (byte)((ushort)value >> 8);
                memory[index + 1] = (byte)value;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetShortLE(byte[] memory, int index, int value)
        {
            unchecked
            {
                memory[index] = (byte)value;
                memory[index + 1] = (byte)((ushort)value >> 8);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetMedium(byte[] memory, int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                memory[index] = (byte)(unsignedValue >> 16);
                memory[index + 1] = (byte)(unsignedValue >> 8);
                memory[index + 2] = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetMediumLE(byte[] memory, int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                memory[index] = (byte)unsignedValue;
                memory[index + 1] = (byte)(unsignedValue >> 8);
                memory[index + 2] = (byte)(unsignedValue >> 16);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetInt(byte[] memory, int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                memory[index] = (byte)(unsignedValue >> 24);
                memory[index + 1] = (byte)(unsignedValue >> 16);
                memory[index + 2] = (byte)(unsignedValue >>8);
                memory[index + 3] = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetIntLE(byte[] memory, int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                memory[index] = (byte)unsignedValue;
                memory[index + 1] = (byte)(unsignedValue >> 8);
                memory[index + 2] = (byte)(unsignedValue >> 16);
                memory[index + 3] = (byte)(unsignedValue >> 24);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetLong(byte[] memory, int index, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                memory[index] = (byte)(unsignedValue >> 56);
                memory[index + 1] = (byte)(unsignedValue >> 48);
                memory[index + 2] = (byte)(unsignedValue >> 40);
                memory[index + 3] = (byte)(unsignedValue >> 32);
                memory[index + 4] = (byte)(unsignedValue >> 24);
                memory[index + 5] = (byte)(unsignedValue >> 16);
                memory[index + 6] = (byte)(unsignedValue >> 8);
                memory[index + 7] = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetLongLE(byte[] memory, int index, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                memory[index] = (byte)unsignedValue;
                memory[index + 1] = (byte)(unsignedValue >> 8);  
                memory[index + 2] = (byte)(unsignedValue >> 16);
                memory[index + 3] = (byte)(unsignedValue >> 24);
                memory[index + 4] = (byte)(unsignedValue >> 32);
                memory[index + 5] = (byte)(unsignedValue >> 40);
                memory[index + 6] = (byte)(unsignedValue >> 48);
                memory[index + 7] = (byte)(unsignedValue >> 56);
            }
        }
    }
}
