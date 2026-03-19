# 📝 Quiz Master — C# WinForms Quiz Application

A fully functional **desktop quiz application** built with **C# Windows Forms** and **SQLite**, featuring multi-subject quizzes, user authentication, admin dashboard, and result tracking.

---

## ✨ Features

### 👤 User Side
- Email & Password Login / Registration
- Subject selection with card-based UI
- 10 MCQ quiz per subject (randomized, 10-min timer)
- Answer navigation (Previous / Next)
- Result screen with Score, Grade, Percentage, Pass/Fail
- Answer review with correct answers highlighted
- Personal quiz history per subject

### 👑 Admin Side
- Secure admin login
- Dashboard: Total Attempts, Students, Avg Score, Pass Rate
- **4 Tabs:** All Results | Subject Summary | Category Management | Question Management
- Add/Edit/Delete subjects (categories) at runtime
- Add/Edit/Delete/Toggle questions per subject
- Filter results by subject, search by name/email
- Export results to CSV

---

## 🗂️ Project Structure

```
QuizApp/
├── Program.cs                → App entry point
├── DatabaseManager.cs        → SQLite database + all CRUD
├── QuizData.cs               → Loads questions from DB
├── LoginForm.cs              → Login screen
├── RegisterForm.cs           → New user registration
├── CategorySelectForm.cs     → Subject selection cards
├── QuizForm.cs               → Quiz interface with timer
├── ResultForm.cs             → Result + answer review
├── AdminDashboard.cs         → Admin panel with 4 tabs
├── QuestionManagerForm.cs    → Question CRUD manager
└── QuizApp.csproj            → Project configuration
```

---

## 🛠️ Tech Stack

| Technology | Usage |
|-----------|-------|
| **C# .NET 8.0** | Core language |
| **Windows Forms** | UI framework |
| **SQLite** | Local database (auto-created) |
| **System.Data.SQLite** | NuGet package |

---

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK

### Run

1. Clone the repo
   ```bash
   git clone https://github.com/yourusername/QuizMaster.git
   ```
2. Open `QuizApp.csproj` in Visual Studio 2022
3. Press **F5**

> NuGet packages restore automatically on first build.

---

## 🔐 Default Login

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@quiz.com | Admin@123 |
| **User** | Register yourself | Your choice |

---

## 📊 Default Data

5 subjects seeded automatically with 10 questions each:
- 💻 C# Programming
- 🔀 Parallel & Distributed Computing
- ⚙️ Compiler Construction
- 🧩 Object Oriented Programming
- 📊 Data Structures & Algorithms

> Delete `quiz_app.db` to reset all data.

---

## 🎨 UI Theme

- **Colors:** Dark Green `#0F4B28` + Gold `#B99137`
- Split-panel layout, card-based subject selection
- All windows resizable & maximizable with scrollbars

---

## 📄 License

MIT License — free to use and modify.

---

*Built with C# Windows Forms ❤️
