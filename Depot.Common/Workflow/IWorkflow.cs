using Depot.DAL;

namespace Depot.Common.Workflow
{
    public interface IWorkflow
    {
        DepotContext Context { get; set; }

        string Validate(out bool valid);
    }
}
