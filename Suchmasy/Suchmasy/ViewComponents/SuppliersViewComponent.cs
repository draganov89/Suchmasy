﻿using Microsoft.AspNetCore.Mvc;
using Suchmasy.Data;

namespace Suchmasy.ViewComponents
{
    public class SuppliersViewComponent : ViewComponent
    {
        public SuppliersViewComponent(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private ApplicationDbContext _dbContext { get; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var users = _dbContext.Suppliers.ToList();
            return View(users);
        }

    }
}
