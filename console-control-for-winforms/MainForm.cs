namespace console_control_for_winforms
{
    enum GameState
    {
        DisplayLogo,
        PromptName,
        Intro,
        Play,
    }
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Console.KeyDown += onConsoleKeyDown;
            _ = execGameLoop();
        }
        private GameState GameState = GameState.DisplayLogo;
        private async Task execGameLoop()
        {
            while (true)
            {
                switch (GameState)
                {
                    case GameState.DisplayLogo:
                        Console.Font = new Font("Consolas", 10);
                        Console.Text =
@"
                           ---------------
            ---------------               0
____________               ---------------
0           ---------------               |
------------                                |
|               WELCOME TO GORK               |
 |                              ---------------
  |             ---------------                0
   |____________                 ---------------
   0             ---------------
    ------------
";
                        Console.Select(Console.Text.Length, 0);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        GameState= GameState.PromptName;
                        continue;
                    case GameState.PromptName:
                        Console.AppendText(@"
What is your name hero? ");
                        break;
                    case GameState.Intro:
                        Console.Clear();
                        Console.AppendText(@"
OK let's PLAY!

Here are the rules:
#1 Don't cheat, unless winning requires it.
#2 Never forget rule #1.

For a list of commands, type 'help'.
Press Enter to continue.
"
                        );
                        break;
                    case GameState.Play:
                        Console.Clear();
                        Console.AppendText(@"
Enter command: "
                        );
                        break;
                }
                string input = await ReadLineAsync();
                if(string.IsNullOrWhiteSpace(input))
                {
                    if(GameState.Equals(GameState.Intro))
                    {
                        GameState = GameState.Play;
                    }
                }
                else
                {
                    switch (GameState)
                    {
                        case GameState.PromptName:
                            Console.AppendText($"Welcome {input} to this adventure!" + Environment.NewLine);
                            for (int i = 0; i < 50; i++)
                            {
                                Console.AppendText(">");
                                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                            }
                            GameState= GameState.Intro;
                            break;
                        case GameState.Intro:
                            GameState= GameState.Play;
                            break;
                        case GameState.Play:
                            Console.AppendText($"You entered: {input}" + Environment.NewLine);
                            await Task.Delay(TimeSpan.FromSeconds(1.5));
                            break;
                    }
                }
            }
        }

        SemaphoreSlim awaiter = new SemaphoreSlim(1, 1);
        private async Task<string> ReadLineAsync()
        {
            int charIndex = Console.GetFirstCharIndexOfCurrentLine();
            int line = Console.GetLineFromCharIndex(charIndex);
            string textB4 = Console.Lines[line];
            // Instruct the semaphore to block until further notice.
            awaiter.Wait(0);
            // Return from this method immediately.
            await awaiter.WaitAsync();
            // Resume here when [Enter] key unblocks the semaphore.
            string input = 
                string.IsNullOrWhiteSpace(textB4) ?
                Console.Lines[line] :
                Console.Lines[line].Replace(textB4, string.Empty);
            return input;
        }

        private void onConsoleKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                // Call Wait(0) so there's something to release
                // in case the awaiter isn't currently awaiting!
                try { awaiter.Wait(0); }
                finally{ awaiter.Release(); }
            }
        }
    }
}