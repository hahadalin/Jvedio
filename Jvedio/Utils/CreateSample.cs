using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;

namespace Jvedio.Utils
{
    public  class CreateSample
    {

        public int number = 1000;
        private int defaultmax = 500;

        public CreateSample(int number)
        {
            this.number = number;
        }

        public CreateSample()
        {

        }

        public void Create()
        {
            int max = number;
            string savepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", $"sample_{max}.sqlite");
            MySqlite db = new MySqlite(savepath, true);
            db.CreateTable(DataBase.SQLITETABLE_MOVIE);
            db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
            db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
            db.CreateTable(DataBase.SQLITETABLE_JAVDB);
            db.CloseDB();


            using (MySqlite mySqlite = new MySqlite(savepath, true))
            {
                for (int i = 0; i < max; i++)
                {
                    Movie movie = new Movie()
                    {
                        id = "id-" + i,
                        favorites = new Random(i * max).Next(0, 5),
                        visits = new Random(i * max).Next(0, 100),
                        title = "名称" + new Random(i * max).Next(max),
                        runtime= new Random(i * max).Next(0, 300),
                        rating = new Random(i * max).Next(0, 100)/10,
                        filesize =Math.Abs( 5 * 1024 * new Random(i * max).Next(0, 1024 * 1024)),
                        subsection = i % 100 == 0 ? "path1;path2" : "",
                        scandate = DateTime.Now.AddDays(-new Random(i * max).Next(-500,500)).ToString("yyyy-MM-dd HH:mm:ss"),
                        otherinfo = DateTime.Now.AddDays(-new Random(i * max+1).Next(-500, 500)). ToString("yyyy-MM-dd HH:mm:ss"),
                        releasedate= DateTime.Now.AddDays(-new Random(i * max+2).Next(-500, 500)).ToString("yyyy-MM-dd"),
                        vediotype = new Random(i * max).Next(1, 3),
                        tag = "系列" + new Random(i * max + 3).Next(defaultmax),
                        director = "导演" + new Random(i * max + 4).Next(defaultmax),
                        studio = "发行商" + new Random(i * max + 6).Next(defaultmax)
                    };
                    movie.genre = GetGenre(movie);
                    movie.actor = GetActor(max);
                    movie.label = GetLabel(max);
                    mySqlite.InsertFullMovie(movie, "movie");
                    Console.WriteLine(i);
                }
            }
        }

        private string GetGenre(Movie movie)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 20);
            for (int i = 0; i < max; i++)
            {
                if (movie.vediotype == 1)
                {
                    var l = GenreUncensored[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
                else if (movie.vediotype == 2)
                {
                    var l = GenreCensored[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
                else if (movie.vediotype == 3)
                {
                    var l = GenreEurope[new Random(i * max).Next(0, 6)].Split(',').ToList();
                    result.Add(l[new Random(i * max + 1).Next(0, l.Count - 1)]);
                }
            }
            return string.Join(" ", result);
        }

        private string GetActor(int maxcount)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 50);
            for (int i = 0; i < max; i++)
            {
                result.Add("演员" + new Random(i * max).Next(1, maxcount));
            }
            return string.Join(" ", result);
        }
        private string GetLabel( int maxcount)
        {
            List<string> result = new List<string>();
            int max = new Random().Next(0, 10);
            for (int i = 0; i < max; i++)
            {
                result.Add("标签" + new Random(i * max).Next(1, maxcount));
            }
            if (new Random(max).Next(maxcount) % 10 == 0) result.Add("高清");
            if (new Random(max+1).Next(maxcount) % 20 == 0) result.Add("中文");
            if (new Random(max + 1).Next(maxcount) % 100 == 0) result.Add("流出");
            return string.Join(" ", result);
        }

    }
}
