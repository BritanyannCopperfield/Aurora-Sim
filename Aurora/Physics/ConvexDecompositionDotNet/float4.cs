﻿/*
 * Copyright (c) Contributors, http://aurora-sim.org/, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/* The MIT License
 * 
 * Copyright (c) 2010 Intel Corporation.
 * All rights reserved.
 *
 * Based on the convexdecomposition library from 
 * <http://codesuppository.googlecode.com> by John W. Ratcliff and Stan Melax.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace Aurora.Region.Physics.ConvexDecompositionDotNet
{
    public class float4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public float4()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        public float4(float _x, float _y, float _z, float _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public float4(float3 v, float _w)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = _w;
        }

        public float4(float4 f)
        {
            x = f.x;
            y = f.y;
            z = f.z;
            w = f.w;
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public float3 xyz()
        {
            return new float3(x, y, z);
        }

        public void setxyz(float3 xyz)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            float4 f = obj as float4;
            if (f == null)
                return false;

            return this == f;
        }

        public static float4 Homogenize(float3 v3)
        {
            return Homogenize(v3, 1.0f);
        }

        //C++ TO C# CONVERTER NOTE: C# does not allow default values for parameters. Overloaded methods are inserted above.
        //ORIGINAL LINE: float4 Homogenize(const float3 &v3, const float &w =1.0f)
        public static float4 Homogenize(float3 v3, float w)
        {
            return new float4(v3.x, v3.y, v3.z, w);
        }

        public static float4 cmul(float4 a, float4 b)
        {
            return new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        public static float4 operator +(float4 a, float4 b)
        {
            return new float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }
        public static float4 operator -(float4 a, float4 b)
        {
            return new float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static float4 operator *(float4 v, float4x4 m)
        {
            return v.x * m.x + v.y * m.y + v.z * m.z + v.w * m.w; // yes this actually works
        }

        public static bool operator ==(float4 a, float4 b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
                return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
        }

        public static bool operator !=(float4 a, float4 b)
        {
            return !(a == b);
        }

        public static float4 operator *(float4 v, float s)
        {
            return new float4(v.x * s, v.y * s, v.z * s, v.w * s);
        }

        public static float4 operator *(float s, float4 v)
        {
            return new float4(v.x * s, v.y * s, v.z * s, v.w * s);
        }
    }
}
