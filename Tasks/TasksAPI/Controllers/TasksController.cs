using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using TaskService.Models;

namespace TaskService.Controllers
{
    public class TaskController : ApiController
    {
        private TaskContext db = new TaskContext();
        private string hectagonapikey = WebConfigurationManager.AppSettings["hectagonapikey"];

        // GET: api/Tasks
        public IQueryable<Task> GetTasks()
        {
            return db.Tasks;
        }

        // GET: api/Tasks/5
        [ResponseType(typeof(Task))]
        public IHttpActionResult GetTask(int id)
        {
            Task task = db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        // PUT: api/Tasks/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTask(int id, Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != task.TaskId)
            {
                return BadRequest();
            }

            db.Entry(task).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Tasks
        [ResponseType(typeof(Task))]
        public IHttpActionResult PostTask(Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Tasks.Add(task);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = task.TaskId }, task);
        }

        // DELETE: api/Tasks/5
        [ResponseType(typeof(Task))]
        public IHttpActionResult DeleteTask(int id)
        {
            Task task = db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            db.Tasks.Remove(task);
            db.SaveChanges();

            return Ok(task);
        }

        // PATCH: api/Tasks/5
        // this operation marks a task as completed
        [HttpPatch]
        [ResponseType(typeof(bool))]
        public async System.Threading.Tasks.Task<IHttpActionResult> PatchTaskCompleted(int id)
        {
            Task task = db.Tasks.Find(id);
            if(task == null)
            {
                return NotFound();
            } else if(task.Complete == true)
            {
                return Ok(true);
            } else
            {
                // set task complete flag to true
                task.Complete = true;
                db.Entry(task).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    await SendTaskCompleteNotificationAsync(task);
                    return Ok(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task SendTaskCompleteNotificationAsync(Task task)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://hectagonapi.azure-api.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Trace", "true");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", hectagonapikey);

                var apiUriString = String.Concat("notifications/SendTaskCompleteNotification/", task.TaskId);
                HttpResponseMessage response = await client.PostAsync(apiUriString, null);
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    //all good
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TaskExists(int id)
        {
            return db.Tasks.Count(e => e.TaskId == id) > 0;
        }
    }
}