using System;
using System.Linq.Expressions;
using NUnit.Framework;
using SalesforceMagic.LinqProvider;
using Shouldly;

namespace SalesforceMagicTests
{
    public class SoqlVisitorTests
    {
        [Test]
        public void ShouldConvertBinaryExpressionsToSoql()
        {
            TestExpression(a => a.IsDeleted).ShouldBe("IsDeleted = True");
            TestExpression(a => a.IsDeleted == true).ShouldBe("(IsDeleted = True)");

            TestExpression(a => !a.IsDeleted).ShouldBe("(IsDeleted != True)");
            TestExpression(a => a.IsDeleted != true).ShouldBe("(IsDeleted != True)");
            TestExpression(a => a.IsDeleted == false).ShouldBe("(IsDeleted = False)");
            TestExpression(a => a.IsDeleted != false).ShouldBe("(IsDeleted != False)");

            TestExpression(a => !a.IsDeleted && !a.IsClosed).ShouldBe("((IsDeleted != True) AND (IsClosed != True))");
            TestExpression(a => !a.IsDeleted && a.Id == "034687OAEB").ShouldBe("((IsDeleted != True) AND (Id = '034687OAEB'))");
            TestExpression(a => a.Id == "034687OAEB" && !a.IsDeleted).ShouldBe("((Id = '034687OAEB') AND (IsDeleted != True))");
        }

        private string TestExpression(Expression<Func<TestAccount, bool>> expression)
        {
            return SOQLVisitor.ConvertToSOQL(expression);
        }

        private class TestAccount
        {
            public string Id { get; set; }

            public bool IsDeleted { get; set; }

            public bool IsClosed { get; set; }
        }
    }
}
