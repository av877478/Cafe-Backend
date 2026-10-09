using Cafe.Repository.Abstarct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cafe.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        #region PRIVATE VARIABLES

        private IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
          
        }

        #endregion
    }
}
