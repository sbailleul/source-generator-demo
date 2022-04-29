namespace Neos.SourceGenerator.CLI;

partial class Program
{
    static void Main(string[] args)
    {
        var user = new User() { Firstname = "Bob", Lastname = "Lennon", Id = Guid.NewGuid() };
        var otherUser = new User.Builder().WithFirstname("Bob").WithLastname("Lennon").GetUser();
        HelloFrom("Generated Code");
    }

    static partial void HelloFrom(string name);
}