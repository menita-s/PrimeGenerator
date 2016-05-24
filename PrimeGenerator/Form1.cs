using System;
using System.Threading;
using System.Windows.Forms;

namespace PrimeGenerator
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cts;

        public Form1()
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

            StartCountdownDelegate countdown = new StartCountdownDelegate(StartCountdown);
            countdown.BeginInvoke(60, new AsyncCallback(TimesUpCallback), null);

            GeneratePrimeDelegate calc = new GeneratePrimeDelegate(GeneratePrime);
            calc.BeginInvoke(1000000000, token, null, null);
        }

        private delegate void StartCountdownDelegate(int seconds);
        private void StartCountdown(int seconds)
        {
            for (var x = 1; x <= seconds; x++)
            {
                ShowCountdownDelegate showCountdown = new ShowCountdownDelegate(ShowCountDown);
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
            if (txtValue.InvokeRequired == false)
            {
                //rtxtPrimeNumbers.Text += "  " + number;
                txtValue.Text = number.ToString();
            }
            else
            {
                ShowIfPrimeNumberDelegate showPrime = new ShowIfPrimeNumberDelegate(DisplayPrimeNumber);
                BeginInvoke(showPrime, new object[] { number });
            }
        }

        private delegate void GeneratePrimeDelegate(int maxNum, CancellationToken token);
        void GeneratePrime(int maxNum, CancellationToken token)
        {
            DisplayPrimeNumber(2);
            for (int i = 3; i < maxNum; i++)
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
            for (int k = 3; k <= Math.Ceiling(Math.Sqrt(number)); k++)
            {
                if (number > k && number % k == 0)
                    break;
                if (k >= Math.Ceiling(Math.Sqrt(number)) || number == k)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
