using UglyToad.PdfPig;
using System.Text;

namespace Citire_rapida;

public partial class MainPage : ContentPage
{
    private int _lastIndex = -1;

    public MainPage()
    {
        InitializeComponent();
        SetDiceFace(5); // Fața inițială a zarului
    }

    // --- LOGICA ZARULUI SI TEXTELOR ---
    private async void OnRandomTextClicked(object sender, EventArgs e)
    {
        var random = new Random();

        // 1. ANIMAȚIA
        var rotationTask = DiceView.RotateTo(720, 800, Easing.CubicOut);

        for (int i = 0; i < 6; i++)
        {
            SetDiceFace(random.Next(1, 7));
            await Task.Delay(100);
        }

        await rotationTask;
        DiceView.Rotation = 0;

        // 2. TEXTE ÎN ENGLEZĂ (English Texts)
        string[] sampleTexts = {
            // Text 1: The Flow State
            "The state of flow, also known as 'the zone', is a mental state in which a person performing an activity is fully immersed in a feeling of energized focus, full involvement, and enjoyment in the process of the activity. In essence, flow is characterized by the complete absorption in what one does, and a resulting transformation in one's sense of time. To achieve this state, the challenge of the task must match the person's skill level; if the task is too easy, boredom sets in, but if it is too hard, anxiety takes over. Speed reading is often a gateway to flow because it demands total concentration.",

            // Text 2: Black Holes
            "Black holes are among the most mysterious and fascinating objects in the known universe. They form when massive stars collapse under their own gravity at the end of their life cycles. The gravitational pull of a black hole is so intense that nothing, not even light, can escape once it crosses the boundary known as the event horizon. Because no light can get out, people can't see black holes directly. They are invisible. Space telescopes with special tools can help find black holes by observing the behavior of material and stars that are very close to them.",

            // Text 3: The History of Writing
            "From the ancient clay tablets of Sumer to the high-resolution digital screens of today, the history of writing is the history of human civilization itself. The invention of the movable type printing press by Johannes Gutenberg in the 15th century democratized knowledge, allowing for the rapid spread of ideas that led to the Renaissance and the Scientific Revolution. Today, we live in an era of information overload, where the ability to filter and absorb written information quickly has become an essential superpower. Speed reading is not just about velocity; it is about adapting the brain to this modern data stream.",

            // Text 4: Neuroplasticity
            "The human brain is not a fixed structure but an incredibly adaptable organ capable of reorganizing itself throughout life. This phenomenon is called neuroplasticity. When you learn a new skill, such as speed reading or a new language, the brain creates new neural connections and strengthens existing ones. Through consistent practice, you can train the visual cortex to recognize entire groups of words instantly, reducing the dependency on subvocalization. Thus, reading becomes a direct visual process, much faster and more efficient than the internal auditory process we were taught in school.",
            
            // Text 5: Artificial Intelligence
            "Generative Artificial Intelligence has fundamentally changed how we interact with technology. Unlike traditional systems that simply classify data, generative models can create new content, whether it be text, images, or code. These large language models (LLMs) are trained on vast datasets, learning not just grammar but also the nuances of human logic and creativity. While this technology promises to revolutionize fields like medicine, education, and engineering, it also raises profound ethical questions about the future of work and the nature of consciousness."
        };

        // 3. GENERARE RANDOM FĂRĂ REPETIȚIE
        int newIndex;
        if (sampleTexts.Length > 1)
        {
            do { newIndex = random.Next(sampleTexts.Length); } while (newIndex == _lastIndex);
        }
        else { newIndex = 0; }

        _lastIndex = newIndex;

        // 4. AFISARE SI RESETARE
        InputText.Text = sampleTexts[newIndex];

        // Resetăm cursorul la început pentru a forța afișarea corectă
        InputText.CursorPosition = 0;
        InputText.SelectionLength = 0;

        SetDiceFace(random.Next(1, 7));
    }

    // --- LOGICA VIZUALĂ A ZARULUI ---
    private void SetDiceFace(int number)
    {
        DotTL.IsVisible = DotTR.IsVisible = DotML.IsVisible = DotC.IsVisible =
        DotMR.IsVisible = DotBL.IsVisible = DotBR.IsVisible = false;

        switch (number)
        {
            case 1: DotC.IsVisible = true; break;
            case 2: DotTL.IsVisible = DotBR.IsVisible = true; break;
            case 3: DotTL.IsVisible = DotC.IsVisible = DotBR.IsVisible = true; break;
            case 4: DotTL.IsVisible = DotTR.IsVisible = DotBL.IsVisible = DotBR.IsVisible = true; break;
            case 5: DotTL.IsVisible = DotTR.IsVisible = DotC.IsVisible = DotBL.IsVisible = DotBR.IsVisible = true; break;
            case 6: DotTL.IsVisible = DotTR.IsVisible = DotML.IsVisible = DotMR.IsVisible = DotBL.IsVisible = DotBR.IsVisible = true; break;
        }
    }

    // --- NAVIGARE ---
    private async void OnRead(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InputText.Text)) return;
        string encodedText = Uri.EscapeDataString(InputText.Text);
        await Shell.Current.GoToAsync($"{nameof(ReadPage)}?text_de_transmis={encodedText}");
    }

    // --- UPLOAD PDF ---
    private async void OnPickPdfClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select PDF",
                FileTypes = FilePickerFileType.Pdf
            });

            if (result != null)
            {
                LoadingSpinner.IsVisible = LoadingSpinner.IsRunning = true;
                InputText.Text = await ExtractTextFromPdf(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not read PDF: " + ex.Message, "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = LoadingSpinner.IsRunning = false;
        }
    }

    private async Task<string> ExtractTextFromPdf(string path)
    {
        return await Task.Run(() => {
            StringBuilder text = new StringBuilder();
            using (var pdf = PdfDocument.Open(path))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.Append(page.Text);
                    text.Append(" ");
                }
            }
            return text.ToString();
        });
    }
}