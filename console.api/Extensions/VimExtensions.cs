/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NetVimClient;

namespace S3.Vm.Console.Extensions
{
    public enum Feature
    {
        iso,
        net,
        eth,
        boot,
        guest
    }
    public static class VimExtensions
    {
        public static string AsString(this ManagedObjectReference mor)
        {
            return $"{mor.type}|{mor.Value}";
        }

        public static ManagedObjectReference AsReference(this string mor)
        {
            var a = mor.Split('|');
            return new ManagedObjectReference
            {
                type = a.First(),
                Value = a.Last()
            };
        }

        public static void AddRam(this VirtualMachineConfigSpec vmcs, int ram)
        {
            vmcs.memoryMB = (ram > 0) ? ram * 1024 : 1024;
            vmcs.memoryMBSpecified = true;
        }

        public static void AddCpu(this VirtualMachineConfigSpec vmcs, string cpu)
        {
            string[] p = cpu.Split('x');
            int sockets = 1, coresPerSocket = 1;
            if (!Int32.TryParse(p[0], out sockets))
            {
                sockets = 1;
            }

            if (p.Length > 1)
            {
                if (!Int32.TryParse(p[1], out coresPerSocket))
                {
                    coresPerSocket = 1;
                }
            }

            vmcs.numCPUs = sockets * coresPerSocket;
            vmcs.numCPUsSpecified = true;
            vmcs.numCoresPerSocket = coresPerSocket;
            vmcs.numCoresPerSocketSpecified = true;
        }

        public static void AddBootOption(this VirtualMachineConfigSpec vmcs, int delay)
        {
            if (delay != 0)
            {
                vmcs.bootOptions = new VirtualMachineBootOptions();
                if (delay > 0)
                {
                    vmcs.bootOptions.bootDelay = delay * 1000;
                    vmcs.bootOptions.bootDelaySpecified = true;
                }
                if (delay < 0)
                {
                    vmcs.bootOptions.enterBIOSSetup = true;
                    vmcs.bootOptions.enterBIOSSetupSpecified = true;
                }
            }
        }

        public static void AddGuestInfo(this VirtualMachineConfigSpec vmcs, string[] list)
        {
            List<OptionValue> options = new List<OptionValue>();
            foreach (string item in list)
            {
                OptionValue option = new OptionValue();
                int x = item.IndexOf('=');
                if (x > 0)
                {
                    option.key = item.Substring(0, x).Replace(" ", "").Trim();
                    if (!option.key.StartsWith("guestinfo."))
                        option.key = "guestinfo." + option.key;
                    option.value = item.Substring(x + 1).Trim();
                    options.Add(option);
                }
            }
            vmcs.extraConfig = options.ToArray();
        }

        public static void MergeGuestInfo(this VirtualMachineConfigSpec vmcs, string settings)
        {
            //constitue options dictionary
            //constitute settings array
            //foreach setting add/update options
            //persist result in annotation
        }

        public static ObjectContent First(this ObjectContent[] tree, string type)
        {
            return tree.Where(o => o.obj.type == type).FirstOrDefault();
        }

        public static ObjectContent FindTypeByName(this ObjectContent[] tree, string type, string name)
        {
            foreach (var content in tree.Where(o => o.obj.type.EndsWith(type)))
            {
                if (content.propSet
                    .Any(p => p.name == "name" && p.val.ToString().ToLower() == name))
                {
                    return content;
                }
            }
            return null;
        }

        public static ObjectContent[] FindType(this ObjectContent[] tree, string type)
        {
            return tree.Where(o => o.obj.type.EndsWith(type)).ToArray();
        }

        public static ObjectContent FindTypeByReference(this ObjectContent[] tree, ManagedObjectReference mor)
        {
            return tree
                .Where(o => o.obj.type == mor.type && o.obj.Value == mor.Value)
                .SingleOrDefault();
        }

        public static object GetProperty(this ObjectContent content, string name)
        {
            return content
                .propSet.Where(p => p.name == name)
                .Select(p => p.val)
                .SingleOrDefault();
        }

        public static bool IsInPool(this ObjectContent content, ManagedObjectReference pool)
        {
            ManagedObjectReference mor = content.GetProperty("resourcePool") as ManagedObjectReference;
            return mor != null && mor.Value == pool.Value;
        }
    }
}

