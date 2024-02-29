
namespace Depot.Common.Navigation
{
    public class SubMenu : Menu
    {
        public char KeyChar { get; set; }
        public Action? Action { get; set; }

        public SubMenu(char keyChar, string title, string description, Action? action = null, Menu? parent = null) 
            : base(title, description, parent)
        {
            KeyChar = keyChar;
            Action = action;
        }
    }
}
