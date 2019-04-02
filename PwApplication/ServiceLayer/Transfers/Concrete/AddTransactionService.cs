﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DataLayer.EfCode;
using DataLayer.EfClasses;

namespace ServiceLayer.Transfers.Concrete
{
    /// <summary>
    /// Обеспечивает создание транзакции и добавление ее в базу данных
    /// </summary>
    public class AddTransactionService
    {
        private readonly PwContext context;
        public string LastError { get; private set; }

        public AddTransactionService(PwContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Создает новый шаблон транзакции
        /// </summary>
        /// <param name="sender">Идентификатор отправителя</param>
        /// <returns></returns>
        public AddTransactionDto GetBlancTransaction(int sender)
        {
            return new AddTransactionDto
            {
                SenderId = sender
            };
        }

        /// <summary>
        /// Создает новый шаблон транзакции на основе существующей
        /// </summary>
        /// <param name="transferId">Транзакция для клонирования</param>
        /// <returns></returns>
        public AddTransactionDto GetCloneTransaction(int transferId)
        {
            Transfer old = context.Transfers.AsNoTracking().Where(p => p.TransferId == transferId).SingleOrDefault();
            return new AddTransactionDto
            {
                SenderId = old.SenderId,
                RecipientId = old.RecipientId,
                Amount = old.Amount
            };
        }

        /// <summary>
        /// Добавляет траназкцию
        /// </summary>
        /// <param name="dto">Новая транзакция</param>
        /// <returns></returns>
        public Transfer AddTransaction(AddTransactionDto dto)
        {
            if (isValidBalance(dto.SenderId, dto.Amount))
            {
                Transfer newTransaction = new Transfer
                {
                    SenderId = dto.SenderId,
                    RecipientId = dto.RecipientId,
                    Amount = dto.Amount,
                    Timestamp = DateTime.Now
                };
                context.Transfers.Add(newTransaction);

                Operation outgoingOperation = new Operation
                {
                    TransferId = newTransaction.TransferId,
                    UserId = newTransaction.SenderId,
                    Credit = newTransaction.Amount,
                    ResultingBalance = context.Users.Where(p => p.UserId == newTransaction.SenderId)
                    .Select(p => p.Balance).FirstOrDefault() - newTransaction.Amount
                };
                context.Operations.Add(outgoingOperation);

                Operation incomingOperation = new Operation
                {
                    TransferId = newTransaction.TransferId,
                    UserId = newTransaction.RecipientId,
                    Debit = newTransaction.Amount,
                    ResultingBalance = context.Users.Where(p => p.UserId == newTransaction.RecipientId)
                    .Select(p => p.Balance).FirstOrDefault() + newTransaction.Amount
                };
                context.Operations.Add(incomingOperation);

                var sender = context.Users.Where(p => p.UserId == dto.SenderId).SingleOrDefault();
                sender.Balance = outgoingOperation.ResultingBalance;

                var recipient = context.Users.Where(p => p.UserId == dto.RecipientId).SingleOrDefault();
                recipient.Balance = incomingOperation.ResultingBalance;

                context.SaveChanges();
                return newTransaction;
            }
            else
            {
                return null;
            }
        }

        private bool isValidBalance(int userId, decimal amount)
        {
            var userBalance = context.Users.Where(p => p.UserId == userId).Select(p=>p.Balance).SingleOrDefault();
            if(userBalance-amount<0)
            {
                LastError = "You don't have enough PWs!";
                return false;
            }
            else
            {
                LastError = string.Empty;
                return true;
            }
        }
    }
}
