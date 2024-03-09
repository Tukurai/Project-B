namespace Depot.Common.Navigation
{
    public class PagingMenu<T> : Menu
    {
        public List<T> ListItems { get; set; }
        public int PageNumber { get; set; }
        public int MaxItemsPerPage { get; set; }

        public PagingMenu(string title, string description, List<T> listItems, Menu? parent = null) : base(title, description, parent)
        {
            ListItems = listItems;
            PageNumber = 1;
            MaxItemsPerPage = 7;
        }

        public PagingMenu(char keyChar, string title, string description, List<T> listItems, Action? action = null, Menu? parent = null)
            : this(title, description, listItems, parent)
        {
            KeyChar = keyChar;
            Action = action;
        }

        public override void AddMenuItem(Menu navigationItem)
        {
            List<char> unavailableKeys = new List<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            // Key 1 = previous page,
            // Key 2 = next page,
            // Key 3-9 = select item

            if (navigationItem.KeyChar != null && unavailableKeys.Contains(navigationItem.KeyChar.Value))
            {
                throw new ArgumentException("The key specified is a PagingMenu specific key and can't be used.");
            }

            base.AddMenuItem(navigationItem);
        }

        public override void Show()
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

        protected override void AddAdditionalOptions()
        {
            //TODO: Add paging options
        }
    }
}
