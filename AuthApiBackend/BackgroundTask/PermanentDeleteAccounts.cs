using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;

namespace AuthApiBackend.BackgroundTask
{
    
    public class PermanentDeleteAccounts : BackgroundService
    {
        
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan timer = TimeSpan.FromMinutes(1);

        public PermanentDeleteAccounts(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while(!stoppingToken.IsCancellationRequested)
            {

                using(var scope =  serviceProvider.CreateAsyncScope())
                {
                    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var NumberOfAccounts = await accountService.NumberOfAccountsToDelete(stoppingToken);

                    var roundsToMake = NumberOfAccounts / 10;

                    var round = 0;

                    while (round <= roundsToMake)
                    {

                        IEnumerable<User>? permanentDeleteAccounts = await accountService.GetAllDeletedAccounts(round, stoppingToken);

                        if (permanentDeleteAccounts?.Count() != 0)
                        {
                            foreach (var user in permanentDeleteAccounts!)
                            {
                                await userService.DeleteUser(user, stoppingToken);
                            }
                        }

                        round++;
                    }
                }

                await Task.Delay(timer, stoppingToken);
            }

        }
        
    }

}