using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FileSender
{
    public class JobScheduler
    {
        private static IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

        public static async Task Start(string startPath, string email, int intervalInHours)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            if (await scheduler.IsJobGroupPaused((await scheduler.GetJobGroupNames()).FirstOrDefault()))
                await scheduler.ResumeAll();

            if (jobKeys.Count == 0)
            {
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<ScheduleJob>().Build();

                ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .UsingJobData("startPath", startPath)
                .UsingJobData("zipPath", Path.GetTempPath() + "/result.zip")
                .UsingJobData("email", email)
                .UsingJobData("interval", intervalInHours)
                .StartNow()
                .WithSimpleSchedule(x => x
                .WithIntervalInHours(intervalInHours)
                .RepeatForever())
                .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }

        public static async Task Stop()
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            await scheduler.DeleteJobs(jobKeys);
        }

        public class ScheduleJob : IJob
        {
            async Task IJob.Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.MergedJobDataMap;
                var startPath = dataMap.GetString("startPath");
                var zipPath = dataMap.GetString("zipPath");
                var email = dataMap.GetString("email");
                await SendMessageWithAttachment(startPath, zipPath, email);
            }

            async Task SendMessageWithAttachment(string startPath, string zipPath, string email)
            {
                var cameraFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "/Camera/";

                if (!Directory.Exists(cameraFolder))
                    Directory.CreateDirectory(cameraFolder);

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, true);

                DirectoryInfo dr = new DirectoryInfo(startPath);
                foreach (FileInfo fi in dr.GetFiles())
                    fi.MoveTo(cameraFolder + fi.Name);

                using (MailMessage mail = new MailMessage())
                using (SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com"))
                using (Attachment attachment = new Attachment(zipPath))
                {
                    mail.From = new MailAddress("autosendservice2018@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = DateTime.Now.ToString();
                    mail.Body = "Mail with attachment";

                    mail.Attachments.Add(attachment);

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("autosendservice2018@gmail.com", "inchvorparol");
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                }
                File.Delete(zipPath);
            }
        }
    }
}
