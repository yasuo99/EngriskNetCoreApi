using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Mediator.Accounts
{
    public class List
    {
        public class Query : IRequest<List<Account>>
        {

        }
        public class Handler : IRequestHandler<Query, List<Account>>
        {
            private readonly ApplicationDbContext _context;
            public Handler(ApplicationDbContext context)
            {
               _context = context;
            }

            public async Task<List<Account>> Handle(Query request, CancellationToken cancellationToken)
            {
               return await _context.Accounts.ToListAsync();
            }
        }
    }
}