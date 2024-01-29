using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Account.DependencyInjection
{
    public static class CompositionRoot
    {
        public static void RegisterBaseAccountDependencies(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        }
    }
}
