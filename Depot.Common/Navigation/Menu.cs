namespace Depot.Common.Navigation
{
    public class Menu
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Menu> Options { get; set; } = new List<Menu>();
        public Menu ActiveItem { get; set; }
        public Menu? Parent { get; set; } = null;
        public bool IsShowing { get; set; } = false;

        public char? KeyChar { get; set; } = null;
        public Action? Action { get; set; } = null;

        public Menu(string title, string description, Menu? parent = null)
        {
            Title = title;
            Description = description;
            Parent = parent;
            ActiveItem = this;

            AddStandardOptions();
        }


        public Menu(char keyChar, string title, string description, Action? action = null, Menu? parent = null)
            : this(title, description, parent)
        {
            KeyChar = keyChar;
            Action = action;
            ActiveItem = this;
        }

        public virtual void AddMenuItem(Menu navigationItem)
        {
            List<char> unavailableKeys = new List<char> { '0' };
            // Key 0 = back to parent or close,

            if (navigationItem.KeyChar != null && unavailableKeys.Contains(navigationItem.KeyChar.Value))
            {
                throw new ArgumentException("The key specified is a Menu specific key and can't be used.");
            }

            navigationItem.Parent = this;
            Options.Add(navigationItem);
        }

        public virtual void Show()
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

        public virtual void Reset()
        {
            ActiveItem = this;
        }

        private void AddStandardOptions()
        {
            if (Parent == null)
            {
                Options.Add(new Menu('0', "Terug", $"Afsluiten.", () => { Environment.Exit(0); }));
            } else {
                Options.Add(new Menu('0', "Terug", $"Terug naar {Parent.Title}.", () => { ActiveItem = Parent; }));
            }

            AddAdditionalOptions();
        }

        protected virtual void AddAdditionalOptions() {}
    }
}
