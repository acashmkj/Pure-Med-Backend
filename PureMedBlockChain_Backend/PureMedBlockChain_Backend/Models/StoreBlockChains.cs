using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureMedBlockChain_Backend.Models
{
    //Class for storing blockchains in database
    public class StoreBlockChains
    {
        public int ID { get; set; }

        //Difficulty of blockChain
        public int Difficulty { get; set; }

        //Main supply block chain
        public string PureMedBlockChain { get; set; }

        //PureMed blockChain of first verifier
        public string Verifier_1BlockChain { get; set; }

        //PureMed blockChain of second verifier
        public string Verifier_2BlockChain { get; set; }

        //PureMed blockChain of third verifier
        public string Verifier_3BlockChain { get; set; }
    }
}
