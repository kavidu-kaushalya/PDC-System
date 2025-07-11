using System;

namespace PDC_System.Orders
{
    public class Order
    {
        public DateTime CreateDate { get; set; }

        public DateTime DueDate { get; set; }
        public string CustomerName { get; set; }
        public string Description { get; set; }
        public bool IsFinished { get; set; }
        public string Notes { get; set; }

        public string Countdown
        {
            get
            {
                if (IsFinished)
                    return "Finished";

                TimeSpan timeRemaining = DueDate - DateTime.Now;
                return $"{timeRemaining.Days} DAYS {timeRemaining.Hours} HOURS {timeRemaining.Minutes} MIN";
            }
        }
    }
}