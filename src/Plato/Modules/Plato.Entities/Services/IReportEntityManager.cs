﻿using System.Threading.Tasks;
using Plato.Entities.Models;
using Plato.Internal.Abstractions;

namespace Plato.Entities.Services
{
    
    public interface IReportEntityManager<TModel> where TModel : class
    {
        Task<ICommandResult<TModel>> ReportAsync(ReportSubmission<TModel> submission);
    }
}
