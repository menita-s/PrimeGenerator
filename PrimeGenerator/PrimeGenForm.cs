using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PrimeGenerator
{
    public partial class PrimeGenForm : Form
    {
        private CancellationTokenSource cts;
        private List<long> primes;

        public PrimeGenForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            txtValue.Text = "";
            lblTimer.Text = "";
            btnStart.Enabled = false;
            primes = new List<long>();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            //create 2 delegates to spin off the clock and generate prime numbers in 
            //separate threads

            //instantiate a delegate to maintain a simple countdown clock
            StartCountdownDelegate countdown = StartCountdown;
            //invoke the clock countdown asyncronously
            //fire a callBack to stop generation of the prime number
            countdown.BeginInvoke(60, TimesUpCallback, null);

            //instantiate a delegate to generate prime upto 1 billion
            GeneratePrimeDelegate calc = GeneratePrime;
            //invoke the method asyncronously
            calc.BeginInvoke(100000000, token, null, null);
        }

        private delegate void StartCountdownDelegate(int seconds);
        private void StartCountdown(int seconds)
        {
            //since UI is on a different thread, create an instance of the delegate to display seconds count down
            ShowCountdownDelegate showCountdown = ShowCountDown;
            for (var x = 1; x <= seconds; x++)
            {
                BeginInvoke(showCountdown, new object[] { x });
                Thread.Sleep(1000); // sleep every second
            }
            //enable the start button after 1 minute is up
            btnStart.Invoke(new EnableButtonDelegate(EnableButton));
        }

        void TimesUpCallback(IAsyncResult result)
        {
            //call a Cancel() on the cancellation token to call a halt to the prime number generator method
            cts.Cancel();
        }

        //create delegate for UI handling
        private delegate void EnableButtonDelegate();
        private void EnableButton()
        {
            btnStart.Enabled = true;
        }

        //create delegate for UI handling
        delegate void ShowCountdownDelegate(int seconds);
        private void ShowCountDown(int seconds)
        {
            lblTimer.Text = $"Counting down in seconnds: {seconds}";
        }

        //create delegate for UI handling
        delegate void ShowIfPrimeNumberDelegate(int number);
        private void DisplayPrimeNumber(int number)
        {
            ShowIfPrimeNumberDelegate showPrime = DisplayPrimeNumber;
            if (txtValue.InvokeRequired == false)
            {
                txtValue.Text = number.ToString();
            }
            else
            {
                BeginInvoke(showPrime, new object[] { number });
            }
        }

        //create delegate to enable async calling of prime number generation
        private delegate void GeneratePrimeDelegate(int maxNum, CancellationToken token);
        
        // a simple yet somewhat optimized algorithm for prime number generation.
        void GeneratePrime(int maxNum, CancellationToken token)
        {
            Console.WriteLine($"Time started: {DateTime.UtcNow}");
            primes.Add(2);// initiate the list with first prime number
            var nextPotentialPrime = 3;
            try
            {
                while (primes[primes.Count - 1] < maxNum)
                {
                    if (token.IsCancellationRequested) // break the loop if a cancel() is called
                        break;

                    int sqrt = (int) Math.Sqrt(nextPotentialPrime);
                    bool isPrime = true;
                    for (int i = 0; (int) primes[i] <= sqrt; i++)
                    {
                        if (nextPotentialPrime%primes[i] == 0)
                        {
                            isPrime = false;
                            break;
                        }
                    }
                    if (isPrime)
                    {
                        DisplayPrimeNumber(nextPotentialPrime);
                        primes.Add(nextPotentialPrime);
                    }
                    nextPotentialPrime += 2; //look only for odd numbers
                }
            }
            catch (Exception)
            {
                //intentionally swallow the exception
            }
            finally
            {
                Console.WriteLine($"Maximum prime number generated is: {primes[primes.Count - 1]}");
                Console.WriteLine($"Time ended: {DateTime.UtcNow}");

            }
            
            
        }
       
    }
}
