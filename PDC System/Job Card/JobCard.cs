using System;

namespace PDC_System
{
    public class JobCard
    {
        public DateTime JobCardDate { get; set; } = DateTime.Now;
        public string? Customer_Name { get; set; }
        public string? Description { get; set; }
        public string? Paper_Size { get; set; }
        public int GSM { get; set; }
        public string? Paper_Type { get; set; }
        public int Quantity { get; set; }
        public int Printed { get; set; }
        public string? Duplex { get; set; }
        public string? Laminate { get; set; }
        public string? Special_Note { get; set; }

        public bool? IsSeen { get; set; }


    }


}

