using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;


using LoginAndCrud.Contracts;

namespace LoginAndCrud.Application
{
    public class UserValidator
    {
        private readonly IUserRepository _repo;

        public UserValidator(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task ValidateCreateAsync(CreateUserRequest req, CancellationToken ct)
        {
            if (await _repo.ExistsByUsernameOrEmailAsync(req.Username, req.Email, ct))
                throw new InvalidOperationException("Username o Email ya existen.");
        }

        public async Task ValidateUpdateEmailAsync(int id, string newEmail, CancellationToken ct)
        {
            if (await _repo.EmailInUseByOtherAsync(id, newEmail, ct))
                throw new InvalidOperationException("Email ya en uso.");
        }
    }
}
