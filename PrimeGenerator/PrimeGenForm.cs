using System;
using System.Threading;
using System.Windows.Forms;

namespace PrimeGenerator
{
    public partial class PrimeGenForm : Form
    {
        private CancellationTokenSource cts;

        public PrimeGenForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            txtValue.Text = "";
            lblTimer.Text = "";
            btnStart.Enabled = false;

            cts = new CancellationTokenSource();
            var token = cts.Token;

            StartCountdownDelegate countdown = StartCountdown;
            countdown.BeginInvoke(60, TimesUpCallback, null);

            GeneratePrimeDelegate calc = GeneratePrime;
            calc.BeginInvoke(1000000000, token, null, null);
        }

        private delegate void StartCountdownDelegate(int seconds);
        private void StartCountdown(int seconds)
        {
            ShowCountdownDelegate showCountdown = ShowCountDown;
            for (var x = 1; x <= seconds; x++)
            {
                BeginInvoke(showCountdown, new object[] { x });
                Thread.Sleep(1000);
            }
            btnStart.Invoke(new EnableButtonDelegate(EnableButton));
        }

        void TimesUpCallback(IAsyncResult result)
        {
            cts.Cancel();
        }

        private delegate void EnableButtonDelegate();
        private void EnableButton()
        {
            btnStart.Enabled = true;
        }

        delegate void ShowCountdownDelegate(int seconds);
        private void ShowCountDown(int seconds)
        {
            lblTimer.Text = $"Counting down in seconnds: {seconds}";
        }

        delegate void ShowIfPrimeNumberDelegate(int number);
        private void DisplayPrimeNumber(int number)
        {
            ShowIfPrimeNumberDelegate showPrime = DisplayPrimeNumber;
            if (txtValue.InvokeRequired == false)
            {
                txtValue.Text = number.ToString();
                //Console.WriteLine(number);
            }
            else
            {
                BeginInvoke(showPrime, new object[] { number });
            }
        }

        private delegate void GeneratePrimeDelegate(int maxNum, CancellationToken token);
        void GeneratePrime(int maxNum, CancellationToken token)
        {
            for (int i = 2; i < maxNum; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                if (IsPrimeNumber(i))
                {
                    DisplayPrimeNumber(i);
                }
            }
        }

        private bool IsPrimeNumber(long number)
        {
            for (int j = 2; j <= Math.Ceiling(Math.Sqrt(number)); ++j)
            {
                if (number != j && number % j == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
