using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Account;
using AutoMapper;
using Domain.Models;
using MediatR;
using Persistence;

namespace Application.Mediator.Accounts
{
    public class Create
    {
        public class Command : IRequest
        {
            public AccountForRegisterDTO Account { get; set; }
        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;
            public Handler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var account = _mapper.Map<Account>(request.Account);
                return Unit.Value;
            }
        }
    }
}