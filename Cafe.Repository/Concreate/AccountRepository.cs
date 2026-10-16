using System.Threading.Tasks;
using Cafe.Model.Model;
using Cafe.Repository.Abstarct;
using Cafe.Service;

namespace Cafe.Repository.Concreate
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountService _accountService;

        public AccountRepository(AccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<RegistrationCompletionResponseDto> RegisterAdminAsync(RegistrationRequestDto request)
        {
            return await _accountService.RegisterAdminAsync(request);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            return await _accountService.LoginAsync(request);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _accountService.GetUserByIdAsync(id);
        }
    }
}
