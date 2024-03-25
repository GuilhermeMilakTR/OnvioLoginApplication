
namespace LoginApp
{
    public class AccountsService
    {
        private static AccountsService instance;

        public static AccountsService Instance
        {
            get
            {
                return instance ??= new AccountsService();
            }
        }

        public AccountsDTO Accounts { get; private set; }

        private AccountsService()
        {
            Accounts = AccountDataStoreService.Read();

            SetCodeForAccounts();
        }

        public AccountDTO FindByCode(int code)
        {
            return Accounts.Accounts.FirstOrDefault(x => x.Code.Equals(code));
        }

        public AccountDTO Save(AccountDTO accountDTO)
        {
            if (accountDTO.Code == -1)
            {
                accountDTO.Code = FindNextCode();

                Accounts.Accounts.Add(accountDTO);
            }
            else
            {
                AccountDTO existingAccountDTO = Accounts.Accounts.First(x => x.Code == accountDTO.Code);

                existingAccountDTO.CompanyId = accountDTO.CompanyId;
                existingAccountDTO.ContactId = accountDTO.ContactId;
                existingAccountDTO.UserId = accountDTO.UserId;
                existingAccountDTO.Username = accountDTO.Username;
                existingAccountDTO.Password = accountDTO.Password;
                existingAccountDTO.Environment = accountDTO.Environment;
            }

            AccountDataStoreService.Save(Accounts);

            return accountDTO;
        }

        public void RemoveByCode(int code)
        {
            Accounts.Accounts.Remove(FindByCode(code));

            AccountDataStoreService.Save(Accounts);
        }

        private void SetCodeForAccounts()
        {
            int i = 0;

            Accounts.Accounts.ForEach(account =>
            {
                account.Code = i++;
            });
        }

        private int FindNextCode()
        {
            return Accounts.Accounts.Count == 0 ? 0 : Accounts.Accounts.Max(x => x.Code) + 1;
        }
    }
}