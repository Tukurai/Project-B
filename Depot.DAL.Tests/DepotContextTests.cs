using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Depot.DAL;
using Depot.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

[TestClass]
public class DepotContextTests
{
    private DbSet<User> _mockUsers;
    private DbSet<Ticket> _mockTickets;
    private DbSet<Tour> _mockTours;
    private DbSet<Group> _mockGroups;
    private DepotContext _depotContext;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockUsers = Substitute.For<DbSet<User>>();
        _mockTickets = Substitute.For<DbSet<Ticket>>();
        _mockTours = Substitute.For<DbSet<Tour>>();
        _mockGroups = Substitute.For<DbSet<Group>>();

        _depotContext = new DepotContext()
        {
            Users = _mockUsers,
            Tickets = _mockTickets,
            Tours = _mockTours,
            Groups = _mockGroups
        };
    }

    [TestMethod]
    public void LoadJson_WhenUsersJsonExists()
    {
        // Arrange
        var usersJson = "[{\"Id\":1,\"Name\":\"Test User\"}]";
        File.WriteAllText("TestUsers.json", usersJson);

        // Act
        _depotContext.LoadJson();

        // Assert
        _mockUsers.Received().AddRange(Arg.Any<IEnumerable<User>>());

        // Clean up
        File.Delete("TestUsers.json");
    }
}