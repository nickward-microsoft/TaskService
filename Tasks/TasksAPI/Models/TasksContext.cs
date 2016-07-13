using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TasksAPI.Models
{
    public class TasksContext : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
    }
}