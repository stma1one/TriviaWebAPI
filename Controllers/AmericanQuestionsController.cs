using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        [Route ("Hello")]
        public async Task<IActionResult> Hello()
        {
            return  Ok("hello World");
        }

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

        [Route("GetAllQuestions")]
        [HttpGet]
        public List<AmericanQuestion> GetAllQuestions([FromQuery] string secret)
        {
            if (secret == "kuku")
                return this.questions;
            return new List<AmericanQuestion>();
        }

        [Route("GetAllUsers")]
        [HttpGet]
        public List<User> GetAllUsers([FromQuery] string secret)
        {
            if (secret == "kuku")
                return this.users;
            return new List<User>();
        }

        [Route("GetRandomQuestion")]
        [HttpGet]
        public AmericanQuestion GetRandomQuestion()
        {
            return this.questions[r.Next(this.questions.Count)];
        }

        [Route("PostNewQuestion")]
        [HttpPost]
        public bool PostNewQuestion([FromBody] AmericanQuestion q)
        {
            //check if login was made
            string user = HttpContext.Session.GetObject<string>("user");

            if (user == q.CreatorNickName)
            {
                User u = users.Find(us => us.NickName == q.CreatorNickName);
                this.questions.Add(q);
                u?.Questions.Add(q);
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }

        [Route("DeleteQuestion")]
        [HttpPost]
        public bool DeleteQuestion([FromBody] AmericanQuestion q)
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
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }

        [Route("RegisterUser")]
        [HttpPost]
        public bool RegisterUser([FromBody] User u)
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
                Login(u);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
