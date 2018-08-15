using SortingHat.API;
using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using System;
using System.Linq;
using Xunit;

namespace SortingHat.Test
{
    public class ParserTest
    {
        [Fact]
        public void EmptySearch()
        {
            var parser = new QueryParser("");
            var parseTree = parser.Parse();

            Assert.Null(parseTree);
        }

        [Fact]
        public void SingleTag()
        {
            var parser = new QueryParser(":tax:2018");
            var parseTree = parser.Parse();

            Assert.IsType<TagNode>(parseTree);

            Assert.Equal(":tax:2018", (parseTree as TagNode).Tag);
        }

        [Fact]
        public void SimpleQuery()
        {
            var parser = new QueryParser(":test or true and (:movie or not :blue)");
            var visitor = new ToStringVisitor(OperatorType.Logical);

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:test ∨ (true ∧ (:movie ∨ ¬:blue)))", visitor.Result);
        }

        [Fact]
        public void PrecedenceTest()
        {
            var parser = new QueryParser(":A || :B && :C");
            var visitor = new ToStringVisitor(OperatorType.Logical);

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(:A ∨ (:B ∧ :C))", visitor.Result);
        }

        [Fact]
        public void OrAssociativityTest()
        {
            var parser = new QueryParser(":A || :B || :C || :D");
            var visitor = new ToStringVisitor(OperatorType.Programming);

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(((:A || :B) || :C) || :D)", visitor.Result);
        }

        [Fact]
        public void AndAssociativityTest()
        {
            var parser = new QueryParser(":A && :B && :C && :D");
            var visitor = new ToStringVisitor(OperatorType.Programming);

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(((:A && :B) && :C) && :D)", visitor.Result);
        }

        [Fact]
        public void MultipleNotTest()
        {
            var parser = new QueryParser("!!!:A && !!:B");
            var visitor = new ToStringVisitor();

            var ir = parser.Parse();

            ir.Accept(visitor);
            Assert.Equal("(not(not(not(:A))) and not(not(:B)))", visitor.Result);
        }

        [Fact]
        public void EmptyStringNextNode()
        {
            var parser = new QueryParser("");
            var ir = parser.Parse();
            var next = parser.NextNode();

            Assert.IsType<TagNode>(next.First());
        }

        [Fact]
        public void SQLQuery()
        {
            var parser = new QueryParser(":tax:2016 || :cool && :audio:original");
            var visitor = new DB.SearchQueryVisitor(new DB.SQLiteDB(new DatabaseSettings { DBName = "hat", DBPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) }));

            var ir = parser.Parse();

            ir.Accept(visitor);

            Assert.Equal("SELECT FilePaths.Path, Files.Hash, Files", visitor.Result);
        }
    }
}
