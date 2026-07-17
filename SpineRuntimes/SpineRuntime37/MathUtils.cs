/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Runtime.CompilerServices;

namespace SpineRuntime37 {
	public static class MathUtils {
		public const float PI = 3.1415927f;
		public const float PI2 = PI * 2;
		public const float RadDeg = 180f / PI;
		public const float DegRad = PI / 180;

		static Random random = new Random();

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

        static public float RandomTriangle(float min, float max)
        {
            return RandomTriangle(min, max, (min + max) * 0.5f);
        }

        static public float RandomTriangle(float min, float max, float mode)
        {
            float u = random.NextSingle();
            float d = max - min;
            if (u <= (mode - min) / d) return min + MathF.Sqrt(u * d * (mode - min));
            return max - MathF.Sqrt((1 - u) * d * (max - mode));
        }
    }

    public abstract class IInterpolation
    {
        public static IInterpolation Pow2 = new Pow(2);
        public static IInterpolation Pow2Out = new PowOut(2);

        protected abstract float Apply(float a);

        public float Apply(float start, float end, float a)
        {
            return start + (end - start) * Apply(a);
        }
    }

    public class Pow : IInterpolation
    {
        public float Power { get; set; }

        public Pow(float power)
        {
            Power = power;
        }

        protected override float Apply(float a)
        {
            if (a <= 0.5f) return (float)Math.Pow(a * 2, Power) / 2;
            return MathF.Pow((a - 1) * 2, Power) / (Power % 2 == 0 ? -2 : 2) + 1;
        }
    }

    public class PowOut : Pow
    {
        public PowOut(float power) : base(power)
        {
        }

        protected override float Apply(float a)
        {
            return MathF.Pow(a - 1, Power) * (Power % 2 == 0 ? -1 : 1) + 1;
        }
    }
}
