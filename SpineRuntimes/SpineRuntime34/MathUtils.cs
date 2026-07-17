/******************************************************************************
 * Spine Runtimes Software License v2.5
 * 
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Runtime.CompilerServices;

namespace SpineRuntime34 {
	public static class MathUtils {
		public const float PI = 3.1415927f;
		public const float PI2 = PI * 2;
		public const float RadDeg = 180f / PI;
		public const float DegRad = PI / 180;

        /// <summary>Returns the sine of a given angle in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float Sin(float radians)
        {
            return MathF.Sin(radians);
        }

        /// <summary>Returns the cosine of a given angle in radians.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float Cos(float radians)
        {
            return MathF.Cos(radians);
        }

        /// <summary>Returns the sine of a given angle in degrees.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float SinDeg(float degrees)
        {
            return MathF.Sin(degrees * DegRad);
        }

        /// <summary>Returns the cosine of a given angle in degrees.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float CosDeg(float degrees)
        {
            return MathF.Cos(degrees * DegRad);
        }

        /// <summary>Returns the atan2 using Math.Atan2.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float Atan2(float y, float x)
        {
            return MathF.Atan2(y, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float Clamp(float value, float min, float max)
        {
            return Math.Clamp(value, min, max);
        }
    }
}
