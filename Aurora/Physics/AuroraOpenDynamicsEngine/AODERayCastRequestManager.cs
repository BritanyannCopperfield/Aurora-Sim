/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Aurora.Framework.ConsoleFramework;
using Aurora.Framework.Physics;
using OpenMetaverse;

//using Ode.NET;

namespace Aurora.Physics.AuroraOpenDynamicsEngine
{
    /// <summary>
    ///     Processes raycast requests as ODE is in a state to be able to do them.
    ///     This ensures that it's thread safe and there will be no conflicts.
    ///     Requests get returned by a different thread then they were requested by.
    /// </summary>
    public class AuroraODERayCastRequestManager
    {
        /// <summary>
        ///     ODE contact array to be filled by the collision testing
        /// </summary>
        protected int contactsPerCollision = 16;
        protected IntPtr ContactgeomsArray = IntPtr.Zero;

        private readonly List<ContactResult> m_contactResults = new List<ContactResult>();

        /// <summary>
        ///     ODE near callback delegate
        /// </summary>
        private readonly d.NearCallback nearCallback;

        /// <summary>
        ///     Pending Raycast Requests
        /// </summary>
        protected List<ODERayRequest> m_PendingRayRequests = new List<ODERayRequest>();

        /// <summary>
        ///     Pending Raycast Requests
        /// </summary>
        protected List<ODERayCastRequest> m_PendingRequests = new List<ODERayCastRequest>();

        /// <summary>
        ///     Scene that created this object.
        /// </summary>
        private AuroraODEPhysicsScene m_scene;


        public AuroraODERayCastRequestManager(AuroraODEPhysicsScene pScene)
        {
            m_scene = pScene;
            nearCallback = near;

            ContactgeomsArray = Marshal.AllocHGlobal(contactsPerCollision * d.ContactGeom.unmanagedSizeOf);
        }

        /// <summary>
        ///     Queues a raycast
        /// </summary>
        /// <param name="position">Origin of Ray</param>
        /// <param name="direction">Ray normal</param>
        /// <param name="length">Ray length</param>
        /// <param name="retMethod">Return method to send the results</param>
        public void QueueRequest(Vector3 position, Vector3 direction, float length, RaycastCallback retMethod)
        {
            lock (m_PendingRequests)
            {
                ODERayCastRequest req = new ODERayCastRequest
                                            {
                                                callbackMethod = retMethod,
                                                length = length,
                                                Normal = direction,
                                                Origin = position
                                            };

                m_PendingRequests.Add(req);
            }
        }

        /// <summary>
        ///     Queues a raycast
        /// </summary>
        /// <param name="position">Origin of Ray</param>
        /// <param name="direction">Ray normal</param>
        /// <param name="length">Ray length</param>
        /// <param name="count">Ray count</param>
        /// <param name="retMethod">Return method to send the results</param>
        public void QueueRequest(Vector3 position, Vector3 direction, float length, int count, RayCallback retMethod)
        {
            lock (m_PendingRayRequests)
            {
                ODERayRequest req = new ODERayRequest
                                        {
                                            callbackMethod = retMethod,
                                            length = length,
                                            Normal = direction,
                                            Origin = position,
                                            Count = count
                                        };

                m_PendingRayRequests.Add(req);
            }
        }

        /// <summary>
        ///     Process all queued raycast requests
        /// </summary>
        /// <returns>Time in MS the raycasts took to process.</returns>
        public int ProcessQueuedRequests()
        {
            int time = Environment.TickCount;
            ODERayCastRequest[] reqs = new ODERayCastRequest[0];
            lock (m_PendingRequests)
            {
                if (m_PendingRequests.Count > 0)
                {
                    reqs = m_PendingRequests.ToArray();
                    m_PendingRequests.Clear();
                }
            }
            for (int i = 0; i < reqs.Length; i++)
            {
                if (reqs[i].callbackMethod != null) // quick optimization here, don't raycast 
                    RayCast(reqs[i]); // if there isn't anyone to send results
            }

            ODERayRequest[] rayReqs = new ODERayRequest[0];
            lock (m_PendingRayRequests)
            {
                if (m_PendingRayRequests.Count > 0)
                {
                    rayReqs = m_PendingRayRequests.ToArray();
                    m_PendingRayRequests.Clear();
                }
            }
            for (int i = 0; i < rayReqs.Length; i++)
            {
                if (rayReqs[i].callbackMethod != null) // quick optimization here, don't raycast 
                    RayCast(rayReqs[i]); // if there isn't anyone to send results
            }

            lock (m_contactResults)
                m_contactResults.Clear();

            return Environment.TickCount - time;
        }

        /// <summary>
        ///     Method that actually initiates the raycast
        /// </summary>
        /// <param name="req"></param>
        private void RayCast(ODERayCastRequest req)
        {
            // Create the ray
            IntPtr ray = d.CreateRay(m_scene.space, req.length);
            d.GeomRaySet(ray, req.Origin.X, req.Origin.Y, req.Origin.Z, req.Normal.X, req.Normal.Y, req.Normal.Z);

            // Collide test
            d.SpaceCollide2(m_scene.space, ray, IntPtr.Zero, nearCallback);

            // Remove Ray
            d.GeomDestroy(ray);


            // Define default results
            bool hitYN = false;
            uint hitConsumerID = 0;
            float distance = 999999999999f;
            Vector3[] closestcontact = {new Vector3(99999f, 99999f, 99999f)};
            Vector3 snormal = Vector3.Zero;

            // Find closest contact and object.
            lock (m_contactResults)
            {
                foreach (
                    ContactResult cResult in
                        m_contactResults.Where(
                            cResult =>
                            Vector3.Distance(req.Origin, cResult.Pos) < Vector3.Distance(req.Origin, closestcontact[0]))
                    )
                {
                    closestcontact[0] = cResult.Pos;
                    hitConsumerID = cResult.ConsumerID;
                    distance = cResult.Depth;
                    hitYN = true;
                    snormal = cResult.Normal;
                }

                m_contactResults.Clear();
            }

            // Return results
            if (req.callbackMethod != null)
                req.callbackMethod(hitYN, closestcontact[0], hitConsumerID, distance, snormal);
        }

