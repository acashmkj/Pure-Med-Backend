﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PureMedBlockChain_Backend.Data;
using PureMedBlockChain_Backend.Models;

namespace PureMedBlockChain_Backend.Controllers
{
    public class BlockChainController : ControllerBase
    {
        private readonly UserDbContext _userContext;
        private readonly ProductInfoDbContext _productInfoDbContext;
        private BlockChain PureMedBlockChain;
        private readonly BlockChainsDbContext blockChainsDbContext;

        public BlockChainController(IOptions<BlockChain> blockChain, UserDbContext userDbContext, BlockChainsDbContext sdb, ProductInfoDbContext pdb)
        {
            _userContext = userDbContext;
            _productInfoDbContext = pdb;
            blockChainsDbContext = sdb;
            PureMedBlockChain = blockChain.Value;
        }

        [HttpPost]
        public string GetBlockChain()
        {
            return JsonConvert.SerializeObject(PureMedBlockChain.Chain);
        }

        [HttpPost]
        public async Task<string> GetFullBlockChain(string userName, string password)
        {
            var User = await _userContext.UserAccounts.FirstOrDefaultAsync(m => m.UserName == userName);
            if (User != null)
            {
                if(User.Password==password)
                {
                    var AccessRights = JsonConvert.DeserializeObject<List<string>>(User.AccessRights);
                    if (AccessRights.Contains("Admin"))
                    {
                        return JsonConvert.SerializeObject(PureMedBlockChain);
                    }
                }
            }
            return "False";
        }

        [HttpPost]
        public async Task<string> SetBlockChainDifficulty(string difficulty, string userName, string password)
        {
            var User = await _userContext.UserAccounts.FirstOrDefaultAsync(m => m.UserName == userName);
            if (User != null)
            {
                if(User.Password==password)
                {
                    var AccessRights = JsonConvert.DeserializeObject<List<string>>(User.AccessRights);
                    if (AccessRights.Contains("Admin"))
                    {
                        var temp = int.TryParse(difficulty, out int diff);
                        if (temp)
                        {
                            PureMedBlockChain.Difficulty = diff;
                            var tempChains = await blockChainsDbContext.AllBlockChains.SingleOrDefaultAsync(m => m.ID == 1);
                            if (tempChains != null)
                            {
                                tempChains.Difficulty = diff;
                                blockChainsDbContext.AllBlockChains.Update(tempChains);
                                await blockChainsDbContext.SaveChangesAsync();
                                return "True";
                            }
                        }
                    }
                }
            }
            return "False";
        }

        [HttpPost]
        public async Task<string> CreateTransaction(string transaction,string userName,string password)
        {
            var User = await _userContext.UserAccounts.FirstOrDefaultAsync(m => m.UserName == userName);
            if (User != null)
            {
                if(User.Password==password)
                {
                    var AccessRights = JsonConvert.DeserializeObject<List<string>>(User.AccessRights);
                    if (AccessRights.Contains("CreateTransaction") || AccessRights.Contains("Admin"))
                    {
                        Transaction newTransaction = JsonConvert.DeserializeObject<Transaction>(transaction);
                        PureMedBlockChain.CreateTransaction(newTransaction);
                        return "True";
                    }
                }
            }
            return "False";
        }

        [HttpPost]
        public async Task<string> CreateFirstTransaction(string productInfo, string transaction, string userName, string password)
        {
            var User = await _userContext.UserAccounts.FirstOrDefaultAsync(m => m.UserName == userName);
            if (User != null)
            {
                if (User.Password == password)
                {
                    var AccessRights = JsonConvert.DeserializeObject<List<string>>(User.AccessRights);
                    if (AccessRights.Contains("CreateTransaction") || AccessRights.Contains("Admin"))
                    {
                        var pro = JsonConvert.DeserializeObject<ProductInfo>(productInfo);
                        ProductInfo newProduct = new ProductInfo()
                        {
                            ProductName = pro.ProductName,
                            ProductType = pro.ProductType,
                            ProductCreator = User.ID,
                            ProductID = pro.ProductID,
                            CreationDate = pro.CreationDate
                        };
                        await _productInfoDbContext.ProductsInfos.AddAsync(newProduct);
                        await _productInfoDbContext.SaveChangesAsync();
                        Transaction newTransaction = JsonConvert.DeserializeObject<Transaction>(transaction);
                        PureMedBlockChain.CreateTransaction(newTransaction);
                        return "True";
                    }
                }
            }
            return "False";
        }

        [HttpPost]
        public string CheckTransactionID(string ID)
        {
            foreach(var block in PureMedBlockChain.Chain)
            {
                foreach (var Transaction in block.Transactions)
                {
                    if(Transaction.ProductID==ID)
                    {
                        return "True";
                    }
                }
            }
            foreach (var Transaction in PureMedBlockChain.PendingTransactions)
            {
                if(Transaction.ProductID==ID)
                {
                    return "True";
                }
            }
            foreach (var Transaction in PureMedBlockChain.MiningTransactions)
            {
                if (Transaction.ProductID == ID)
                {
                    return "True";
                }
            }
            return "False";
        }
    }
}