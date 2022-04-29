namespace Neos.SourceGenerator.CLI;

partial class Program
{
    static void Main(string[] args)
    {
        var user = new UserClass() { Firstname = "Bob", Lastname = "Lennon", Id = Guid.NewGuid() };
        var otherUser = new UserClass.Builder().WithFirstname("Bob").WithLastname("Lennon").GetUserClass();
        HelloFrom("Generated Code");
    }

    static partial void HelloFrom(string name);
}