/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foreman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Foreman.Core.Services
{
    public interface IWebHookRepositoryService
    {
        Task<IEnumerable<WebHook>> Get(CancellationToken ct);
        Task<WebHook> GetById(Guid id, CancellationToken ct);
        Task<WebHook> Create(WebHook webHook, CancellationToken ct);
        Task<WebHook> Update(WebHook webHook, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
    }
    public class WebHookRepositoryService : IWebHookRepositoryService
    {
        private readonly ApplicationDbContext _context;
        public WebHookRepositoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WebHook>> Get(CancellationToken ct)
        {
            return await _context.WebHooks.ToListAsync(ct);
        }

        public async Task<WebHook> GetById(Guid id, CancellationToken ct)
        {
            return await _context.WebHooks.SingleOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<WebHook> Create(WebHook webHook, CancellationToken ct)
        {
            if (webHook.Id == Guid.Empty)
                webHook.Id = Guid.NewGuid();
            
            _context.WebHooks.Add(webHook);
            await _context.SaveChangesAsync(ct);
            return webHook;
        }

        public async Task<WebHook> Update(WebHook webHook, CancellationToken ct)
        {
            _context.Entry(webHook).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebHookExists(webHook.Id))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return webHook;
        }
        
        public async Task Delete(Guid id, CancellationToken ct)
        {
            var webHook = await _context.WebHooks.SingleOrDefaultAsync(m => m.Id == id);
            if (webHook == null)
            {
                throw new ArgumentException("WebHook not found");
            }

            _context.WebHooks.Remove(webHook);
            await _context.SaveChangesAsync(ct);
        }
        
        private bool WebHookExists(Guid id)
        {
            return _context.WebHooks.Any(e => e.Id == id);
        }
    }
}
