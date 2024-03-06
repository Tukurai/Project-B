namespace Depot.Common.Navigation
{
    public class Menu
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<SubMenu> Options { get; set; } = new List<SubMenu>();
        public Menu ActiveItem { get; set; }
        public Menu? Parent { get; set; } = null;
        public bool IsShowing { get; set; } = false;

        public Menu(string title, string description, Menu? parent = null)
        {
            Title = title;
            Description = description;
            Parent = parent;
            ActiveItem = this;
        }

        public void AddMenuItem(SubMenu navigationItem)
        {
            navigationItem.Parent = this;
            Options.Add(navigationItem);
        }

        public void Show()
        {
            IsShowing = true;
            do
            {
                Console.Clear();
                Console.WriteLine(ActiveItem.Title);
                Console.WriteLine(ActiveItem.Description);
                Console.WriteLine();

                foreach (var item in ActiveItem.Options)
                {
                    Console.WriteLine($"{item.KeyChar}. {item.Title}");
                }

                Console.WriteLine();

                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();

                var selectedOption = ActiveItem.Options.Find(x => x.KeyChar == input);
                if (selectedOption != null)
                {
                    if (selectedOption.Action == null)
                    {
                        ActiveItem = selectedOption;
                    }
                    else
                    {
                        selectedOption.Action();
                    }
                }
            } while (IsShowing);
        }

        public void Reset()
        {
            ActiveItem = this;
        }
    }
}
