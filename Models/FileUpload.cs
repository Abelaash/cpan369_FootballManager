using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    [Table("Filetbl")]
    public class FileUpload
	{
		[Key]
		public int FileId { get; set; }

		[Required]
		[StringLength(255)]
		public string FileName { get; set; }

		[Required]
		[StringLength(500)]
		public string FilePath { get; set; }

		[Required]
		public DateTime UploadDate { get; set; }
	}
}