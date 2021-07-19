using System;
using System.Collections.Generic;
using Application.DTOs.Account;
using Application.DTOs.Admin.Chart;

namespace Application.DTOs.Admin
{
    public class DashboardDTO
    {
        public DashboardDTO()
        {
            JoinProgress = new List<SeriesChartDTO>();
            Online = new List<string>();
        }
        public int TotalAccount { get; set; }
        public int TotalRoute { get; set; }
        public int TotalQuiz { get; set; }
        public int TotalExam { get; set; }
        public List<SeriesChartDTO> JoinProgress { get; set; }
        public List<string> Online { get; set; }
    }
}