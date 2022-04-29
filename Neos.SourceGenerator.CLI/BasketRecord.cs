
using Neos.SourceGenerator.Abstraction;

namespace Neos.SourceGenerator.CLI;

[Builder]
public partial record BasketRecord
{
    public Guid Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }   
}




