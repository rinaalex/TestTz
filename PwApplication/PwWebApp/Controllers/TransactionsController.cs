﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataLayer.EfCode;
using ServiceLayer.Transfers;
using ServiceLayer.Transfers.Concrete;
using ServiceLayer.Transfers.QueryObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PwWebApp.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly PwContext context;

        public TransactionsController(PwContext context)
        {
            this.context = context;
        }

        // GET: api/transactions
        [Authorize]
        [HttpGet]
        public IEnumerable<TransactionListDto> Get()
        {
            ListTransactionService service = new ListTransactionService(context);
            SortFilterOptions optrions = new SortFilterOptions();
            optrions.OrderByOptions = OrderByOptions.ByDateTimeDesc;
            int userId = Convert.ToInt32(User.Identity.Name);
            var transactions = service.GetList(userId, optrions);
            return transactions;
        }

        // POST: api/transactions
        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody]AddTransactionDto dto)
        {
            if (ModelState.IsValid)
            {
                dto.SenderId = Convert.ToInt32(User.Identity.Name);
                AddTransactionService service = new AddTransactionService(context);
                var transaction = service.AddTransaction(dto);
                if (transaction == null)
                {
                    return BadRequest(service.LastError);
                }
                return new ObjectResult(transaction);
            }
            else
            {
                ModelState.AddModelError("", "The Amount should be positive");
                return BadRequest();
            }
        }

        // GET:
        [Authorize]
        [HttpGet("/getRecipientList")]
        public IEnumerable<RecipientListDto> GetRecipientList()
        {
            RecipientListService service = new RecipientListService(context);
            var senderId = Convert.ToInt32(User.Identity.Name);
            var recipients = service.GetRecipientList(senderId);
            return recipients;
        }        
    }
}
