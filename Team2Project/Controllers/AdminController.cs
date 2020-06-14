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
    public class AdminController : Controller
    {
        private AppDbContext db = new AppDbContext();


        //Create View For Attendance 
        public ActionResult AttendanceList()
        {
            return View(db.Attendance.ToList());
        }

        public ActionResult Profile()
        {
            var AID = Convert.ToInt32(Session["Admin"]);

            var user = db.Users.SingleOrDefault(c => c.UsersID == AID);

            if (user != null)
            {
                return View(user);
            }

            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register([Bind(Include = "ID , Username , Password ,Programme,Email, Roles")] Users user)
        {
            if (ModelState.IsValid)
            {
                user.Roles = Roles.Student;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("AttendanceList");
            }
            return View();
        }

        public ActionResult AjaxCheckEmail(string Email)
        {
            // check user after you type the email
            var CheckDuplicate = db.Users.Where(c => c.Email == Email).SingleOrDefault();

            if (CheckDuplicate != null)
            {
                var emailDuplicate = "Email is not available. Please try another email!";
                return Json(new { Error = true, emailDuplicate }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var emailNoDuplicate = "Email is available.";
                return Json(new { Error = false, emailNoDuplicate }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SelectProgramme()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SelectProgramme(string selectedProgramme)
        {
            List<TimetableVM> AllCourseName = new List<TimetableVM>();
            var Getdata = db.Courses.ToList().Where(c => c.Programme == (Programme)Enum.Parse(typeof(Programme), selectedProgramme));
            foreach (var item in Getdata)
            {
                var check = AllCourseName.SingleOrDefault(a => a.CourseName == item.CourseName);
                if (check == null)
                {
                    TimetableVM AddCourseName = new TimetableVM
                    {
                        CoursesID = item.CoursesID,
                        CourseName = item.CourseName,
                    };
                    AllCourseName.Add(AddCourseName);
                }
            }
            return Json(new { AllCourseName, data = true }, JsonRequestBehavior.AllowGet);

            //Session["SelectedProgramme"] = selected.Programmes.ToString();
            //return RedirectToAction("CourseIndex");
        }

        //Will add one ajax action for select course

        //Will Change to Ajax to show all time for the course
        [HttpPost]
        public ActionResult CourseIndex(int CourseId, string selectedProgramme)
        {
            List<TimetableVM> getTimetable = new List<TimetableVM>();
            /* string programme = Session["SelectedProgramme"].ToString()*/
            //var courseTimetable = db.Courses.ToList().Where(c => c.Programme == (Programme)Enum.Parse(typeof(Programme), selectedProgramme) && c.CourseName == selectedCourse);
            var programme = (Programme)Enum.Parse(typeof(Programme), selectedProgramme);
            var getCourse = db.Courses.SingleOrDefault(c=>c.CoursesID == CourseId && c.Programme == programme);
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

        }


        [HttpPost]
        public ActionResult CreateCourse(string CreateOption, string selectedProgramme, int SelectedCourseId, string CourseName, string StartTime, string EndTime)
        {
            var programme = (Programme)Enum.Parse(typeof(Programme), selectedProgramme);
            var CheckCourseName = db.Courses.SingleOrDefault(c => c.CourseName == CourseName && c.Programme == programme);
            if (CreateOption == "Course")
            {
                if (CheckCourseName != null)
                {
                    string Error = "This Course Name Already Exist";
                    return Json(new { Error, SaveCourse = false }, JsonRequestBehavior.AllowGet);
                }
                Courses getCourseName = new Courses
                {
                    Programme = programme,
                    CourseName = CourseName
                };
                db.Courses.Add(getCourseName);
                db.SaveChanges();
                return Json(new { SaveCourse = true }, JsonRequestBehavior.AllowGet);
            }
            else
            //if (CreateOption == "Time")
            {
                DateTime getStartTime = Convert.ToDateTime(StartTime);
                DateTime getEndTime = Convert.ToDateTime(EndTime);
                var getCourseName = db.Courses.SingleOrDefault(c => c.CoursesID == SelectedCourseId && c.Programme == programme);
                if (getStartTime >= getEndTime)
                {
                    string Error = "Start Time must small than End Time";
                    return Json(new { Error, Savetime = false }, JsonRequestBehavior.AllowGet);
                }
                var CheckTime = db.CourseTimetables.SingleOrDefault(ct => ct.CoursesID == getCourseName.CoursesID && getCourseName.Programme == programme && ct.StartTime == getStartTime);
                if (CheckTime != null)
                {
                    string Error = "You Already Have A Time For This Time";
                    return Json(new { Error, Savetime = false }, JsonRequestBehavior.AllowGet);
                }
                CourseTimetable timetable = new CourseTimetable
                {
                    CoursesID = getCourseName.CoursesID,
                    StartTime = getStartTime,
                    EndTime = getEndTime
                };
                db.CourseTimetables.Add(timetable);
                db.SaveChanges();
                return Json(new { Savetime = true }, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: Admin/Edit/5
        public ActionResult EditCourseTime(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseTimetable courseTimetable = db.CourseTimetables.SingleOrDefault(ct=>ct.CourseTimetableId == id);
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == courseTimetable.CoursesID);

            if (courseTimetable == null)
            {
                return HttpNotFound();
            }
            TimetableVM timetable = new TimetableVM
            {
                //CoursesID = courses.CoursesID,
                CourseName = courses.CourseName,
                //Programme = courses.Programme,
                StartTime = courseTimetable.StartTime.ToString(),
                EndTime = courseTimetable.EndTime.ToString(),

            };
            return View(timetable);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourseTime([Bind(Include = "CourseTimetableId,StartTime,EndTime")] CourseTimetable courseTimetable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courseTimetable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AttendanceList");
            }
            return View(courseTimetable);
        }

        // GET: Admin/Details/5
        public ActionResult CourseTimeDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseTimetable courseTimetable = db.CourseTimetables.SingleOrDefault(ct => ct.CourseTimetableId == id);
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == courseTimetable.CoursesID);
            if (courseTimetable == null)
            {
                return HttpNotFound();
            }
            TimetableVM timetable = new TimetableVM
            {
                //CoursesID = courses.CoursesID,
                CourseName = courses.CourseName,
                CourseTimetableId = courseTimetable.CourseTimetableId,
                //Programme = courses.Programme,
                StartTime = courseTimetable.StartTime.ToString(),
                EndTime = courseTimetable.EndTime.ToString(),

            };
            return View(timetable);
        }

        // GET: Admin/Delete/5
        public ActionResult DeleteCourseTime(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseTimetable courseTimetable = db.CourseTimetables.SingleOrDefault(ct => ct.CourseTimetableId == id);
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == courseTimetable.CoursesID);
            if (courseTimetable == null)
            {
                return HttpNotFound();
            }
            TimetableVM timetable = new TimetableVM
            {
                //CoursesID = courses.CoursesID,
                CourseName = courses.CourseName,
                CourseTimetableId = courseTimetable.CourseTimetableId,
                //Programme = courses.Programme,
                StartTime = courseTimetable.StartTime.ToString(),
                EndTime = courseTimetable.EndTime.ToString(),

            };
            return View(timetable);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("DeleteCourseTime")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CourseTimetable courseTimetable = db.CourseTimetables.SingleOrDefault(ct => ct.CourseTimetableId == id);
            db.CourseTimetables.Remove(courseTimetable);
            db.SaveChanges();
            return RedirectToAction("AttendanceList");
        }











        public ActionResult EditCourse(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == id);
            if (courses == null)
            {
                return HttpNotFound();
            }
            return View(courses);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse([Bind(Include = "CoursesID,CourseName")] Courses courses)
        {
            if (ModelState.IsValid)
            {
                db.Entry(courses).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AttendanceList");
            }
            return View(courses);
        }

        public ActionResult DeleteCourse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == id);
            if (courses == null)
            {
                return HttpNotFound();
            }
            return View(courses);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCourseConfirmed(int id)
        {
            var getId = db.Courses.SingleOrDefault(c => c.CoursesID == id);
            var checktime = db.CourseTimetables.Where(ct => ct.CoursesID == getId.CoursesID);
            if (checktime.Count() != 0)
            {
                db.CourseTimetables.RemoveRange(checktime);
            }
            Courses courses = db.Courses.SingleOrDefault(c => c.CoursesID == id);
            db.Courses.Remove(courses);
            db.SaveChanges();
            return RedirectToAction("AttendanceList");
        }

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
