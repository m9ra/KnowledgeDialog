
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

using DialogTesting.Utilities;


namespace DialogTesting
{
    [TestClass]
    public class KnowledgeClassifierTests
    {
        /// <summary>
        /// Tests that classifier can distinguish classes according to neighbour nodes.
        /// </summary>
        [TestMethod]
        public void NodeClassification()
        {
            Graphs.Alphabet
                .Advice("A", "capital letter")
                .Advice("B", "capital letter")
                .Advice("c", "small letter")

                .Assert("capital letter", "A", "B", "C", "D")
                .Assert("small letter", "a", "b", "c", "d");
        }

        /// <summary>
        /// Tests that classifier can distinguish classes according to neighbour nodes.
        /// </summary>
        [TestMethod]
        public void MissingEdgeClassification()
        {
            Graphs.Alphabet2
                .Advice("A", "capital letter")
                .Advice("B", "capital letter")
                .Advice("c", "small letter")

                .Assert("capital letter", "A", "B", "C", "D")
                .Assert("small letter", "a", "b", "c", "d");
        }


        /// <summary>
        /// Tests that classifier can distinguish classes according to edges.
        /// </summary>
        [TestMethod]
        public void EdgeClassification()
        {
            Graphs.Names
                .Advice("Pavel", "A")
                .Advice("Pepa", "A")
                .Advice("Zuzana", "B")

                .Assert("A", "Pavel", "Pepa", "Ondra", "David")
                .Assert("B", "Zuzana", "Jitka", "Nikola", "Tereza");
        }

        /// <summary>
        /// Tests that classifier can infer default class from a single advice.
        /// </summary>
        [TestMethod]
        public void DefaultClassification()
        {
            Graphs.Alphabet
                .Advice("A", "letter")

                .Assert("letter", "A", "B", "C", "D")
                .Assert("letter", "a", "b", "c", "d");
        }

    }
}
