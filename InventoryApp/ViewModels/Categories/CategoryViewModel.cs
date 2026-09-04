using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.ViewModels
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public int ProductsCount { get; set; }
    }
}