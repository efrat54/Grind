using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper; 
using Grind.Core.DTOs; 
using Grind.Core.Entities; 
using Grind.Core.Interfaces; 
using Grind.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Grind.Service.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public TrainerService(DataContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService; 
        }

        //לא בשימוש כרגע
        public async Task<TrainerSalaryDTO> GetTrainerMonthlySalaryAsync(int trainerId)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Classes)
                .Include(t => t.TrainerPayments)
                .FirstOrDefaultAsync(t => t.Id == trainerId);

            if (trainer == null) return null;

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            decimal monthlySalary = 0;
            int classesTaughtThisMonth = 0;

            // בלוק קוד לחישוב סכום התשלומים שכבר בוצעו למאמן עבור החודש הנוכחי
            // var paymentsMadeThisMonth = trainer.TrainerPayments
            //     .Where(tp => tp.PaymentDate.Month == currentMonth && tp.PaymentDate.Year == currentYear)
            //     .Sum(tp => tp.Amount);
            // הערה: משתנה זה מחושב אך לא ישמש ישירות ב-DTO אם אין בו את המאפיינים הללו.
            // אם תרצה להשתמש בו, תצטרך להוסיף אותו ל-TrainerSalaryDTO.

            // בלוק קוד לחישוב מספר השיעורים שהמאמן לימד החודש
            classesTaughtThisMonth = trainer.Classes
                .Count(c => c.StartTime.Month == currentMonth && c.StartTime.Year == currentYear && !c.IsCancelled);

            monthlySalary = classesTaughtThisMonth * trainer.HourlyRate;

            // יוצרת ומחזירה אובייקט TrainerSalaryDTO עם הנתונים המחושבים
            // המחזירה רק את המאפיינים הקיימים ב-TrainerSalaryDTO המקורי שלך.
            return new TrainerSalaryDTO
            {
                // מזהה המאמן
                TrainerId = trainerId,
                // השכר החודשי המחושב המגיע למאמן
                MonthlySalary = monthlySalary,
                // מספר השיעורים שהמאמן לימד החודש
                ClassesTaughtThisMonth = classesTaughtThisMonth
                // אין את AmountPaidThisMonth ו-BalanceDue כאן, אלא אם תוסיף אותם ל-TrainerSalaryDTO.
            };
        }
        //לא בשימוש כרגע
        public async Task<string> AddTrainerAsync(TrainerRegistrationDTO trainerDto)
        {
            if (await _context.People.AnyAsync(p => p.Username == trainerDto.Username))
            {
                return "שם משתמש זה כבר קיים.";
            }

            if (await _context.People.AnyAsync(p => p.Email == trainerDto.Email))
            {
                return "כתובת דוא\"ל זו כבר רשומה במערכת.";
            }

            var trainer = _mapper.Map<Trainer>(trainerDto);

            trainer.HashedPassword = BCrypt.Net.BCrypt.HashPassword(trainerDto.Password);

            trainer.HireDate = DateTime.UtcNow;
           
            if (trainerDto.Address != null)
            {
                var address = _mapper.Map<Address>(trainerDto.Address);
                _context.Addresses.Add(address); 
                await _context.SaveChangesAsync();
                trainer.AddressId = address.Id; 
                trainer.Address = address; 
            }
            else
            {
                return "פרטי כתובת חובה.";
            }

            _context.Trainers.Add(trainer);

            await _context.SaveChangesAsync();

            return null; 
        }
        //כרגע לא בשימוש
        public async Task<IEnumerable<TrainerDTO>> GetAllTrainersAsync()
        {
            var trainers = await _context.Trainers
                                         .Include(t => t.Address) 
                                         .ToListAsync();

            return _mapper.Map<IEnumerable<TrainerDTO>>(trainers);
        }
        public async Task<TrainerDTO> GetTrainerProfileAsync(int trainerId)
        {
            var trainer = await _context.Trainers
                                        .Include(t => t.Address)
                                        .FirstOrDefaultAsync(t => t.Id == trainerId);
            return _mapper.Map<TrainerDTO>(trainer);
        }

        public async Task<TrainerDTO?> UpdateTrainerProfileAsync(int trainerId, TrainerProfileUpdateDTO dto)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Address)
                .FirstOrDefaultAsync(t => t.Id == trainerId);

            if (trainer == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != trainer.Email)
            {
                bool emailExists = await _context.People
                    .AnyAsync(p => p.Email == dto.Email && p.Id != trainerId);

                if (emailExists)
                    return null; 
            }

            trainer.FirstName = dto.FirstName;
            trainer.LastName = dto.LastName;
            trainer.Email = dto.Email;
            trainer.PhoneNumber = dto.PhoneNumber;
            //שדות לא מאופשרים
            trainer.DateOfBirth = dto.DateOfBirth;
            trainer.HireDate = dto.HireDate;
            trainer.HourlyRate = dto.HourlyRate ?? trainer.HourlyRate;

            if (dto.Specializations != null)
                trainer.Specializations = dto.Specializations;

            if (dto.Address != null)
            {
                trainer.Address.Street = dto.Address.Street;
                trainer.Address.City = dto.Address.City;
                trainer.Address.ApartmentNumber = dto.Address.ApartmentNumber;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<TrainerDTO>(trainer);
        }
        //כרגע לא בשימוש
        public async Task<string> TerminateTrainerAsync(int trainerId)
        {
            var trainer = await _context.Trainers.Include(t => t.Classes).ThenInclude(c => c.ClientClasses) .ThenInclude(cc => cc.Client) .FirstOrDefaultAsync(t => t.Id == trainerId);

            if (trainer == null)
            {
                return "מאמן לא נמצא.";
            }

            var now = DateTime.Now; 

            var futureClasses = trainer.Classes.Where(c => c.StartTime > now && !c.IsCancelled).ToList();

            foreach (var cls in futureClasses)
            {
                cls.IsCancelled = true;
                cls.CancellationReason = $"השיעור בוטל עקב פיטורין של המאמן {trainer.FirstName} {trainer.LastName}.";

                var registeredClients = cls.ClientClasses.Select(cc => cc.Client).ToList();

                // הסר את רישומי הלקוחות משיעורים אלה (אופציונלי, תלוי בלוגיקה העסקית שלך)
                // אם אתה רוצה למחוק את הרשומות של הלקוחות מהשיעור שבוטל:
                // _context.ClientClasses.RemoveRange(cls.ClientClasses);
                // הערה: אם אתה עושה זאת, ודא שללקוח יש מסלול ביטול אחר (למשל זיכוי)

                // שלח מייל לכל לקוח רשום
                foreach (var client in registeredClients)
                {
                    var emailSubject = "עדכון חשוב: שיעור בוטל ב-Grind";
                    var emailBody = $@"
                        שלום {client.FirstName},<br><br>
                        אנו מודיעים לך כי השיעור שלך **'{cls.Name}'** (קטגוריה: {cls.Category}) ביום {cls.StartTime.ToShortDateString()} בשעה {cls.StartTime.ToShortTimeString()} עם המאמן {trainer.FirstName} {trainer.LastName} **בוטל**.
                        <br><br>
                        הביטול הוא עקב שינויים במערכת המאמנים שלנו.
                        <br><br>
                        אנו מתנצלים על אי הנוחות. אנא בדוק את לוח הזמנים שלך לאפשרויות חלופיות.
                        <br><br>
                        בברכה,<br>
                        צוות Grind
                    ";
                    await _emailService.SendEmailAsync(client.Email, emailSubject, emailBody);
                }
            }

            // **2. סימון המאמן כלא פעיל**
            // בהנחה שלישות Person (ש-Trainer יורש ממנה) כוללת שדה IsActive
            trainer.IsActive = false;
            // אם יש לך שדה IsTerminated ספציפי למאמן, השתמש בו
            // trainer.IsTerminated = true;

            // **3. (אופציונלי) טיפול בשכר / תשלומים למאמן**
            // כאן תוכל להוסיף לוגיקה לחישוב פיצויים, סגירת חשבון, מחיקת נתוני שכר וכו'.

            // שמור את כל השינויים במסד הנתונים
            await _context.SaveChangesAsync();

            return null; // הצליח - המאמן פוטר בהצלחה
        }

    }
}