using System.Collections.Generic;

namespace Oswos.Repository
{
    public interface IWebsiteRepository
    {
        void AddOrUpdate(Website website);
        void Remove(Website website);
        Website GetByHost(string host);
        IEnumerable<Website> GetAll();
    }
}