using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VerySimpleShop.Dtos
{
    public class OrderRequest
    {
        public CustomerBase Customer { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }

    public abstract class CustomerBase
    {
    }

    public class RegisteredCustomer : CustomerBase
    {
        public int Id { get; set; }
    }

    public class NewCustomer : CustomerBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Country { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class FixtureTests
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void CanBuildNewCustomer()
        {
            var customer = fixture.Create<NewCustomer>();

            customer.FirstName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CustomerShouldBeDawid()
        {
            var customer = fixture.Build<NewCustomer>()
                .With(x => x.FirstName, "Dawid")
                .With(x => x.Country, "Poland")
                .Create();

            customer.FirstName.Should().Be("Dawid");
        }

        [Fact]
        public void CollectionOfThreeCustomersIsPossibleByDefault()
        {
            var customers = fixture.Build<NewCustomer>()
                .With(x => x.Country, "Poland")
                .CreateMany();

            customers.Count().Should().Be(3);
        }

        [Fact]
        public void CollectionCountCanBeChanged()
        {
            var customers = fixture.Build<NewCustomer>()
                .With(x => x.Country, "Poland")
                .CreateMany(5);

            customers.Count().Should().Be(5);
        }

        [Theory]
        [AutoFixture.Xunit2.AutoData]
        public void CanInjectNewCustomer(NewCustomer customer)
        {
            customer.FirstName.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [AutoFixture.Xunit2.InlineAutoData(3)]
        public void CanInjectNewCustomerAndInlineParameter(int number, NewCustomer customer)
        {
            customer.FirstName.Length.Should().BeGreaterThan(number);
        }
    }

    public class DtoProcessor
    {
        public int CalculateOrderSum(OrderRequest request)
        {
            return request.Items.Sum(x => x.Price);
        }
    }

    public class DtoProcessorTests
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void SumShouldBeNotNegative()
        {
            var order = fixture.Build<OrderRequest>()
                .With(x => x.Customer, fixture.Create<NewCustomer>())
                .Create();

            var sum = new DtoProcessor().CalculateOrderSum(order);

            sum.Should().BePositive();
        }

        [Fact]
        public void SumShouldBeNotNegativeWithCustomize()
        {
            fixture.Customize<OrderRequest>(composer => composer
                .With(x => x.Customer, fixture.Create<NewCustomer>())
            );

            var order = fixture.Create<OrderRequest>();

            var sum = new DtoProcessor().CalculateOrderSum(order);

            sum.Should().BePositive();
        }

        [Fact]
        public void SumShouldBe30()
        {
            fixture.Customize<OrderRequest>(composer => composer
                .With(x => x.Customer, fixture.Create<NewCustomer>())
            );

            fixture.Customize<OrderItem>(composer => composer.With(x => x.Price, 10));
            var order = fixture.Create<OrderRequest>();

            var sum = new DtoProcessor().CalculateOrderSum(order);

            sum.Should().Be(30);
        }

        [Fact]
        public void SumShouldBe50()
        {
            // Code below won't work if we use Build!
            //fixture.Customize<OrderRequest>(composer => composer
            //    .With(x => x.Customer, fixture.Create<NewCustomer>())
            //);

            fixture.Customize<OrderItem>(composer => composer.With(x => x.Price, 10));
            var order = fixture.Build<OrderRequest>()
                .With(x => x.Customer, fixture.Create<NewCustomer>())
                .With(x => x.Items, fixture.CreateMany<OrderItem>(5).ToList())
                .Create();

            var sum = new DtoProcessor().CalculateOrderSum(order);

            sum.Should().Be(50);
        }

        //public DtoProcessorTests()
        //{
        //    fixture.Customize<OrderRequest>(composer => composer
        //        .With(x => x.Customer, fixture.Create<NewCustomer>())
        //    );
        //}
    }

    public class OrderWithPolishCustomerAndItemPrice10Customization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var customer = fixture.Build<NewCustomer>()
                .With(x => x.Country, "Poland")
                .Create();

            fixture.Customize<OrderItem>(composer => composer.With(x => x.Price, 10));

            fixture.Customize<OrderRequest>(composer => composer
                .With(x => x.Customer, customer)
            );
        }
    }

    public class FixtureCustomizationTests
    {
        private readonly IFixture fixture = new Fixture();

        public FixtureCustomizationTests()
        {
            fixture.Customize(new OrderWithPolishCustomerAndItemPrice10Customization());
        }

        [Fact]
        public void CustomerIsFromPoland()
        {
            var request = fixture.Create<OrderRequest>();

            var newCustomer = request.Customer as NewCustomer;
            newCustomer.Country.Should().Be("Poland");
        }

        [Fact]
        public void AllItemShouldHavePrice10()
        {
            var request = fixture.Create<OrderRequest>();

            request.Items.Should().OnlyContain(x => x.Price == 10);
        }
    }
}
