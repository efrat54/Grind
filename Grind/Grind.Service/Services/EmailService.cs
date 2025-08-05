using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.Interfaces; // לשימוש בממשק IEmailService
using System.Net.Mail; // עבור מחלקות הקשורות לשליחת מיילים (MailMessage, SmtpClient)
using System.Net; // עבור מחלקות הקשורות לניהול רשת (NetworkCredential)
using Microsoft.Extensions.Configuration; // לגישה להגדרות אפליקציה (כמו מתוך appsettings.json)

namespace Grind.Service.Services
{
    // הגדרת המחלקה EmailService שמממשת את ממשק IEmailService
    public class EmailService : IEmailService
    {
        // שדה פרטי לקריאה בלבד עבור IConfiguration (לגישה להגדרות היישום)
        private readonly IConfiguration _configuration;
        // שדות פרטיים לקריאה בלבד לאחסון הגדרות שרת ה-SMTP
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _senderPassword; // אזהרה: לא מומלץ לאחסן סיסמאות בצורה כזו ב-production! יש להשתמש בפתרונות אבטחה כגון Azure Key Vault, AWS Secrets Manager או User Secrets במצב פיתוח.

        // בנאי המחלקה: מקבל את התלות IConfiguration דרך Dependency Injection
        public EmailService(IConfiguration configuration)
        {
            // אתחול שדה ה-IConfiguration עם המופע שהוזרק
            _configuration = configuration;
            // טעינת הגדרות שרת ה-SMTP מקובץ התצורה (לדוגמה, appsettings.json)
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            // טעינת פורט ה-SMTP והמרתו ל-int
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            // טעינת הגדרת ה-SSL והמרתה ל-bool
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);
            // טעינת שם השולח
            _senderName = _configuration["EmailSettings:SenderName"];
            // טעינת כתובת המייל של השולח
            _senderEmail = _configuration["EmailSettings:SenderEmail"];
            // טעינת סיסמת המייל של השולח (יש לטפל בזהירות בסיסמאות)
            _senderPassword = _configuration["EmailSettings:SenderPassword"];
        }

        // מתודה אסינכרונית לשליחת מייל
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // יצירת אובייקט MailMessage המייצג את המייל הנשלח
            var mail = new MailMessage();
            // הגדרת כתובת המייל ושם השולח
            mail.From = new MailAddress(_senderEmail, _senderName);
            // הוספת כתובת המייל של הנמען
            mail.To.Add(toEmail);
            // הגדרת נושא המייל
            mail.Subject = subject;
            // הגדרת גוף המייל
            mail.Body = message;
            // קביעה האם גוף המייל הוא HTML או טקסט רגיל (כאן מוגדר כטקסט רגיל)
            mail.IsBodyHtml = false;

            // שימוש בבלוק using כדי לוודא ש-SmtpClient נסגר ומשוחרר משאבים כראוי
            using (var smtpClient = new SmtpClient(_smtpServer))
            {
                // הגדרת הפורט לשרת ה-SMTP
                smtpClient.Port = _smtpPort;
                // הגדרת האם להשתמש ב-SSL לאבטחת התקשורת
                smtpClient.EnableSsl = _enableSsl;
                // הגדרת פרטי ההתחברות (שם משתמש וסיסמה) לשרת ה-SMTP
                smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                // שליחת המייל באופן אסינכרוני
                await smtpClient.SendMailAsync(mail);
            }
        }
    }
}