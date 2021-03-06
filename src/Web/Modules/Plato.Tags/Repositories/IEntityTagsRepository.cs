﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PlatoCore.Repositories;

namespace Plato.Tags.Repositories
{
    public interface IEntityTagsRepository<T> : IRepository<T> where T : class
    {

        Task<IEnumerable<T>> SelectByEntityId(int entityId);

        Task<IEnumerable<T>> SelectByEntityReplyId(int entityReplyId);

        Task<bool> DeleteByEntityId(int entityId);

        Task<bool> DeleteByEntityIdAndTagId(int entityId, int tagId);

    }

}
