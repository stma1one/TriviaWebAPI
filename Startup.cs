using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TriviaWebAPI.DataTransferObjects;

namespace TriviaWebAPI
{
    public class DataSingleton
    {
        public List<AmericanQuestion> Questions { get; set; }
        public List<User> Users { get; set; }
        public DataSingleton()
        {
            Questions = InitQuestions();
            Users = InitUsers();
        }
        private List<User> InitUsers()
        {
            List<User> users = new List<User>();
            List<AmericanQuestion> userQuestions = new List<AmericanQuestion>();
            foreach (AmericanQuestion item in this.Questions)
            {
                userQuestions.Add(item);
            }
            users.Add(new User
            {
                Email = "kuku@kaka.com",
                NickName = "kuku",
                Password = "kaka",
                Questions = userQuestions
            });
            return users;
        }
        private List<AmericanQuestion> InitQuestions()
        {
            string[] questionsArr = { "In which year Israel was established?", "Who is the president of USA?", "When Michael Jordan played for the Chicago Bulls, how many NBA Championships did he win?", "Which Williams sister has won more Grand Slam titles?", " Which racer holds the record for the most Grand Prix wins?" };
            string[] answersArr = { "1948", "Joe Biden", "Six", "Serena", "Michael Schumacher" };
            string[,] otherAnswersArr = { { "1938", "1947", "1958" }, { "Donald Trump", "Arnold Shvarzeneger", "Barak Obama" }, { "Five", "Four", "None" }, { "Venus", "Both won same number", "Both never won a grandslam" }, { "Juan Manuel Fangio", "Jack Brabham", "Sebastian Vettel" } };

            List<AmericanQuestion> questions = new List<AmericanQuestion>();
            for (int i = 0; i < questionsArr.Length; i++)
            {
                string[] otherQ = new string[3];
                for (int j = 0; j < 3; j++)
                {
                    otherQ[j] = otherAnswersArr[i, j];
                }
                questions.Add(new AmericanQuestion()
                {
                    QText = questionsArr[i],
                    CorrectAnswer = answersArr[i],
                    OtherAnswers = otherQ,
                    CreatorNickName = "kuku"
                });
            }
            return questions;
        }

    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add the Controllers to the service!
            //without this line of code no routing to the AmericanQueustionsControllerClass will be done
            services.AddControllers();

            #region Add Session support
            //The following two commands set the Session state to work!
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            #endregion

            #region Add Singletone example
            //Add a dependency injection with a singletone scope! the object will be created on the first injection
            //and disposed when the app will go down.
            services.AddSingleton(typeof(DataSingleton));
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Development and Https redirection support
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            #endregion

            #region Static file support
            //To use static files wwwroot (Add wwwroot folder first)
            app.UseStaticFiles();
            #endregion

            app.UseRouting();

            app.UseAuthorization();

            #region Session support
            //Tells the application to use Session!
            app.UseSession();
            #endregion

            app.UseEndpoints(endpoints =>
            {
                //Map all routings for controllers
                endpoints.MapControllers();
            });

            
        }
    }
}
