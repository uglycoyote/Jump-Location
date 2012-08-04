﻿using System;
using System.Threading;
using Moq;
using Should;
using Xunit;

namespace Jump.Location.Specs
{
    public class CommandControllerSpec
    {
        public class DescribeUpdateTimes
        {
            private readonly Mock<IDatabase> dbMock;
            private readonly Mock<IFileStoreProvider> fsMock;
            private readonly CommandController controller;

            public DescribeUpdateTimes()
            {
                dbMock = new Mock<IDatabase>();
                fsMock = new Mock<IFileStoreProvider>();
                controller = new CommandController(dbMock.Object, fsMock.Object);
            }

            [Fact]
            public void It_saves_eventually()
            {
                var recordMock = new Mock<IRecord>();
                recordMock.SetupAllProperties();
                dbMock.Setup(x => x.GetByFullName(It.IsAny<string>())).Returns(recordMock.Object);

                controller.UpdateLocation("foo");
                controller.UpdateLocation("bar");
                Thread.Sleep(30);
                fsMock.Verify(x => x.Save(dbMock.Object));
            }

            [Fact]
            public void It_updates_weights()
            {
                var recordMock = new Mock<IRecord>();
                recordMock.SetupAllProperties();
                dbMock.Setup(x => x.GetByFullName("foo")).Returns(recordMock.Object);

                controller.UpdateLocation("foo");
                Thread.Sleep(30);
                controller.UpdateLocation("foo");

                recordMock.Verify(x => x.AddTimeSpent(It.IsAny<TimeSpan>()), Times.Once());
            }
        }

        public class DescribeFindBest
        {
            private readonly Mock<IDatabase> dbMock;
            private readonly Mock<IFileStoreProvider> fsMock;
            private readonly CommandController controller;

            public DescribeFindBest()
            {
                dbMock = new Mock<IDatabase>();
                fsMock = new Mock<IFileStoreProvider>();
                controller = new CommandController(dbMock.Object, fsMock.Object);
            }

            [Fact]
            public void Exact_match_on_last_segment_from_a_DB_of_one()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void Exact_match_on_last_segment_from_a_DB_of_many()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\Kerianne", 10M), 
                        new Record(@"FS::C:\Users\lthompson", 10M), 
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }

            [Fact]
            public void No_match_returns_null()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("notfound");
                record.ShouldBeNull();
            }

            [Fact]
            public void Exact_match_on_last_segment_is_case_insensitive()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\TKELLOGG", 10M), 
                    });

                var record = controller.FindBest("tkellogg");
                record.Path.ShouldEqual(@"C:\Users\TKELLOGG");
            }

            [Fact]
            public void Last_segment_only_starts_with()
            {
                dbMock.Setup(x => x.Records).Returns(new[]
                    {
                        new Record(@"FS::C:\Users\tkellogg", 10M), 
                    });

                var record = controller.FindBest("t");
                record.Path.ShouldEqual(@"C:\Users\tkellogg");
            }
        }
    }
}
