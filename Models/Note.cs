using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCook.Models
{
    /// <summary>
    * Model cho bảng Notes(Ghi chú cá nhân)
    /// </summary>
    public class Note
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public string NoteText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
