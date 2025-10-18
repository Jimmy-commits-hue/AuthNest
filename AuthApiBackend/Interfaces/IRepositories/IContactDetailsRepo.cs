namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IContactDetailsRepo
    {
        Task CreateAsync(Models.ContactDetails contactDetails, CancellationToken cancellationToken);

        Task<Models.ContactDetails?> GetAsync(string userId, CancellationToken cancellationToken);

        Task UpdateIsEmailVerified(string emailId, CancellationToken cancellationToken);
    }

}
