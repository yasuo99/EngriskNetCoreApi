using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using MediatR;
using Persistence;

namespace Application.Mediator.Words
{
    public class List
    {
        public class Query: IRequest<List<Word>>{}
        public class Handler : IRequestHandler<Query, List<Word>>
        {
            private readonly ApplicationDbContext _context;
            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public Task<List<Word>> Handle(Query request, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}