using Microsoft.AspNetCore.Mvc.Filters;
using Persistence;

namespace Domain.Services
{
    public class ActionFilter : IActionFilter
    {
        private readonly ApplicationDbContext _context;
        public ActionFilter(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new System.NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}