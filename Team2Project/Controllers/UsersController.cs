using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Team2Project.Models;
using Team2Project.ViewModel;

namespace Team2Project.Controllers
{
    public class UsersController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(Users user)
        {
            var users = CheckUser(user);
            if (users != null)
            {
                if (users.Roles == Roles.Student)
                {
                    Session["Student"] = users.UsersID;
                    return RedirectToAction("HomePage");
                }
                else if (users.Roles == Roles.Trainer)
                {
                    Session["Admin"] = users.UsersID;
                    return RedirectToAction("AttendanceList", "Admin");
                }
            }
            else
            {
                ViewBag.Error = "Wrong Username and Passowrd ";
            }
            return View();
        }

        [NonAction]
        public Users CheckUser(Users user)
        {
            var GetUser = db.Users.SingleOrDefault(a => a.Username == user.Username && a.Password == user.Password);
            return GetUser;
        }

        public ActionResult GetAttendance()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAttendance(Users users)
        {
            return View();
        }

        //public ActionResult Profile()
        //{
        //    var id = Convert.ToInt32(Session["Student"]);

        //    var user = db.Users.SingleOrDefault(u => u.UsersID == id);
        //    if (user != null)
        //    {
        //        return View(user);
        //    }
        //    return View();
        //}

        public ActionResult Logout()
        {
            return View();
        }

        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult CheckIn()
        {
            var id = Convert.ToInt32(Session["Student"]);

            var user = db.Users.SingleOrDefault(u => u.UsersID == id);
            if (user != null)
            {
                Session["StudentProgramme"] = user.Programme;
                Session["SelectedProgramme"] = user.Programme.ToString();
                var courses = db.Courses.Where(c => c.Programme == user.Programme).ToList();
                ViewBag.Courses = new SelectList(courses, "CoursesID", "CourseName");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CheckIn(string CourseName)
        {
            List<TimetableVM> getTimetable = new List<TimetableVM>();
            var getCourse = db.Courses.SingleOrDefault(c => c.CourseName == CourseName);
            var getTime = db.CourseTimetables.Where(c => c.CoursesID == getCourse.CoursesID);
            foreach (var item in getTime)
            {
                if (getTime == null)
                {
                    string Error = "No Data";
                    return Json(new { Error, time = false }, JsonRequestBehavior.AllowGet);
                }
                TimetableVM AddTimetable = new TimetableVM
                {
                    CoursesID = item.CoursesID,
                    CourseTimetableId = item.CourseTimetableId,
                    CourseName = getCourse.CourseName,
                    Programme = getCourse.Programme,
                    StartTime = item.StartTime.ToString(),
                    EndTime = item.EndTime.ToString()
                };
                getTimetable.Add(AddTimetable);
            }

            return Json(new { getTimetable, time = true }, JsonRequestBehavior.AllowGet);
            //Session["SelectedCourse"] = selected.CourseName;
            //return RedirectToAction("CourseDateIndex");
        }

        // Ajax
        [HttpPost]
        public ActionResult TakeAttendance(int CourseTimeID)
        {
            
            var getTime = db.CourseTimetables.SingleOrDefault(c => c.CourseTimetableId == CourseTimeID);
            var studentID = Convert.ToInt32(Session["Student"]);
            var student = db.Users.SingleOrDefault(c => c.UsersID == studentID);
            Attendance AddAttendance = new Attendance
            {
                Name = student.Username,
                Programme = student.Programme,
                CourseTimetableId = getTime.CourseTimetableId,
                AttendanceTime = DateTime.Now
            };
            db.Attendance.Add(AddAttendance);
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }


























        //// GET: Users
        //public ActionResult Index()
        //{
        //    return View(db.Users.ToList());
        //}

        //// GET: Users/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Users users = db.Users.Find(id);
        //    if (users == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(users);
        //}

        //// GET: Users/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Users/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "UsersID,Username,Password,Email,Programme,Roles")] Users users)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Users.Add(users);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(users);
        //}

        //// GET: Users/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Users users = db.Users.Find(id);
        //    if (users == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(users);
        //}

        //// POST: Users/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "UsersID,Username,Password,Email,Programme,Roles")] Users users)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(users).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(users);
        //}

        //// GET: Users/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Users users = db.Users.Find(id);
        //    if (users == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(users);
        //}

        //// POST: Users/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Users users = db.Users.Find(id);
        //    db.Users.Remove(users);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
