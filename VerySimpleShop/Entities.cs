using AutoFixture;
using FluentAssertions;
using System;
using Xunit;

namespace VerySimpleShop.Entities
{
    public class CustomerEntity
    {
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }
        public bool OrderPlaced { get; private set; }

        public CustomerEntity(string firstName, string lastName, DateTime dateOfBirth)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.DateOfBirth = dateOfBirth;
        }

        public void PlaceOrder()
        {
            this.OrderPlaced = true;
        }
    }

    public class FixtureTests
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void CanBuildNewCustomer()
        {
            var customer = fixture.Create<CustomerEntity>();

            customer.FirstName.Should().NotBeNullOrEmpty();
            customer.LastName.Should().NotBeNullOrEmpty();
        }
    }
}
