using FuturePath.Data;
using FuturePath.Presentation;

namespace FuturePath;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Create the database (and fill it with test data) the first time the program runs.
        DatabaseInitializer.Initialize();

        // Open the Welcome screen. When it closes, the program ends.
        Application.Run(new WelcomeForm());
    }
}