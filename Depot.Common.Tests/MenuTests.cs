using Microsoft.VisualStudio.TestTools.UnitTesting;
using Depot.Common.Navigation;
using System;

[TestClass]
public class MenuTests
{
    [TestMethod]
    public void AddMenuItem_AddsItemToOptionsList()
    {
        // Arrange
        var menu = new Menu("Test", "Test Description");
        var menuItem = new Menu('1', "Item 1", "Description 1");

        // Act
        menu.AddMenuItem(menuItem);

        // Assert
        Assert.IsTrue(menu.Options.Contains(menuItem));
    }

    [TestMethod]
    public void AddMenuItem_ThrowsExceptionWhenReservedKeyIsUsed()
    {
        // Arrange
        var menu = new Menu("Test", "Test Description");
        var menuItem = new Menu('0', "Item 0", "Description 0");

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => menu.AddMenuItem(menuItem));
    }
    

    [TestMethod]
    public void Return_SetsActiveItemToParentMenu()
    {
        // Arrange
        var parentMenu = new Menu("Parent", "Parent Description");
        var childMenu = new Menu('1', "Child", "Child Description", null, parentMenu);
        parentMenu.AddMenuItem(childMenu);

        // Act
        childMenu.Return();

        // Assert
        Assert.AreEqual(parentMenu, parentMenu.ActiveItem);
    }
}