        /// <summary>
        ///     Method that actually initiates the raycast
        /// </summary>
        /// <param name="req"></param>
        private void RayCast(ODERayRequest req)
        {
            // Create the ray
            IntPtr ray = d.CreateRay(m_scene.space, req.length);
            d.GeomRaySet(ray, req.Origin.X, req.Origin.Y, req.Origin.Z, req.Normal.X, req.Normal.Y, req.Normal.Z);

            // Collide test
            d.SpaceCollide2(m_scene.space, ray, IntPtr.Zero, nearCallback);

            // Remove Ray
            d.GeomDestroy(ray);

            // Find closest contact and object.
            lock (m_contactResults)
            {
                m_contactResults.Sort(delegate(ContactResult a, ContactResult b)
                {
                    return a.Depth.CompareTo(b.Depth);
                });
                
                // Return results
                if (req.callbackMethod != null)
                    req.callbackMethod(m_contactResults.Take(req.Count).ToList());
            }
        }

        // This is the standard Near.   Uses space AABBs to speed up detection.
        private void near(IntPtr space, IntPtr g1, IntPtr g2)
        {
            //Don't test against heightfield Geom, or you'll be sorry!

            // Exclude heightfield geom

            if (g1 == IntPtr.Zero || g2 == IntPtr.Zero)
                return;
            if (d.GeomGetClass(g1) == d.GeomClassID.HeightfieldClass ||
                d.GeomGetClass(g2) == d.GeomClassID.HeightfieldClass)
                return;

            // Raytest against AABBs of spaces first, then dig into the spaces it hits for actual geoms.
            if (d.GeomIsSpace(g1) || d.GeomIsSpace(g2))
            {
                if (g1 == IntPtr.Zero || g2 == IntPtr.Zero)
                    return;

                // Separating static prim geometry spaces.
                // We'll be calling near recursivly if one
                // of them is a space to find all of the
                // contact points in the space
                try
                {
                    d.SpaceCollide2(g1, g2, IntPtr.Zero, nearCallback);
                }
                catch (AccessViolationException)
                {
                    MainConsole.Instance.Warn("[PHYSICS]: Unable to collide test a space");
                    return;
                }

                return;
            }

            if (g1 == IntPtr.Zero || g2 == IntPtr.Zero)
                return;

            int count = 0;
            try
            {
                if (g1 == g2)
                    return; // Can't collide with yourself

                count = d.CollidePtr(g1, g2, (contactsPerCollision & 0xffff), ContactgeomsArray,
                                        d.ContactGeom.unmanagedSizeOf);
            }
            catch (SEHException)
            {
                MainConsole.Instance.Error(
                    "[PHYSICS]: The Operating system shut down ODE because of corrupt memory.  This could be a result of really irregular terrain.  If this repeats continuously, restart using Basic Physics and terrain fill your terrain.  Restarting the sim.");
            }
            catch (Exception e)
            {
                MainConsole.Instance.WarnFormat("[PHYSICS]: Unable to collide test an object: {0}", e);
                return;
            }

            PhysicsActor p1 = null;

            if (g1 != IntPtr.Zero)
                m_scene.actor_name_map.TryGetValue(g1, out p1);

            // Loop over contacts, build results.
            d.ContactGeom curContact = new d.ContactGeom();
            for (int i = 0; i < count; i++)
            {
                if (!GetCurContactGeom(i, ref curContact))
                    break;

                if (p1 != null)
                {
                    if (p1 is AuroraODEPrim)
                    {
                        ContactResult collisionresult = new ContactResult
                                                            {
                                                                ConsumerID = ((AuroraODEPrim) p1).LocalID,
                                                                Pos =
                                                                    new Vector3(curContact.pos.X, curContact.pos.Y,
                                                                                curContact.pos.Z),
                                                                Depth = curContact.depth,
                                                                Normal =
                                                                    new Vector3(curContact.normal.X,
                                                                                curContact.normal.Y,
                                                                                curContact.normal.Z)
                                                            };

                        lock (m_contactResults)
                            m_contactResults.Add(collisionresult);
                    }
                }
            }
        }

        private bool GetCurContactGeom(int index, ref d.ContactGeom newcontactgeom)
        {
            if (ContactgeomsArray == IntPtr.Zero || index >= contactsPerCollision)
                return false;

            IntPtr contactptr = new IntPtr(ContactgeomsArray.ToInt64() + (index * d.ContactGeom.unmanagedSizeOf));
            newcontactgeom = (d.ContactGeom)Marshal.PtrToStructure(contactptr, typeof(d.ContactGeom));
            return true;
        }

        /// <summary>
        ///     Dereference the creator scene so that it can be garbage collected if needed.
        /// </summary>
        internal void Dispose()
        {
            m_scene = null;
            if (ContactgeomsArray != IntPtr.Zero)
                Marshal.FreeHGlobal(ContactgeomsArray);
        }
    }

    public struct ODERayCastRequest
    {
        public Vector3 Normal;
        public Vector3 Origin;
        public RaycastCallback callbackMethod;
        public float length;
    }

    public struct ODERayRequest
    {
        public int Count;
        public Vector3 Normal;
        public Vector3 Origin;
        public RayCallback callbackMethod;
        public float length;
    }
}