using Depot.DAL;

namespace Depot.Common.Workflow
{
    public class Workflow : IWorkflow
    {
        public DepotContext Context { get; set; }

        public Workflow(DepotContext context)
        {
            Context = context;
        }
    }
}
