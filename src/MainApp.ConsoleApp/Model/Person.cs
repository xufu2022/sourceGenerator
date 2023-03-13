using MainApp.Generators;

namespace MainApp.ConsoleApp.Model
{
    [GenerateToString]
    public partial class Person
    {
        public string? FirstName { get; set; } //jwiwe rwe r

        public string? LastName { get; set; }

        public int Age { get; set; }
    }
}