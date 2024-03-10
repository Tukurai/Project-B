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
            AddStandardOptions();

            IsShowing = true;
            do
            {
                Console.Clear();
                Console.WriteLine(ActiveItem.Title);
                Console.WriteLine(ActiveItem.Description);
                Console.WriteLine();

                foreach (var item in ActiveItem.Options)
                {
                    Console.WriteLine($"{item.KeyChar}. {item.Title} | {item.Description}");
                }

                Console.WriteLine();

                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();

                var selectedOption = ActiveItem.Options.Find(x => x.KeyChar == input);
                if (selectedOption != null)
                {
                    if (selectedOption.Options.Any())
                    {
                        ActiveItem = selectedOption;
                    }
                    
                    if (selectedOption.Action != null)
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

        public virtual void Return()
        {
            var mainMenu = this;
            while (mainMenu.Parent != null)
            {
                mainMenu = mainMenu.Parent;
            }

            mainMenu.ActiveItem = ActiveItem?.Parent;
        }

        public virtual void Shutdown()
        {
            Environment.Exit(0);
        }

        protected void AddStandardOptions()
        {
            foreach (var option in Options)
            {
                option.AddStandardOptions();
            }

            AddReturnOrShutdown();
            AddAdditionalOptions();
        }

        protected virtual void AddReturnOrShutdown()
        {
            if (Parent == null)
            {
                Options.Add(new Menu('0', "Afsluiten", $"Applicatie afsluiten.", Shutdown, this));
                return;
            }

            if (Options.Any())
            {
                Options.Add(new Menu('0', "Terug", $"Terug naar {Parent.Title}.", Return, this));
            }
        }

        protected virtual void AddAdditionalOptions()
        {
        }
    }
}
