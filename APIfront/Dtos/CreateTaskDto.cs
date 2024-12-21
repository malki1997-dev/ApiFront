﻿using System.ComponentModel.DataAnnotations;

namespace APIfront.Dtos
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Le titre ne peut pas dépasser 100 caractères.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères.")]
        public string Description { get; set; }
    }
}
