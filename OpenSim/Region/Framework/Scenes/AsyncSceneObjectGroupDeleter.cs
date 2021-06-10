/*
 * Copyright (c) InWorldz Halcyon Developers
 * Copyright (c) Contributors, http://opensimulator.org/
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
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
using System.Reflection;
using System.Timers;
using log4net;
using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Region.Framework.Scenes
{        
    class DeleteToInventoryHolder
    {
        public DeRezAction action;
        public IClientAPI remoteClient;
        public List<SceneObjectGroup> objectGroups;
        public UUID folderID;
        public bool permissionToDelete;
    }
    
    /// <summary>
    /// Asynchronously derez objects.  This is used to derez large number of objects to inventory without holding 
    /// up the main client thread.
    /// </summary>
    public class AsyncSceneObjectGroupDeleter
    {   
        private static readonly ILog m_log
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <value>
        /// Is the deleter currently enabled?
        /// </value>
        public bool Enabled;
        
        private Timer m_inventoryTicker = new Timer(500);       
        private readonly Queue<DeleteToInventoryHolder> m_inventoryDeletes = new Queue<DeleteToInventoryHolder>();        
        private Scene m_scene;        
        
        public AsyncSceneObjectGroupDeleter(Scene scene)
        {
            m_scene = scene;
            
            m_inventoryTicker.AutoReset = false;
            m_inventoryTicker.Elapsed += InventoryRunDeleteTimer;            
        }

        /// <summary>
        /// Delete the given object from the scene
        /// </summary>
        public void DeleteToInventory(DeRezAction action, UUID folderID,
                IEnumerable<SceneObjectGroup> objectGroups, IClientAPI remoteClient, 
                bool permissionToDelete)
        {
            if (Enabled)
                m_inventoryTicker.Stop();


            lock (m_inventoryDeletes)
            {
                DeleteToInventoryHolder dtis = new DeleteToInventoryHolder();
                dtis.action = action;
                dtis.folderID = folderID;
                dtis.objectGroups = new List<SceneObjectGroup>(objectGroups);

                if (dtis.objectGroups.Count == 0)
                {
                    //something is wrong, caller sent us an empty set
                    throw new ArgumentException("DeleteToInventory() Can not work with an empty set", "objectGroups");
                }

                dtis.remoteClient = remoteClient;
                dtis.permissionToDelete = permissionToDelete;

                m_inventoryDeletes.Enqueue(dtis);
            }

            if (Enabled)
                m_inventoryTicker.Start();
        
            // Visually remove it, even if it isnt really gone yet.  This means that if we crash before the object
            // has gone to inventory, it will reappear in the region again on restart instead of being lost.
            // This is not ideal since the object will still be available for manipulation when it should be, but it's
            // better than losing the object for now.
            if (permissionToDelete)
            {
                foreach (SceneObjectGroup group in objectGroups)
                {
                    group.DeleteGroup(false);
                }
            }
        }
        
        private void InventoryRunDeleteTimer(object sender, ElapsedEventArgs e)
        {
            while (InventoryDeQueueAndDelete())
            {
            }
        }            

        /// <summary>
        /// Move the next object in the queue to inventory.  Then delete it properly from the scene.
        /// </summary>
        /// <returns></returns>
        public bool InventoryDeQueueAndDelete()
        {
            DeleteToInventoryHolder x = null;            
 
            try
            {
                lock (m_inventoryDeletes)
                {
                    int left = m_inventoryDeletes.Count;
                    if (left > 0)
                    {
                        x = m_inventoryDeletes.Dequeue();
                    }
                }

                if (x != null)
                {
                    try
                    {
                        m_scene.DeleteToInventory(x.action, x.folderID, x.objectGroups, x.remoteClient);

                        if (x.permissionToDelete)
                        {
                            m_scene.DeleteSceneObjects(x.objectGroups, false);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Text.StringBuilder objectNames = new System.Text.StringBuilder();
                        foreach (var obj in x.objectGroups)
                        {
                            objectNames.Append(obj.Name);
                            objectNames.Append(", ");
                        }

                        m_log.DebugFormat("[SCENE] Exception background sending object(s) {0}: {1}", objectNames.ToString(), e);
                    }
                    
                    return true;
                }   
                
            }
            catch (Exception e)
            {
                // We can't put the object group details in here since the root part may have disappeared (which is where these sit).
                // FIXME: This needs to be fixed.
                m_log.ErrorFormat(
                    "[SCENE]: Queued sending of scene object to agent {0} {1} failed: {2}",
                    (x != null ? x.remoteClient.Name : "unavailable"), (x != null ? x.remoteClient.AgentId.ToString() : "unavailable"), e.ToString());
            }

            return false;
        }        
    }
}
