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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using log4net;
using Nini.Config;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.CoreModules.Avatar.InstantMessage
{
    public class PresenceModule : IRegionModule, IPresenceModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool m_Enabled = false;
        private bool m_Gridmode = false;

        private List<Scene> m_Scenes = new List<Scene>();

        // we currently are only interested in root-agents. If the root isn't here, we don't know the region the
        // user is in, so we have to ask the messaging server anyway.
        private Dictionary<UUID, Scene> m_RootAgents =
                new Dictionary<UUID, Scene>();

        public event PresenceChange OnPresenceChange;
        public event BulkPresenceData OnBulkPresenceData;

        public void Initialize(Scene scene, IConfigSource config)
        {
            lock (m_Scenes)
            {
                // This is a shared module; Initialize will be called for every region on this server.
                // Only check config once for the first region.
                if (m_Scenes.Count == 0)
                {
                    IConfig cnf = config.Configs["Messaging"];
                    if (cnf != null && cnf.GetString(
                            "PresenceModule", "PresenceModule") !=
                            "PresenceModule")
                        return;

                    cnf = config.Configs["Startup"];
                    if (cnf != null)
                        m_Gridmode = cnf.GetBoolean("gridmode", false);

                    m_Enabled = true;
                }

                if (m_Gridmode)
                    NotifyMessageServerOfStartup(scene);

                m_Scenes.Add(scene);
            }

            scene.RegisterModuleInterface<IPresenceModule>(this);

            scene.EventManager.OnNewClient += OnNewClient;
            scene.EventManager.OnSetRootAgentScene += OnSetRootAgentScene;
            scene.EventManager.OnMakeChildAgent += OnMakeChildAgent;
        }

        public void PostInitialize()
        {
        }

        public void Close()
        {
            if (!m_Gridmode || !m_Enabled)
                return;

            if (OnPresenceChange != null)
            {
                lock (m_RootAgents)
                {
                    // on shutdown, users are kicked, too
                    foreach (KeyValuePair<UUID, Scene> pair in m_RootAgents)
                    {
                        OnPresenceChange(new PresenceInfo(pair.Key, UUID.Zero));
                    }
                }
            }

            lock (m_Scenes)
            {
                foreach (Scene scene in m_Scenes)
                    NotifyMessageServerOfShutdown(scene);
            }
        }

        public string Name
        {
            get { return "PresenceModule"; }
        }

        public bool IsSharedModule
        {
            get { return true; }
        }

        // new client doesn't mean necessarily that user logged in, it just means it entered one of the
        // the regions on this server
        public void OnNewClient(IClientAPI client)
        {
            client.OnConnectionClosed += OnConnectionClosed;
            client.OnLogout += OnLogout;

            // KLUDGE: See handler for details.
            client.OnEconomyDataRequest += OnEconomyDataRequest;
        }

        // connection closed just means *one* client connection has been closed. It doesn't mean that the
        // user has logged off; it might have just TPed away.
        public void OnConnectionClosed(IClientAPI client)
        {
            // TODO: Have to think what we have to do here...
            // Should we just remove the root from the list (if scene matches)?
            if (!(client.Scene is Scene))
                return;
            Scene scene = (Scene)client.Scene;

            lock (m_RootAgents)
            {
                Scene rootScene;
                if (!(m_RootAgents.TryGetValue(client.AgentId, out rootScene)) || scene != rootScene)
                    return;

                m_RootAgents.Remove(client.AgentId);
            }

            // Should it have logged off, we'll do the logout part in OnLogout, even if no root is stored
            // anymore. It logged off, after all...
        }

        // Triggered when the user logs off.
        public void OnLogout(IClientAPI client)
        {
            if (!(client.Scene is Scene))
                return;
            Scene scene = (Scene)client.Scene;

            // On logout, we really remove the client from rootAgents, even if the scene doesn't match
            lock (m_RootAgents)
            {
                if (m_RootAgents.ContainsKey(client.AgentId)) m_RootAgents.Remove(client.AgentId);
            }

            // now inform the messaging server and anyone who is interested
            NotifyMessageServerOfAgentLeaving(client.AgentId, scene.RegionInfo.RegionID, scene.RegionInfo.RegionHandle);
            if (OnPresenceChange != null) OnPresenceChange(new PresenceInfo(client.AgentId, UUID.Zero));
        }

        public void OnSetRootAgentScene(UUID agentID, Scene scene)
        {
            // OnSetRootAgentScene can be called from several threads at once (with different agentID).
            // Concurrent access to m_RootAgents is prone to failure on multi-core/-processor systems without
            // correct locking).
            lock (m_RootAgents)
            {
                Scene rootScene;
                if (m_RootAgents.TryGetValue(agentID, out rootScene) && scene == rootScene)
                {
                    return;
                }
                m_RootAgents[agentID] = scene;
            }

            Util.FireAndForget(delegate(object obj)
            {
                // inform messaging server that agent changed the region
                NotifyMessageServerOfAgentLocation(agentID, scene.RegionInfo.RegionID, scene.RegionInfo.RegionHandle);
            });
        }

        private void OnEconomyDataRequest(IClientAPI client, UUID agentID)
        {
            // KLUDGE: This is the only way I found to get a message (only) after login was completed and the
            // client is connected enough to receive UDP packets.
            // This packet seems to be sent only once, just after connection was established to the first
            // region after login.
            // We use it here to trigger a presence update; the old update-on-login was never be heard by
            // the freshly logged in viewer, as it wasn't connected to the region at that time.
            // TODO: Feel free to replace this by a better solution if you find one.

            // get the agent. This should work every time, as we just got a packet from it
            ScenePresence agent = null;
            lock (m_Scenes)
            {
                foreach (Scene scene in m_Scenes)
                {
                    agent = scene.GetScenePresence(agentID);
                    if (agent != null) break;
                }
            }

            // just to be paranoid...
            if (agent == null)
            {
                m_log.ErrorFormat("[PRESENCE]: Got a packet from agent {0} who can't be found anymore!?", agentID);
                return;
            }

            // we are a bit premature here, but the next packet will switch this child agent to root.
            if (OnPresenceChange != null) OnPresenceChange(new PresenceInfo(agentID, agent.Scene.RegionInfo.RegionID));
        }

        public void OnMakeChildAgent(ScenePresence agent)
        {
            // OnMakeChildAgent can be called from several threads at once (with different agent).
            // Concurrent access to m_RootAgents is prone to failure on multi-core/-processor systems without
            // correct locking).
            lock (m_RootAgents)
            {
                Scene rootScene;
                if (m_RootAgents.TryGetValue(agent.UUID, out rootScene) && agent.Scene == rootScene)
                {
                    m_RootAgents.Remove(agent.UUID);
                }
            }
            // don't notify the messaging-server; either this agent just had been downgraded and another one will be upgraded
            // to root momentarily (which will notify the messaging-server), or possibly it will be closed in a moment,
            // which will update the messaging-server, too.
        }

        private void NotifyMessageServerOfStartup(Scene scene)
        {
            Hashtable xmlrpcdata = new Hashtable();
            xmlrpcdata["RegionUUID"] = scene.RegionInfo.RegionID.ToString();
            ArrayList SendParams = new ArrayList();
            SendParams.Add(xmlrpcdata);
            try
            {
                string methodName = "region_startup";
                XmlRpcRequest UpRequest = new XmlRpcRequest(methodName, SendParams);
                XmlRpcResponse resp = UpRequest.Send(Util.XmlRpcRequestURI(scene.CommsManager.NetworkServersInfo.MessagingURL, methodName), 5000);

                Hashtable responseData = (Hashtable)resp.Value;
                if (responseData == null || (!responseData.ContainsKey("success")) || (string)responseData["success"] != "TRUE")
                {
                    m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of region startup for region {0}", scene.RegionInfo.RegionName);
                }
            }
            catch (WebException)
            {
                m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of region startup for region {0}", scene.RegionInfo.RegionName);
            }
        }

        private void NotifyMessageServerOfShutdown(Scene scene)
        {
            Hashtable xmlrpcdata = new Hashtable();
            xmlrpcdata["RegionUUID"] = scene.RegionInfo.RegionID.ToString();
            ArrayList SendParams = new ArrayList();
            SendParams.Add(xmlrpcdata);
            try
            {
                string methodName = "region_shutdown";
                XmlRpcRequest DownRequest = new XmlRpcRequest(methodName, SendParams);
                XmlRpcResponse resp = DownRequest.Send(Util.XmlRpcRequestURI(scene.CommsManager.NetworkServersInfo.MessagingURL, methodName), 5000);

                Hashtable responseData = (Hashtable)resp.Value;
                if ((!responseData.ContainsKey("success")) || (string)responseData["success"] != "TRUE")
                {
                    m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of region shutdown for region {0}", scene.RegionInfo.RegionName);
                }
            }
            catch (WebException)
            {
                m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of region shutdown for region {0}", scene.RegionInfo.RegionName);
            }
        }

        private void NotifyMessageServerOfAgentLocation(UUID agentID, UUID region, ulong regionHandle)
        {
            Hashtable xmlrpcdata = new Hashtable();
            xmlrpcdata["AgentID"] = agentID.ToString();
            xmlrpcdata["RegionUUID"] = region.ToString();
            xmlrpcdata["RegionHandle"] = regionHandle.ToString();
            ArrayList SendParams = new ArrayList();
            SendParams.Add(xmlrpcdata);
            try
            {
                string methodName = "agent_location";
                XmlRpcRequest LocationRequest = new XmlRpcRequest(methodName, SendParams);
                XmlRpcResponse resp = LocationRequest.Send(Util.XmlRpcRequestURI(m_Scenes[0].CommsManager.NetworkServersInfo.MessagingURL, methodName), 5000);

                Hashtable responseData = (Hashtable)resp.Value;
                if ((!responseData.ContainsKey("success")) || (string)responseData["success"] != "TRUE")
                {
                    m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of agent location for {0}", agentID.ToString());
                }
            }
            catch (WebException)
            {
                m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of agent location for {0}", agentID.ToString());
            }
        }

        private void NotifyMessageServerOfAgentLeaving(UUID agentID, UUID region, ulong regionHandle)
        {
            Hashtable xmlrpcdata = new Hashtable();
            xmlrpcdata["AgentID"] = agentID.ToString();
            xmlrpcdata["RegionUUID"] = region.ToString();
            xmlrpcdata["RegionHandle"] = regionHandle.ToString();
            ArrayList SendParams = new ArrayList();
            SendParams.Add(xmlrpcdata);
            try
            {
                string methodName = "agent_leaving";
                XmlRpcRequest LeavingRequest = new XmlRpcRequest(methodName, SendParams);
                XmlRpcResponse resp = LeavingRequest.Send(Util.XmlRpcRequestURI(m_Scenes[0].CommsManager.NetworkServersInfo.MessagingURL, methodName), 5000);

                Hashtable responseData = (Hashtable)resp.Value;
                if ((!responseData.ContainsKey("success")) || (string)responseData["success"] != "TRUE")
                {
                    m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of agent leaving for {0}", agentID.ToString());
                }
            }
            catch (WebException)
            {
                m_log.ErrorFormat("[PRESENCE]: Failed to notify message server of agent leaving for {0}", agentID.ToString());
            }
        }
    }
}
