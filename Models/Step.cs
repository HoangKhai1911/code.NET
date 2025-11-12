using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WinCook.Models

{
    /// <summary>
    /// Model cho bảng Steps
    /// </summary>
    public class Step
    {
        public int StepId { get; set; }
        public int RecipeId { get; set; }
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
    }
}
