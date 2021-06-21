using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace VerySimpleShop.Services
{
    public interface ICustomerRepository
    {
        public Entities.CustomerEntity GetById(int id);
        public void AddCustomer(Entities.CustomerEntity entity);
    }

    public class OrderService
    {
        private readonly ICustomerRepository repository;

        public ICustomerRepository Repository => this.repository;   // for tests only!

        public OrderService(ICustomerRepository repository)
        {
            this.repository = repository;
        }

        public bool ProcessOrder(Dtos.OrderRequest request)
        {
            if (request.Customer is Dtos.RegisteredCustomer registeredCustomer)
            {
                var customer = this.Repository.GetById(registeredCustomer.Id);
                customer.PlaceOrder();
                return false;
            }
            else if (request.Customer is Dtos.NewCustomer newCustomer)
            {
                var customer = new Entities.CustomerEntity(newCustomer.FirstName, newCustomer.LastName, newCustomer.DateOfBirth);
                Repository.AddCustomer(customer);
                customer.PlaceOrder();
                return true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class FixtureTests
    {
        private readonly IFixture fixture = new Fixture();

        public FixtureTests()
        {
            fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());
        }

        [Fact]
        public void CanCreateOrderserviceForTest()
        {
            var service = fixture.Create<OrderService>();

            service.Should().NotBeNull();
            service.Repository.Should().NotBeNull();
        }
    }

    public class OrderServiceTests
    {
        private readonly IFixture fixture = new Fixture();

        public OrderServiceTests()
        {
            fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());
            fixture.Customize(new Dtos.OrderWithPolishCustomerAndItemPrice10Customization());
        }

        [Fact]
        public void ForNewCustomerReturnsTrue()
        {
            var request = fixture.Create<Dtos.OrderRequest>();
            var service = fixture.Create<OrderService>();

            var result = service.ProcessOrder(request);

            result.Should().BeTrue();
        }

        [Fact]
        public void ForNewCustomerAddsToRepository()
        {
            //var repositoryMock = fixture.Create<Mock<ICustomerRepository>>();
            var repositoryMock = fixture.Freeze<Mock<ICustomerRepository>>();
            var request = fixture.Create<Dtos.OrderRequest>();
            var service = fixture.Create<OrderService>();

            var result = service.ProcessOrder(request);

            repositoryMock.Verify(r => r.AddCustomer(It.Is<Entities.CustomerEntity>(entity => MatchEntity(entity, request))), Times.Once);
        }

        private static bool MatchEntity(Entities.CustomerEntity entity, Dtos.OrderRequest request)
        {
            Dtos.NewCustomer newCustomer = ((Dtos.NewCustomer)request.Customer);
            return entity.FirstName == newCustomer.FirstName && entity.LastName == newCustomer.LastName;
        }
    }

    public class OrderServiceForRegisteredCUstomerTests
    {
        private readonly IFixture fixture = new Fixture();

        public OrderServiceForRegisteredCUstomerTests()
        {
            fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization() { ConfigureMembers = true });
            fixture.Customize(new Dtos.OrderWithPolishCustomerAndItemPrice10Customization());
        }

        [Fact]
        public void ForRegisteredCustomerReturnsFalse()
        {
            var request = fixture.Build<Dtos.OrderRequest>()
                .With(x => x.Customer, fixture.Create<Dtos.RegisteredCustomer>())
                .Create();
            var service = fixture.Create<OrderService>();

            var result = service.ProcessOrder(request);

            result.Should().BeFalse();
        }
    }


    public class AutoMoqDataAttribute : AutoFixture.Xunit2.AutoDataAttribute
    {
        static private Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoFixture.AutoMoq.AutoMoqCustomization());
            fixture.Customize(new Dtos.OrderWithPolishCustomerAndItemPrice10Customization());

            return fixture;
        }

        public AutoMoqDataAttribute()
            : base(() => CreateFixture())
        {
        }
    }

    public class OrderServiceDeclarativeWayTests
    {
        [Theory]
        [AutoMoqData]
        public void ForNewCustomerReturnsTrue(Dtos.OrderRequest request, OrderService service)
        {
            var result = service.ProcessOrder(request);

            result.Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public void ForNewCustomerAddsToRepository(
            [AutoFixture.Xunit2.Frozen] Mock<ICustomerRepository> repositoryMock,
            Dtos.OrderRequest request,
            OrderService service)
        {
            var result = service.ProcessOrder(request);

            repositoryMock.Verify(r => r.AddCustomer(
                It.Is<Entities.CustomerEntity>(entity => entity.FirstName == ((Dtos.NewCustomer)request.Customer).FirstName)),
                Times.Once);
        }
    }
}
