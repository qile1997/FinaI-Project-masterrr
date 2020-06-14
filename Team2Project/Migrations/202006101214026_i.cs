namespace Team2Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class i : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attendances",
                c => new
                    {
                        AttendanceID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Programme = c.Int(),
                        CourseTimetableId = c.Int(nullable: false),
                        AttendanceTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AttendanceID);
            
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        CoursesID = c.Int(nullable: false, identity: true),
                        Programme = c.Int(nullable: false),
                        CourseName = c.String(),
                    })
                .PrimaryKey(t => t.CoursesID);
            
            CreateTable(
                "dbo.CourseTimetables",
                c => new
                    {
                        CourseTimetableId = c.Int(nullable: false, identity: true),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        CoursesID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CourseTimetableId)
                .ForeignKey("dbo.Courses", t => t.CoursesID, cascadeDelete: true)
                .Index(t => t.CoursesID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UsersID = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        Programme = c.Int(),
                        Roles = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UsersID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CourseTimetables", "CoursesID", "dbo.Courses");
            DropIndex("dbo.CourseTimetables", new[] { "CoursesID" });
            DropTable("dbo.Users");
            DropTable("dbo.CourseTimetables");
            DropTable("dbo.Courses");
            DropTable("dbo.Attendances");
        }
    }
}
