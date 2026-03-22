using AnotadorGymApp.Data.Models.DTOs.Rutina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Data.Models.Results
{
    public class RutinaListResult
    {
        public List<RutinaDto> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
