using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Xunit;

namespace SortingHat.Test
{
    public class CodeExperiments
    {
        private int _count = 0;
        private bool TrueSideEfffect()
        {
            ++_count;

            return true;
        }

        private bool FalseSideEfffect()
        {
            ++_count;

            return false;
        }

        [Fact]
        public void TheAllIteratorDoesShortcircuitEvaluation()
        {
            _count = 0;
            var booleans = new List<Func<bool>> { TrueSideEfffect, TrueSideEfffect, FalseSideEfffect, TrueSideEfffect, TrueSideEfffect };

            Assert.False(booleans.All(p => p()));

            // The last two sideeffects are not executed
            Assert.Equal(3, _count);
        }

        [Fact]
        public void NullAndObjectPattern()
        {
            var maybeNullStrings = new List<string?> { null, "Cool", "1337", null, "Test" };

            foreach (var maybeNullString in maybeNullStrings)
            {
                if (maybeNullString is { } notNullString)
                {
                    // this string is not nullable!
                    Assert.NotNull(notNullString);
                }

                if (maybeNullString is null)
                {
                    Assert.Null(maybeNullString);
                }
            }
        }

        interface IDo
        {
            string GiveMe();
        }

        class Do1 : IDo
        {
            public string GiveMe() => "one";
        }

        class Do2 : IDo
        {
            public string GiveMe() => "two";
        }

        [Fact]
        void OverrideAutoFacRegistration()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Do1>().As<IDo>();
            builder.RegisterType<Do2>().As<IDo>();

            var container = builder.Build();

            var doInstance = container.Resolve<IDo>();

            Assert.Equal("two", doInstance.GiveMe());
        }
    }
}
