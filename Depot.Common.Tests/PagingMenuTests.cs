using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Depot.Common.Navigation;


[TestClass]
public class PagingMenuTests
{
    [TestMethod]
    public void TestCanCreateBasicPagingMenu()
    {
        // Arrange
        var pagingMenu = new PagingMenu("Test", "Test Description");
        var listItems = new List<Menu>
        {
            new Menu("Item 1", "Description 1"),
            new Menu("Item 2", "Description 2"),
            new Menu("Item 3", "Description 3"),
            new Menu("Item 4", "Description 4"),
            new Menu("Item 5", "Description 5"),
            new Menu("Item 6", "Description 6"),
            new Menu("Item 7", "Description 7"),
            new Menu("Item 8", "Description 8")
        };

        // Act
        pagingMenu.SetListItems(listItems);

        // Assert
        
        Assert.AreEqual(2, pagingMenu.Pages.Count());
    }
    

    [TestMethod]
    public void AddMenuItem_ThrowsExceptionWhenReservedKeyIsUsed()
    {
        // Arrange
        var pagingMenu = new PagingMenu("Test", "Test Description");
        var menuItem = new Menu('1', "Item 1", "Description 1");

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => pagingMenu.AddMenuItem(menuItem));
    }


}