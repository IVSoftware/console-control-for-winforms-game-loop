As I understand it, you want something that looks and behaves like a Console, but in a WinForms app, and this is a stylistic choice (because otherwise there are better ways to prompt user for input in WinForms!). Running a "game loop" similar to your console app is possible but requires special care. We say that WinForms is "event-driven" because the application has a message loop to listens for events like mouse clicks and key presses. For example, this loop is going to get stuck waiting for keypresses because it's "blocking the UI thread" that _detects_ those keypresses. 

    void BadGameLoop()
    {
        while(run)  // Don't do this!
        { 
            string input = ReadLine();
            switch(input)
            {
                // Do something
            }
        }
    }

On the other hand, the `await` keyword will cause the method to return immediately, but then resume at this spot when "something happens":

    async Task GoodGameLoop()
    {
        while(run)  // Don't do this!
        { 
            string input = await ReadLineAsync();
            switch(input)
            {
                // Do something
            }
        }
    }

***
**Read a command asynchronously**

Block when the `ReadLineAsync()` is called.

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

Unblock the semaphore when the [Enter] key is pressed. 

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

***
**Game Init**

Here's the code I used to test this answer:

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
            .
            .
            .
            // ReadLineAsync method ...
            // onConsoleKeyDown method ...
        }
    }