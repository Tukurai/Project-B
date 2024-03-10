using System.Text;

namespace Depot.Common.Navigation
{
    public class PagingMenu : Menu
    {
        public int PageNumber { get; set; }
        public int MaxItemsPerPage { get; set; }
        public IEnumerable<Menu[]> Pages { get; private set; } = new List<Menu[]>();

        public PagingMenu(string title, string description, Menu? parent = null) : base(title, description, parent)
        {
            PageNumber = 0;
            MaxItemsPerPage = 7;
        }

        public PagingMenu(char keyChar, string title, string description, Action? action = null, Menu? parent = null)
            : this(title, description, parent)
        {
            KeyChar = keyChar;
            Action = action;
        }

        public void SetListItems(List<Menu> listItems)
        {
            Pages = listItems.Chunk(MaxItemsPerPage);
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

                if (ActiveItem.GetType() == typeof(PagingMenu))
                {
                    if (Pages.Any())
                    {
                        foreach (var item in Pages.Skip(PageNumber).First())
                        {
                            Console.WriteLine($"{item.KeyChar}. {item.Title} | {item.Description}");
                        }
                    }
                }

                Console.WriteLine();

                var input = Console.ReadKey().KeyChar;
                Console.WriteLine();

                var selectedOption = ActiveItem.Options.Find(x => x.KeyChar == input);
                if (selectedOption == null && int.TryParse(input.ToString(), out int indexSelected))
                {
                    selectedOption = Pages.Skip(PageNumber).First().Skip(indexSelected - 3).First();
                }

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

        protected override void AddAdditionalOptions()
        {
            Options.Add(new Menu('1', "Vorige", $"Vorige items bekijken uit de lijst.", Previous, this));
            Options.Add(new Menu('2', "Volgende", $"Volgende items bekijken uit de lijst.", Next, this));
        }

        private void Previous()
        {
            PageNumber--;
            if (PageNumber < 0)
            {
                PageNumber = 0;
            }
        }

        private void Next()
        {
            PageNumber++;
            var pageCount = Pages.Count() - 1; // Zero based
            if (PageNumber >= pageCount)
            {
                PageNumber = pageCount;
            }
        }
    }
}
