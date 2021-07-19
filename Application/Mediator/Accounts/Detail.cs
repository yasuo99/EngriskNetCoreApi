using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using MediatR;
using Persistence;

namespace Application.Mediator.Accounts
{
    public class Detail
    {
        public class Query: IRequest<Account>{
            public int Id { get; set; }
        }
        public class Handler : IRequestHandler<Query, Account>
        {
            private readonly ApplicationDbContext _context;
            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Account> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Accounts.FindAsync(request.Id);
            }
        }
    }
}