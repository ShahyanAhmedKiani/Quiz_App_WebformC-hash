using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace QuizApp
{
    public class QuizCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public int QuestionCount { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Text { get; set; }
        public string[] Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
    }

    public static class DatabaseManager
    {
        private static string dbPath = "quiz_app.db";
        private static string connectionString => $"Data Source={dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT UNIQUE NOT NULL, Password TEXT NOT NULL,
                FullName TEXT NOT NULL, IsAdmin INTEGER DEFAULT 0,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP)", conn).ExecuteNonQuery();

            new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE NOT NULL, Description TEXT,
                Icon TEXT DEFAULT '📚', IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP)", conn).ExecuteNonQuery();

            new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Questions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId INTEGER NOT NULL,
                QuestionText TEXT NOT NULL,
                OptionA TEXT NOT NULL, OptionB TEXT NOT NULL,
                OptionC TEXT NOT NULL, OptionD TEXT NOT NULL,
                CorrectOption INTEGER NOT NULL, Explanation TEXT,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY(CategoryId) REFERENCES Categories(Id))", conn).ExecuteNonQuery();

            new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS Results (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserEmail TEXT NOT NULL, UserName TEXT NOT NULL,
                CategoryId INTEGER NOT NULL, CategoryName TEXT NOT NULL,
                Score INTEGER NOT NULL, TotalQuestions INTEGER NOT NULL,
                Percentage REAL NOT NULL, Grade TEXT NOT NULL,
                TakenAt TEXT DEFAULT CURRENT_TIMESTAMP)", conn).ExecuteNonQuery();

            new SQLiteCommand(@"INSERT OR IGNORE INTO Users (Email,Password,FullName,IsAdmin)
                VALUES ('admin@quiz.com','Admin@123','Administrator',1)", conn).ExecuteNonQuery();

            long catCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Categories", conn).ExecuteScalar();
            if (catCount == 0) SeedData(conn);
        }

        private static void SeedData(SQLiteConnection conn)
        {
            var cats = new[] {
                (1,"C# Programming","C# language concepts","💻"),
                (2,"Parallel & Distributed Computing","Threads, processes, distributed systems","🔀"),
                (3,"Compiler Construction","Lexer, parser, code generation","⚙️"),
                (4,"Object Oriented Programming","OOP principles and patterns","🧩"),
                (5,"Data Structures & Algorithms","Arrays, trees, graphs, sorting","📊"),
            };
            foreach (var (id,name,desc,icon) in cats)
            {
                using var c = new SQLiteCommand("INSERT OR IGNORE INTO Categories (Id,Name,Description,Icon) VALUES (@id,@n,@d,@i)", conn);
                c.Parameters.AddWithValue("@id",id); c.Parameters.AddWithValue("@n",name);
                c.Parameters.AddWithValue("@d",desc); c.Parameters.AddWithValue("@i",icon);
                c.ExecuteNonQuery();
            }

            string ins = "INSERT OR IGNORE INTO Questions (CategoryId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation) VALUES (@cid,@q,@a,@b,@c,@d,@cor,@exp)";
            var qs = new (int cid,string q,string a,string b,string c,string d,int cor,string exp)[] {
                (1,"C# mein 'sealed' keyword ka matlab?","Inherit nahi kar sakte","Delete karta hai","Public banata hai","Abstract banata hai",0,"sealed se class inherit nahi hoti"),
                (1,"'namespace' kisliye use hota hai?","Variable declare","Classes group karne","Loop chalana","Database connect",1,"Namespace classes organize karta hai"),
                (1,"List<T> kahan se aata hai?","System.IO","System.Collections.Generic","System.Data","System.Threading",1,"List<T> generic collection hai"),
                (1,"'try-catch' kisliye hai?","Loop chalana","Exception handle karna","Variable declare","Class banana",1,"try-catch exceptions pakadta hai"),
                (1,"'interface' mein kya hota hai?","Implementation","Method signatures","Private vars","Constructor",1,"Interface sirf signatures define karta hai"),
                (1,"'var' C# mein kya karta hai?","Global var","Type inference","Const banata hai","Array declare",1,"var compiler ko type decide karne deta hai"),
                (1,"'async/await' kisliye?","Sorting","Async operations","DB queries","UI design",1,"async/await non-blocking code ke liye"),
                (1,"LINQ full form?","Language Integrated Query","List In Queue","Linear Index Query","Language Input Queue",0,"LINQ = Language Integrated Query"),
                (1,"'delegate' kya hota hai?","Class type","Function pointer","Variable type","Loop construct",1,"Delegate method reference hold karta hai"),
                (1,"'property' kya hoti hai?","Sirf field","get/set wrapper","Static method","Abstract class",1,"Property get/set accessor deti hai"),
                (2,"'race condition' kya hai?","Fastest thread jeetta","Multiple threads same data access","CPU race","Memory leak",1,"Race condition = uncontrolled shared data access"),
                (2,"'Mutex' ka kaam?","Memory allocate","Shared resource lock karna","Thread create","Process kill",1,"Mutex mutual exclusion enforce karta hai"),
                (2,"MPI full form?","Message Passing Interface","Multiple Processing Index","Memory Parallel Integration","Mutex Process Interface",0,"MPI distributed systems mein use hota hai"),
                (2,"'Deadlock' kab hoti hai?","Thread bahut fast chale","Threads ek doosre ka wait karen","Memory full","CPU overload",1,"Deadlock = circular waiting condition"),
                (2,"Amdahl's Law kya batata hai?","Memory bandwidth","Parallel speedup ki limit","Network latency","Thread count",1,"Amdahl's Law max parallel speedup batata hai"),
                (2,"CAP Theorem mein kya hai?","Consistency Availability Partition","CPU API Process","Cache Array Process","None",0,"CAP = Consistency Availability Partition tolerance"),
                (2,"Semaphore aur Mutex mein farq?","Koi farq nahi","Semaphore count-based Mutex binary","Mutex faster","Semaphore sirf OS mein",1,"Semaphore multiple access allow karta hai"),
                (2,"'Load Balancing' kisliye?","Data delete","Kaam multiple servers pe barant'na","Cache clear","Thread sleep",1,"Load balancing workload distribute karta hai"),
                (2,"'MapReduce' kya hai?","DB query","Large data parallel processing","Sorting algorithm","Network protocol",1,"MapReduce big data parallel processing hai"),
                (2,"Thread aur Process mein farq?","Koi farq nahi","Process alag memory thread share","Thread slower","Process threads nahi bana sakta",1,"Thread same memory share karta hai"),
                (3,"Compiler ka pehla phase?","Code Generation","Lexical Analysis","Syntax Analysis","Optimization",1,"Lexical Analysis tokens banata hai"),
                (3,"'Token' kya hota hai?","Error message","Source code smallest unit","Memory address","Output file",1,"Token lexer ka output hai"),
                (3,"'Parse Tree' kisliye?","Memory management","Syntax structure represent karna","Network routing","File storage",1,"Parse tree grammatical structure dikhati hai"),
                (3,"CFG full form?","Computer File Generator","Context Free Grammar","Code Flow Graph","Compiler Flag Guide",1,"CFG = Context Free Grammar"),
                (3,"'Symbol Table' kya store karta hai?","Errors","Variables/functions info","Binary code","Network addresses",1,"Symbol table identifiers track karta hai"),
                (3,"Intermediate code generation kisliye?","Direct machine code","Platform-independent representation","Error reporting","Lexing",1,"Intermediate code portable hota hai"),
                (3,"'Semantic Analysis' kya check karta hai?","Spelling","Type checking aur logical correctness","Network errors","Memory leaks",1,"Semantic analysis meaning check karta hai"),
                (3,"Shift-Reduce kaunse parser mein?","LL Parser","LR Parser","Recursive Descent","Earley Parser",1,"LR parser shift-reduce use karta hai"),
                (3,"Regular Expression compiler mein?","Code optimization","Lexical patterns define karna","Memory allocation","Type checking",1,"Regex tokens identify karta hai"),
                (3,"Dead Code Elimination kya hai?","Compiler crash","Never execute wala code hatana","Memory error","Infinite loop",1,"Dead code optimization mein hata diya jata hai"),
                (4,"'Encapsulation' kya hai?","Multiple inheritance","Data aur methods ko band karna","Method overloading","Abstract class",1,"Encapsulation data hiding provide karta hai"),
                (4,"'Polymorphism' ka matlab?","Single form","Ek cheez multiple forms mein","Multiple classes","Static binding",1,"Polymorphism = many forms"),
                (4,"Abstract Class aur Interface farq?","Koi farq nahi","Abstract partial implementation de sakta hai","Interface faster","Abstract inherit nahi",1,"Abstract class mein implementation ho sakti hai"),
                (4,"'Inheritance' ka faida?","Code delete","Code reuse aur extension","Memory save","Speed increase",1,"Inheritance parent code reuse karta hai"),
                (4,"'Singleton' kya karta hai?","Multiple objects","Sirf ek object ensure","Class delete","Interface implement",1,"Singleton = only one instance"),
                (4,"'Method Overloading' kya hai?","Runtime polymorphism","Same naam different parameters","Parent method replace","Abstract method",1,"Overloading = same name different signatures"),
                (4,"'Virtual' keyword kisliye?","Class hide","Override allow karna","Constructor banana","Interface implement",1,"virtual method override allow karta hai"),
                (4,"SOLID mein 'S' kya hai?","Speed","Single Responsibility Principle","Static","Singleton",1,"S = Single Responsibility Principle"),
                (4,"'Constructor' kab call hota hai?","Object delete par","Object create par","Method call par","Program end par",1,"Constructor object banate waqt call hota hai"),
                (4,"'Composition over Inheritance' matlab?","Inheritance avoid","Objects contain karo instead of inherit","Abstract use karo","Interface prefer",1,"Composition flexible design deta hai"),
                (5,"Binary Search complexity?","O(n)","O(log n)","O(n2)","O(1)",1,"Binary search har step mein half karta hai"),
                (5,"Stack kaunsa principle?","FIFO","LIFO","Random","Priority",1,"Stack = Last In First Out"),
                (5,"Queue kaunsa principle?","LIFO","FIFO","Random","Sorted",1,"Queue = First In First Out"),
                (5,"Linked List random access?","Haan O(1)","Nahi O(n)","Sirf sorted mein","Haan O(log n)",1,"Linked list sequential access karta hai"),
                (5,"Big O kya represent karta hai?","Exact time","Worst-case complexity","Memory used","Lines of code",1,"Big O = worst case complexity"),
                (5,"Binary Tree max children?","1","2","3","4",1,"Binary tree har node mein max 2 children"),
                (5,"Bubble Sort average complexity?","O(n log n)","O(n2)","O(n)","O(1)",1,"Bubble sort O(n2) average case"),
                (5,"Hash Table average lookup?","O(n)","O(log n)","O(1)","O(n2)",2,"Hash table O(1) average lookup"),
                (5,"BFS kya hai?","Backtracking","Breadth First Search","Binary Fast Search","None",1,"BFS level by level traverse karta hai"),
                (5,"Recursion mein Base Case kyu?","Speed","Infinite recursion rokna","Memory save","Output format",1,"Base case recursion terminate karta hai"),
            };
            foreach (var (cid,q,a,b,c,d,cor,exp) in qs)
            {
                using var cmd = new SQLiteCommand(ins, conn);
                cmd.Parameters.AddWithValue("@cid",cid); cmd.Parameters.AddWithValue("@q",q);
                cmd.Parameters.AddWithValue("@a",a); cmd.Parameters.AddWithValue("@b",b);
                cmd.Parameters.AddWithValue("@c",c); cmd.Parameters.AddWithValue("@d",d);
                cmd.Parameters.AddWithValue("@cor",cor); cmd.Parameters.AddWithValue("@exp",exp);
                cmd.ExecuteNonQuery();
            }
        }

        // CATEGORY CRUD
        public static List<QuizCategory> GetAllCategories(bool activeOnly = false)
        {
            var list = new List<QuizCategory>();
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            string where = activeOnly ? "WHERE c.IsActive=1" : "";
            string sql = $@"SELECT c.*, (SELECT COUNT(*) FROM Questions q WHERE q.CategoryId=c.Id AND q.IsActive=1) as QCount
                FROM Categories c {where} ORDER BY c.Name";
            using var r = new SQLiteCommand(sql, conn).ExecuteReader();
            while (r.Read())
                list.Add(new QuizCategory { Id=Convert.ToInt32(r["Id"]), Name=r["Name"].ToString(),
                    Description=r["Description"].ToString(), Icon=r["Icon"].ToString(),
                    IsActive=Convert.ToInt32(r["IsActive"])==1, QuestionCount=Convert.ToInt32(r["QCount"]) });
            return list;
        }

        public static bool AddCategory(QuizCategory cat)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                using var cmd = new SQLiteCommand("INSERT INTO Categories (Name,Description,Icon) VALUES (@n,@d,@i)", conn);
                cmd.Parameters.AddWithValue("@n",cat.Name); cmd.Parameters.AddWithValue("@d",cat.Description??"");
                cmd.Parameters.AddWithValue("@i",cat.Icon??"📚"); cmd.ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        public static bool UpdateCategory(QuizCategory cat)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                using var cmd = new SQLiteCommand("UPDATE Categories SET Name=@n,Description=@d,Icon=@i,IsActive=@a WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@n",cat.Name); cmd.Parameters.AddWithValue("@d",cat.Description??"");
                cmd.Parameters.AddWithValue("@i",cat.Icon??"📚"); cmd.Parameters.AddWithValue("@a",cat.IsActive?1:0);
                cmd.Parameters.AddWithValue("@id",cat.Id); cmd.ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        public static bool DeleteCategory(int id)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                new SQLiteCommand($"DELETE FROM Results WHERE CategoryId={id}", conn).ExecuteNonQuery();
                new SQLiteCommand($"DELETE FROM Questions WHERE CategoryId={id}", conn).ExecuteNonQuery();
                new SQLiteCommand($"DELETE FROM Categories WHERE Id={id}", conn).ExecuteNonQuery();
                return true;
            } catch { return false; }
        }

        public static bool ToggleCategoryStatus(int id)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                new SQLiteCommand($"UPDATE Categories SET IsActive=1-IsActive WHERE Id={id}", conn).ExecuteNonQuery();
                return true;
            } catch { return false; }
        }

        // QUESTION CRUD
        public static List<Question> GetQuestionsByCategory(int categoryId)
        {
            var list = new List<Question>();
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            string sql = @"SELECT q.*, c.Name as CatName FROM Questions q
                JOIN Categories c ON q.CategoryId=c.Id
                WHERE q.CategoryId=@cid AND q.IsActive=1 ORDER BY RANDOM() LIMIT 10";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid", categoryId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Question { Id=Convert.ToInt32(r["Id"]), CategoryId=Convert.ToInt32(r["CategoryId"]),
                    CategoryName=r["CatName"].ToString(), Text=r["QuestionText"].ToString(),
                    Options=new[]{r["OptionA"].ToString(),r["OptionB"].ToString(),r["OptionC"].ToString(),r["OptionD"].ToString()},
                    CorrectIndex=Convert.ToInt32(r["CorrectOption"]), Explanation=r["Explanation"].ToString(),
                    IsActive=Convert.ToInt32(r["IsActive"])==1 });
            return list;
        }

        public static DataTable GetQuestionsByCategoryTable(int categoryId)
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            string sql = @"SELECT q.Id, q.QuestionText as 'Sawal', q.OptionA as 'A', q.OptionB as 'B',
                q.OptionC as 'C', q.OptionD as 'D',
                CASE q.CorrectOption WHEN 0 THEN 'A' WHEN 1 THEN 'B' WHEN 2 THEN 'C' ELSE 'D' END as 'Sahi',
                q.Explanation as 'Waja',
                CASE q.IsActive WHEN 1 THEN 'Active ✅' ELSE 'Band ❌' END as 'Status'
                FROM Questions q WHERE q.CategoryId=@cid ORDER BY q.Id";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid", categoryId);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable(); adapter.Fill(dt); return dt;
        }

        public static Question GetQuestionById(int id)
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Questions WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new Question { Id=Convert.ToInt32(r["Id"]), CategoryId=Convert.ToInt32(r["CategoryId"]),
                    Text=r["QuestionText"].ToString(),
                    Options=new[]{r["OptionA"].ToString(),r["OptionB"].ToString(),r["OptionC"].ToString(),r["OptionD"].ToString()},
                    CorrectIndex=Convert.ToInt32(r["CorrectOption"]), Explanation=r["Explanation"].ToString(),
                    IsActive=Convert.ToInt32(r["IsActive"])==1 };
            return null;
        }

        public static bool AddQuestion(Question q)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                using var cmd = new SQLiteCommand(@"INSERT INTO Questions (CategoryId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation)
                    VALUES (@cid,@q,@a,@b,@c,@d,@cor,@exp)", conn);
                cmd.Parameters.AddWithValue("@cid",q.CategoryId); cmd.Parameters.AddWithValue("@q",q.Text);
                cmd.Parameters.AddWithValue("@a",q.Options[0]); cmd.Parameters.AddWithValue("@b",q.Options[1]);
                cmd.Parameters.AddWithValue("@c",q.Options[2]); cmd.Parameters.AddWithValue("@d",q.Options[3]);
                cmd.Parameters.AddWithValue("@cor",q.CorrectIndex); cmd.Parameters.AddWithValue("@exp",q.Explanation??"");
                cmd.ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        public static bool UpdateQuestion(Question q)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                using var cmd = new SQLiteCommand(@"UPDATE Questions SET CategoryId=@cid,QuestionText=@q,
                    OptionA=@a,OptionB=@b,OptionC=@c,OptionD=@d,CorrectOption=@cor,Explanation=@exp,IsActive=@active WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@cid",q.CategoryId); cmd.Parameters.AddWithValue("@q",q.Text);
                cmd.Parameters.AddWithValue("@a",q.Options[0]); cmd.Parameters.AddWithValue("@b",q.Options[1]);
                cmd.Parameters.AddWithValue("@c",q.Options[2]); cmd.Parameters.AddWithValue("@d",q.Options[3]);
                cmd.Parameters.AddWithValue("@cor",q.CorrectIndex); cmd.Parameters.AddWithValue("@exp",q.Explanation??"");
                cmd.Parameters.AddWithValue("@active",q.IsActive?1:0); cmd.Parameters.AddWithValue("@id",q.Id);
                cmd.ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        public static bool DeleteQuestion(int id)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                new SQLiteCommand($"DELETE FROM Questions WHERE Id={id}", conn).ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        public static bool ToggleQuestionStatus(int id)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                new SQLiteCommand($"UPDATE Questions SET IsActive=1-IsActive WHERE Id={id}", conn).ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        // AUTH
        public static User AuthenticateUser(string email, string password)
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Users WHERE Email=@e AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@e",email); cmd.Parameters.AddWithValue("@p",password);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new User { Id=Convert.ToInt32(r["Id"]), Email=r["Email"].ToString(),
                    FullName=r["FullName"].ToString(), IsAdmin=Convert.ToInt32(r["IsAdmin"])==1 };
            return null;
        }

        public static bool RegisterUser(string email, string password, string fullName)
        {
            try {
                using var conn = new SQLiteConnection(connectionString); conn.Open();
                using var cmd = new SQLiteCommand("INSERT INTO Users (Email,Password,FullName) VALUES (@e,@p,@n)", conn);
                cmd.Parameters.AddWithValue("@e",email); cmd.Parameters.AddWithValue("@p",password);
                cmd.Parameters.AddWithValue("@n",fullName); cmd.ExecuteNonQuery(); return true;
            } catch { return false; }
        }

        // RESULTS
        public static void SaveResult(string email, string name, int catId, string catName, int score, int total)
        {
            double pct = (double)score/total*100;
            string grade = pct>=90?"A+":pct>=80?"A":pct>=70?"B":pct>=60?"C":pct>=50?"D":"F";
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            using var cmd = new SQLiteCommand(@"INSERT INTO Results (UserEmail,UserName,CategoryId,CategoryName,Score,TotalQuestions,Percentage,Grade)
                VALUES (@e,@n,@cid,@cn,@s,@t,@p,@g)", conn);
            cmd.Parameters.AddWithValue("@e",email); cmd.Parameters.AddWithValue("@n",name);
            cmd.Parameters.AddWithValue("@cid",catId); cmd.Parameters.AddWithValue("@cn",catName);
            cmd.Parameters.AddWithValue("@s",score); cmd.Parameters.AddWithValue("@t",total);
            cmd.Parameters.AddWithValue("@p",pct); cmd.Parameters.AddWithValue("@g",grade);
            cmd.ExecuteNonQuery();
        }

        public static DataTable GetAllResults(int categoryId = -1)
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            string where = categoryId > 0 ? $"WHERE CategoryId={categoryId}" : "";
            string sql = $@"SELECT UserName as 'Student', UserEmail as 'Email', CategoryName as 'Subject',
                Score, TotalQuestions as 'Total', ROUND(Percentage,1) as 'Percentage %', Grade,
                TakenAt as 'Date & Time' FROM Results {where} ORDER BY TakenAt DESC";
            using var adapter = new SQLiteDataAdapter(sql, conn);
            var dt = new DataTable(); adapter.Fill(dt); return dt;
        }

        public static DataTable GetUserResults(string email)
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            using var cmd = new SQLiteCommand(@"SELECT CategoryName as 'Subject', Score,
                TotalQuestions as 'Total', ROUND(Percentage,1) as 'Percentage %', Grade,
                TakenAt as 'Date & Time' FROM Results WHERE UserEmail=@e ORDER BY TakenAt DESC", conn);
            cmd.Parameters.AddWithValue("@e", email);
            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable(); adapter.Fill(dt); return dt;
        }

        public static DataTable GetResultsByCategory()
        {
            using var conn = new SQLiteConnection(connectionString); conn.Open();
            string sql = @"SELECT CategoryName as 'Subject', COUNT(*) as 'Attempts',
                COUNT(DISTINCT UserEmail) as 'Students', ROUND(AVG(Percentage),1) as 'Avg %',
                SUM(CASE WHEN Percentage>=50 THEN 1 ELSE 0 END) as 'Pass',
                SUM(CASE WHEN Percentage<50 THEN 1 ELSE 0 END) as 'Fail'
                FROM Results GROUP BY CategoryName ORDER BY Attempts DESC";
            using var adapter = new SQLiteDataAdapter(sql, conn);
            var dt = new DataTable(); adapter.Fill(dt); return dt;
        }
    }
}
