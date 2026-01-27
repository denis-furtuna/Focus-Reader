using System.Threading.Tasks;

namespace Citire_rapida;

[QueryProperty(nameof(TextPrimit), "text_de_transmis")]
public partial class ReadPage : ContentPage
{
    string[] words = Array.Empty<string>();
    int currentIndex = 0;
    int delayMilliseconds = 1200;

    public string TextPrimit
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Includem și semnele de punctuație în cuvinte
                words = value.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }

    public ReadPage()
    {
        InitializeComponent();
    }

    // REGULA DE CALCUL PENTRU LITERA COLORATĂ
    private int GetOrpIndex(int length)
    {
        if (length <= 1) return 0;
        if (length >= 2 && length <= 5) return 1;   // a 2-a litera
        if (length >= 6 && length <= 9) return 2;   // a 3-a litera
        if (length >= 10 && length <= 13) return 3; // a 4-a litera
        return 4; // a 5-a litera pentru 14, 15+
    }

    private void DisplayWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return;

        // Calculăm punctul de focalizare conform regulilor tale
        int orpIndex = GetOrpIndex(word.Length);

        // Siguranță: să nu depășim lungimea cuvântului
        if (orpIndex >= word.Length) orpIndex = word.Length - 1;

        // Prefixul (Alb)
        Part1Label.Text = word.Substring(0, orpIndex);

        // Litera de focus (Roșu)
        OrpLabel.Text = word[orpIndex].ToString();

        // Sufixul (Alb)
        Part2Label.Text = word.Substring(orpIndex + 1);
    }

    bool Start = false;
    bool Restart = false;
    bool Stop = false;
    bool Back = false;
    bool isPlaying = false;

    private async Task StartReading()
    {
        while (currentIndex < words.Length)
        {
           
            // Verificare flag-uri de control
            if (Stop || Restart || Back)
            {
                Start = false;
                Stop = false;

                // Așteptăm până se apasă Start din nou
                while (!Start)
                {
                    if (Restart)   Restart = false; 
                    if (Back)  Back = false; 
                    if (words.Length > 0) DisplayWord(words[currentIndex]);
                    await Task.Delay(100);
                }
            }

            if (currentIndex < words.Length)
            {
                DisplayWord(words[currentIndex]);
                currentIndex++;
            }
            ReadingProgress.Progress = (double)currentIndex / words.Length;
            if (words[currentIndex-1][^1] == ',') await Task.Delay((int)(delayMilliseconds * 2));
            else if (words[currentIndex-1][^1] =='.' ) await Task.Delay(delayMilliseconds*3);
            else await Task.Delay(delayMilliseconds);
        }
        isPlaying = false;
        Start = false;
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        double newStep = Math.Round(e.NewValue / 10.0) * 10.0;
        if (e.NewValue != newStep) ((Slider)sender).Value = newStep;

        ValueLabel.Text = $"Words per minute: {newStep}";
        delayMilliseconds = (int)(60000 / newStep);
    }
    private void ActualizeazaCuloareButon()
    {
        // 1. Culori pentru butonul principal START
        if (currentIndex >= words.Length)
        {
            StartButton.BackgroundColor = Colors.Gray;
            StartButton.Text = "FINISHED";
            
        }
        else
        {
            StartButton.BackgroundColor = Start ? Colors.Red : Colors.Lime;
            StartButton.Text = Start ? "READING..." : "START";
        }

        // 2. Culori pentru butoanele de control (Stop, Restart, Back)
        // Alegem o culoare care să sară în ochi când Start e true
        Color culoareActive = Colors.DarkOrange;
        Color culoareInactive = Colors.DimGray; // Gri închis/estompat
        StopButton.BackgroundColor = Start ? culoareActive:culoareInactive;
        // Activează menținerea ecranului aprins când pornești lectura
        DeviceDisplay.Current.KeepScreenOn = Start?true:false;
        if (Start)
        {
            RestartButton.BackgroundColor = culoareActive;
            BackButton.BackgroundColor = culoareActive;
        }
        else if (currentIndex==0)
        {
          
            RestartButton.BackgroundColor = culoareInactive;
            BackButton.BackgroundColor = culoareInactive;
        }
        

         
    }
    private async void OnStartButtonClicked(object sender, EventArgs e)
    {
        Start = true;
        ActualizeazaCuloareButon(); // <-- Aici devine ROȘU
        if (!isPlaying)
        {
            Restart = false;
            Stop = false;
            Back = false;
            isPlaying = true;
            

            await StartReading();

            ActualizeazaCuloareButon(); // <-- Aici revine la VERDE
        }
    }

    private void OnStopButtonClicked(object sender, EventArgs e)
    {
        Stop = true;
        Start = false;
        ActualizeazaCuloareButon(); // <-- Aici devine VERDE
    }

    private void OnRestartButtonClicked(object sender, EventArgs e)
    {
        Restart = true;
        currentIndex = 0;
        DisplayWord(words[currentIndex]);
        ReadingProgress.Progress = (double)currentIndex / words.Length;
        Start = false;
        ActualizeazaCuloareButon(); // <-- Aici devine VERDE
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {   
        Back = true;
        currentIndex = Math.Max(0, currentIndex - 10);
        DisplayWord(words[currentIndex]);
        ReadingProgress.Progress = (double)currentIndex / words.Length;
        Start = false;
        ActualizeazaCuloareButon(); // <-- Aici devine VERDE
    }
}