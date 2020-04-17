/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Stackstorm.Api.Client;
using Xunit;

namespace Stackstorm.Api.Client.Test
{
    public class StackstormClient : StackstormBase
    {
        [Fact]
        public void ClientGetsToken()
        {
            Assert.True(Client.HasToken());
        }
        
        [Fact]
        public void ClientInstantiatesSt2Vsphere()
        {
            Assert.NotNull(Client.VSphere);
        }
        
        [Fact]
        public void ClientInstantiatesSt2Core()
        {
            Assert.NotNull(Client.Core);
        }
        
        [Fact]
        public void ClientInstantiatesSt2Packs()
        {
            Assert.NotNull(Client.Packs);
        }
        
        [Fact]
        public void ClientInstantiatesSt2Rules()
        {
            Assert.NotNull(Client.Rules);
        }
        
        [Fact]
        public void ClientInstantiatesSt2Actions()
        {
            Assert.NotNull(Client.Actions);
        }
        
        [Fact]
        public void ClientInstantiatesSt2Executions()
        {
            Assert.NotNull(Client.Executions);
        }

        [Fact]
        public void ClientGetsActions()
        {
            var list = Client.Actions.GetActionsAsync().Result;
            foreach (var item in list)
                Assert.True(!string.IsNullOrEmpty(item.name), "Action names can't be null or empty");
            Assert.True(list.Count > 0);
        }
        
        [Fact]
        public void ClientGetsExecutions()
        {
            var list = Client.Executions.GetExecutionsAsync(10).Result;
            foreach (var item in list)
                Assert.True(!string.IsNullOrEmpty(item.id), "Execution id can't be null or empty");
            Assert.True(list.Count > 0);
        }
        
        [Fact]
        public void ClientGetsRules()
        {
            var list = Client.Rules.GetRulesAsync().Result;
            foreach (var item in list)
                Assert.True(!string.IsNullOrEmpty(item.name), "Rule name can't be null or empty");
            Assert.True(list.Count > 0);
        }
        
        [Fact]
        public void ClientGetsPacks()
        {
            var list = Client.Packs.GetPacksAsync().Result;
            foreach (var item in list)
                Assert.True(!string.IsNullOrEmpty(item.id), "Pack id can't be null or empty");
            Assert.True(list.Count > 0);
        }
    }
}
