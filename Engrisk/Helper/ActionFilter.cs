using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Engrisk.Data;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Engrisk.Helper
{
    public class ActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if (resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var repo = resultContext.HttpContext.RequestServices.GetService<ICRUDRepo>();
                var user = await repo.GetOneWithCondition<Account>(acc => acc.Id == userId);
                await repo.SaveAll();
            }
        }
    }
}