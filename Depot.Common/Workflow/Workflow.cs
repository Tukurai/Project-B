using Depot.DAL;

namespace Depot.Common.Workflow
{
    public abstract class Workflow : IWorkflow
    {
        public DepotContext Context { get; set; }

        public Workflow(DepotContext context)
        {
            Context = context;
        }

        public abstract string Validate(out bool valid);
    }
}
