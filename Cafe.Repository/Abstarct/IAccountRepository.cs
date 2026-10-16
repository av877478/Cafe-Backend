using System.Threading.Tasks;
using Cafe.Model.Model;

namespace Cafe.Repository.Abstarct
{
    public interface IAccountRepository
    {
        Task<RegistrationCompletionResponseDto> RegisterAdminAsync(RegistrationRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserDto?> GetUserByIdAsync(int id);
    }
}
