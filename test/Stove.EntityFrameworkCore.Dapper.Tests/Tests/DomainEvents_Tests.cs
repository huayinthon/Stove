﻿using System;
using System.Threading.Tasks;

using Shouldly;

using Stove.Commands;
using Stove.Dapper.Repositories;
using Stove.Domain.Repositories;
using Stove.Domain.Uow;
using Stove.EntityFrameworkCore.Dapper.Tests.Domain;
using Stove.EntityFrameworkCore.Dapper.Tests.Domain.Events;
using Stove.Events.Bus;

using Xunit;

namespace Stove.EntityFrameworkCore.Dapper.Tests.Tests
{
    public class DomainEvents_Tests : StoveEfCoreDapperTestApplicationBase
    {
        private readonly IDapperRepository<Blog> _blogDapperRepository;
        private readonly IRepository<Blog> _blogRepository;
        private readonly IEventBus _eventBus;

        public DomainEvents_Tests()
        {
            Building(builder => { }).Ok();

            _blogRepository = The<IRepository<Blog>>();
            _blogDapperRepository = The<IDapperRepository<Blog>>();
            _eventBus = The<IEventBus>();
        }

        [Fact]
        public void Should_Trigger_Domain_Events_For_Aggregate_Root()
        {
            var isTriggered = false;
            string correlationId = Guid.NewGuid().ToString();
            using (The<IStoveCommandContextAccessor>().Use(correlationId))
            {
                using (IUnitOfWorkCompleteHandle uow = The<IUnitOfWorkManager>().Begin())
                {
                    _eventBus.Register<BlogUrlChangedEvent>((@event, headers) =>
                    {
                        @event.Url.ShouldBe("http://testblog1-changed.myblogs.com");
                        headers["CausationId"].ShouldBe(correlationId);
                        isTriggered = true;
                    });

                    //Act
                    Blog blog1 = _blogRepository.Single(b => b.Name == "test-blog-1");
                    blog1.ChangeUrl("http://testblog1-changed.myblogs.com");
                    _blogRepository.Update(blog1);

                    _blogDapperRepository.Get(blog1.Id).ShouldNotBeNull();

                    uow.Complete();
                }
            }

            //Arrange

            //Assert
            isTriggered.ShouldBeTrue();
        }

        [Fact]
        public async Task should_trigger_event_on_inserted()
        {
            var triggerCount = 0;
            string correlationId = Guid.NewGuid().ToString();

            The<IEventBus>().Register<BlogCreatedEvent>((@event, headers) =>
            {
                @event.Name.ShouldBe("OnSoftware");
                headers["CausationId"].ShouldBe(correlationId);
                triggerCount++;
            });

            using (The<IStoveCommandContextAccessor>().Use(correlationId))
            {
                using (IUnitOfWorkCompleteHandle uow = The<IUnitOfWorkManager>().Begin())
                {
                    _blogRepository.Insert(new Blog("OnSoftware", "www.stove.com"));
                    uow.Complete();
                }
            }

            triggerCount.ShouldBe(1);
        }
    }
}
