using Shared.Entities;
using Shared.Repositories;
namespace Infrastructure.EF.PostgreSQL.Repositories;

internal sealed class InboxMessageRepository(ApplicationDbContext context)
    : BaseRepository<InboxMessage>(context), IInboxMessageRepository
{
}
