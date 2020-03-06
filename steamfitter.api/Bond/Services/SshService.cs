/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Net.Sockets;
using System.Threading;
using Bond.Infrastructure.Models;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Bond
{
    /// <summary>
    /// Manages the creation and maintenance of ssh tunnels to sshd server that in turn provides port forwarding 
    /// </summary>
    internal static class SshService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal static Thread sshManagerThread = null;  
        
        /// <summary>
        /// Creates new thread running Ssh client connection
        /// </summary>
        internal static void Run(ClientConfiguration.SshOptions config)
        {
            while (true)
            {
                // ssh not running? (re)start it...
                if (config.IsEnabled && (sshManagerThread == null || !sshManagerThread.IsAlive))
                {
                    sshManagerThread = RunThread(config); // get new thread
                    sshManagerThread.Start(); // now run it
                    _log.Debug($"Started new SshService thread. Thread alive? {sshManagerThread.IsAlive} ");
                }

                Thread.Sleep((config.CheckProcessIntervalInSeconds * 1000));
            }
        }

        private static Thread RunThread(ClientConfiguration.SshOptions config)
        {
            var t = new Thread(() => { RunEx(config); })
            {
                IsBackground = true,
                Name = "ssh"
            };
            return t;
        }

        private static void RunEx(ClientConfiguration.SshOptions config)
        {
            BondManager.CurrentPorts.Clear();

            try
            {
                _log.Info(
                    $"Starting SSH client with {config.KeepAliveIntervalInSeconds}s keep-alive to {config.Host}:{config.Port} as {config.Username}...");

                using (var client = new SshClient(config.Host, config.Port, config.Username, config.Password))
                {
                    try
                    {
                        client.Connect();
                        client.KeepAliveInterval = TimeSpan.FromSeconds(config.KeepAliveIntervalInSeconds);

                        client.ErrorOccurred += delegate(object sender, ExceptionEventArgs e) { _log.Trace(e); };
                        client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                        {
                            _log.Trace($"Host key received: {e.HostKey}:{e.HostKeyName}");
                        };

                        foreach (var forwardedPort in config.SshPorts)
                        {
                            //define port and add it to client
                            var port = new ForwardedPortRemote(forwardedPort.Server, forwardedPort.ServerPort, forwardedPort.Guest,
                                forwardedPort.GuestPort);
                            client.AddForwardedPort(port);

                            //add delegates to handle port exceptions and requests received 
                            port.Exception += delegate(object sender, ExceptionEventArgs e) { _log.Info(e.Exception.ToString()); };
                            port.RequestReceived += delegate(object sender, PortForwardEventArgs e)
                            {
                                _log.Info($"{e.OriginatorHost}:{e.OriginatorPort}â€”{sender}");
                            };

                            //start the port, which will give us connection information back from server
                            port.Start();
                            //get that bound port from server
                            _log.Info($"Bound port: {port.BoundPort} - Is started?: {port.IsStarted}");

                            forwardedPort.ServerPort = port.BoundPort;

                            if (!BondManager.CurrentPorts.Contains(forwardedPort))
                                BondManager.CurrentPorts.Add(forwardedPort);
                        }

                        var result = client.RunCommand("uptime");
                        _log.Info(result.Result);

                        while (client.IsConnected)
                        {
                            // ... hold the port open ... //    
                            Thread.Sleep(1000);
                        }

                    }
                    catch (SocketException se)
                    {
                        _log.Error(se);

                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                    finally
                    {
                        try
                        {
                            foreach (var port in client.ForwardedPorts)
                            {
                                port.Stop();
                                client.RemoveForwardedPort(port);
                                _log.Info($"Bound port stopped and removed: {port}");
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                        
                        client.Disconnect();
                        _log.Info($"Client disconnected...");
                    }
                }
            }
            catch (SocketException se)
            {
                _log.Error(se);
                
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
    }
}
