using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TriviaWebAPI.DataTransferObjects;

namespace TriviaWebAPI.Controllers
{
    [Route("AmericanQuestions")]
    [ApiController]
    public class AmericanQuestionsController : ControllerBase
    {
        private List<AmericanQuestion> questions;
        private List<User> users;
        private static Random r = new Random();
        
        public AmericanQuestionsController(DataSingleton data)
        {
            this.questions = data.Questions;
            this.users = data.Users;
        }
        /// <summary>
        /// פעולה המחזירה את הטקסט 
        /// hello world
        /// משמשת רק לצורך בדיקה
        /// </summary>
        /// <returns></returns>
        [Route ("Hello")]
        [HttpGet]
        public async Task<IActionResult> Hello()
        {
            return  Ok("hello World");
        }

        /// <summary>
        /// פעולת התחברות
        /// </summary>
        /// <param name="usr">אובייקט מסוג יוזר</param>
        /// <returns> אוביקט משתמש</returns>
        /// <response code="200">מחזיר את אובייקט המשתמש שהתחבר</response>
        /// <response code="403">מחזיר null  אם יש בעית בשם משתמש או סיסמה</response>
        [Route("Login")]
        //[HttpGet]
        [HttpPost]
        
        public async  Task<ActionResult<User>> Login([FromBody] User usr)
        {
            
            User user = null;
            //Search for user
            foreach (User u in users)
            {
                //Check user name and password
                if (u.Email == usr.Email && u.Password == usr.Password)
                {
                    user = u;
                    
                    HttpContext.Session.SetObject("user", u.NickName);
                    return Ok(user);    
                }
            }
            //Check user name and password
            
               return Forbid();
           
        }

        /// <summary>
        /// פעולה המקבלת שם של משתמש ומחזירה את האימייל שלו
        /// </summary>
        /// <param name="nick">שם היוזר</param>
        /// <returns>האימייל של היוזר</returns>
        /// <response code="200">מחזיר את המייל</response>
        /// <response code="404">אם לא נמצא יוזר כזה</response>

        [Route ("GetUserEmail")]
        [HttpGet]

        public async Task<ActionResult> GetUserEmail([FromQuery] string nick)
        {
            foreach(User u in users)
            {
                if (u.NickName == nick)
                    return Ok(u.Email);
            }
            return NotFound();
        }

        /// <summary>
        /// מחזירה את כל השאלות 
        /// </summary>
        /// <param name="secret">מילת הקוד המאפשרת קבלת הנתונים</param>
        /// <returns>אוסף כל השאלות</returns>
        /// <response code="200">אוסף כל השאלות </response>
        /// <response code="404">מערך ריק של אם הקוד לא תקין </response>
        /// </response>
        [Route("GetAllQuestions")]
        [HttpGet]
        public async Task<ActionResult<List<AmericanQuestion>>> GetAllQuestions([FromQuery] string secret)
        {
            if (secret == "kuku")
                return Ok(this.questions);
            return Unauthorized(new List<AmericanQuestion>());
        }
        /// <summary>
        /// פעולה המחזירה את כל המשתמשים בהנתן מילת הקוד
        /// </summary>
        /// <param name="secret">מילת הקוד</param>
        /// <returns>מערך משתמשים</returns>
        /// <response code="200">רשימת כל המשתמשים</response>
        /// <response code="403">רשימה  ריקה של משתמשים</response>
        [Route("GetAllUsers")]
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers([FromQuery] string secret)
        {
            if (secret == "kuku")
                return Ok(this.users);
            return Unauthorized(new List<User>());
        }

        /// <summary>
        /// פעולה המחזירה שאלה אקראית מהמאגר
        /// </summary>
        /// <returns>שאלה אמריקאית</returns>
        /// <response code="200">שאלה אמריקאית רנדומלית מהמאגר</response>
        [Route("GetRandomQuestion")]
        [HttpGet]
        public async Task<ActionResult<AmericanQuestion>> GetRandomQuestion()
        {
            return Ok(this.questions[r.Next(this.questions.Count)]);
        }
        /// <summary>
        /// הכנסת שאלה חדשה למאגר.
        /// מותנה בביצוע לוגאין
        /// </summary>
        /// <param name="q">שאלה חדשה להכנסה למאחר</param>
        /// <returns>אמת אם הצלחי ושקר אחרת</returns>
        /// <response code="201">השאלה התווספה</response>
        /// <response code="403">אין הרשאה</response>
        [Route("PostNewQuestion")]
        [HttpPost]
        public async Task<ActionResult<bool>> PostNewQuestion([FromBody] AmericanQuestion q)
        {
            //check if login was made
            string user = HttpContext.Session.GetObject<string>("user");

            if (user == q.CreatorNickName)
            {
                User u = users.Find(us => us.NickName == q.CreatorNickName);
                this.questions.Add(q);
                u?.Questions.Add(q);
                return Created(q.CreatorNickName,true);
            }
            else
            {
               // Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return Unauthorized(false);
            }
        }
        /// <summary>
        /// פעולה המוחקת אוביקט שאלה מהמאגר
        /// </summary>
        /// <param name="q">השאלה למחיקה</param>
        /// <returns>אמת אם בוצע ושקר אחרת</returns>
        /// <response code="200">מחיקה בוצעה</response>
        /// <response code="404">לא נמצאו רשומות למחיקה</response>
        /// <response code="403">לא רשאי למחוק</response>
        [Route("DeleteQuestion")]
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteQuestion([FromBody] AmericanQuestion q)
        {
            //check if login was made
            string user = HttpContext.Session.GetObject<string>("user");

            if (user == q.CreatorNickName)
            {
                foreach (AmericanQuestion item in questions)
                {
                    if (item.QText == q.QText && q.CreatorNickName == user)
                    {
                        User u = users.Find(us => us.NickName == q.CreatorNickName);
                         questions.Remove(item);
                        u?.Questions.Remove(item);
                        return  Ok(true);
                    }
                }
                return NotFound(false);
            }
            else
            {
               // Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;

                return Unauthorized(false);
            }
        }
        /// <summary>
        /// רישום יוזר חדש
        /// </summary>
        /// <param name="u">אובייקט יוזר</param>
        /// <returns>אמת אם הצליח ושקר אחרת </returns>
        /// <response code="201">נוצר בהצלחה</response>
        /// <response code="409">משתמש כזה כבר קיים</response>
        [Route("RegisterUser")]
        [HttpPost]
        public async Task<ActionResult<bool>> RegisterUser([FromBody] User u)
        {
            bool found = false;
            foreach (User item in users)
            {
                if (item.NickName == u.NickName || item.Email == u.Email)
                {
                    found = true;
                }
            }
            if (!found)
            {
                if (u.Questions == null)
                    u.Questions = new List<AmericanQuestion>();

                this.users.Add(u);
                await Login(u);
                return Created(u.NickName,true);
            }
            else
            {
                return Conflict(false);
            }
        }
    }
}
