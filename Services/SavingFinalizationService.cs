using OnlineBankingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Services
{
    public interface ISavingFinalizationService
    {
        void FinalizeSaving();
    }
    public class SavingFinalizationService : ISavingFinalizationService
    {
        private OnlineBankingDBContext _onlineBankingDB;
        private IOTPService service;

        public SavingFinalizationService(OnlineBankingDBContext onlineBankingDB, IOTPService oTPService)
        {
            _onlineBankingDB = onlineBankingDB;
            service = oTPService;
        }
        public void FinalizeSaving()
        {
            int hour = 0;
            int minute = 0;
            //if (DateTime.Now.Hour == hour)
            //{
            //    if (DateTime.Now.Minute - minute == 0)
            //    {
                    List<SavingInfo> savingInfos = _onlineBankingDB.SavingInfos.ToList();
                    Account sourceAccount;
                    long interest, amount;
                    SavingPackage package;
                    if (savingInfos.Count > 0)
                    {
                        foreach (SavingInfo info in savingInfos)
                        {
                            if (DateTime.Compare(DateTime.Now.Date, info.EndDate.Date) >= 0)
                            {
                                sourceAccount = _onlineBankingDB.Accounts
                                    .FirstOrDefault(a => a.AccountNumber.Equals(info.AccountNumber));
                                package = GetPackage(info.PackageId);
                                interest = (long)(info.Amount * (package.Interest / 100) * ((double)package.Duration / 12));
                                amount = info.Amount + interest;
                                // save history
                                TransferCommand transferCommand = new()
                                {
                                    Id = "MTBT00" + (_onlineBankingDB.TransferCommands.Count() + 1).ToString(),
                                    Amount = amount,
                                    Content = "Finalize saving " + "#" + info.SavingId,
                                    Type = 2,
                                    FromAccountNumber = info.SavingId,
                                    ToAccountNumber = sourceAccount.AccountNumber,
                                    ToCurrentBalance = sourceAccount.Balance + amount,
                                    FromCurrentBalance = amount - amount
                                };
                                _onlineBankingDB.TransferCommands.Add(transferCommand);
                                Transaction toTransaction = new()
                                {
                                    ChangedAmount = amount,
                                    AccountNumber = sourceAccount.AccountNumber,
                                    CreatedAt = DateTime.Now,
                                    CommandId = transferCommand.Id
                                };
                                _onlineBankingDB.Transactions.Add(toTransaction);
                                // change balance
                                sourceAccount.Balance += amount;
                                // remove finalized saving
                                _onlineBankingDB.SavingInfos.Remove(info);
                                _onlineBankingDB.SaveChanges();
                                // send notification message to user
                                User user = GetUser(sourceAccount);
                                service.SendReceivedTransferMessageNotification
                                    (
                                        toTransaction.CreatedAt,
                                        user.Phone,
                                        sourceAccount.AccountNumber,
                                        transferCommand.FromAccountNumber,
                                        amount,
                                        transferCommand.ToCurrentBalance,
                                        transferCommand.Content
                                    );
                                Console.WriteLine("Finalized saving " + info.SavingId);
                            }
                        }
                    }
            //    }
            //}
        }

        private SavingPackage GetPackage(int Id)
        {
            var pakage = _onlineBankingDB.SavingPackages.FirstOrDefault(p => p.Id == Id);
            return pakage;
        }
        private User GetUser(Account account)
        {
            var user = _onlineBankingDB.Users.FirstOrDefault(u => u.Id == account.UserId);
            return user;
        }

    }
}
