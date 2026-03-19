using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuizApp
{
    public class CategorySelectForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);
        static readonly Color Cream = Color.FromArgb(245, 250, 245);

        static readonly Color[] Accents = {
            Color.FromArgb(35,  75, 155),
            Color.FromArgb(0,  140,  95),
            Color.FromArgb(200, 110,  20),
            Color.FromArgb(95,  40, 140),
            Color.FromArgb(0,  145, 175),
            Color.FromArgb(155,  25,  45),
            Color.FromArgb(40, 120,  55),
            Color.FromArgb(115,  75,  15),
        };

        private User user;
        private FlowLayoutPanel flow;

        public CategorySelectForm(User u) { user = u; BuildUI(); LoadCards(); }

        private void BuildUI()
        {
            Text = "Quiz Master - Subject Chunein";
            Size = new Size(1050, 700); MinimumSize = new Size(850, 580);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true; BackColor = Cream;

            // Header
            var hdr = new Panel { Dock = DockStyle.Top, Height = 72 };
            hdr.Paint += (s, e) => {
                using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, hdr.ClientRectangle);
            };
            var lblIcon = new Label { Text = "📝", Font = new Font("Segoe UI Emoji", 17), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 20) };
            var lblT = new Label { Text = "Quiz Master", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(52, 22) };
            var lblHello = new Label { Text = $"Hello, {user.FullName}! 🧑‍💻", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(185, 235, 200), AutoSize = true, BackColor = Color.Transparent, Location = new Point(20, 48) };

            var btnRes = HdrBtn("📊 My Results", Color.FromArgb(20, 115, 60), new Point(800, 19), 145, 34);
            btnRes.Click += ShowHistory;
            var btnOut = HdrBtn("→ Logout", Color.FromArgb(185, 35, 35), new Point(958, 19), 75, 34);
            btnOut.Click += (s, e) => Close();

            hdr.Controls.AddRange(new Control[] { lblIcon, lblT, lblHello, btnRes, btnOut });
            hdr.Resize += (s, e) => { btnOut.Location = new Point(hdr.Width - 90, 19); btnRes.Location = new Point(hdr.Width - 245, 19); };

            // Instruction label
            var sub = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(232, 244, 232) };
            sub.Controls.Add(new Label { Text = "Select your subject from the options below:", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(20, 13) });

            // Cards FlowPanel — AutoScroll handles BOTH scrollbars automatically
            flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,          // shows vertical + horizontal scrollbars when needed
                BackColor = Cream,
                Padding = new Padding(18, 18, 18, 18),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            Controls.Add(flow);
            Controls.Add(sub);
            Controls.Add(hdr);
        }

        private void LoadCards()
        {
            flow.Controls.Clear();
            var cats = DatabaseManager.GetAllCategories(activeOnly: true);
            for (int i = 0; i < cats.Count; i++)
                flow.Controls.Add(MakeCard(cats[i], Accents[i % Accents.Length]));
        }

        private Panel MakeCard(QuizCategory cat, Color accent)
        {
            bool hasQ = cat.QuestionCount > 0;
            var card = new Panel { Size = new Size(300, 240), Margin = new Padding(10), BackColor = Color.White, Cursor = hasQ ? Cursors.Hand : Cursors.Default };

            card.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using var sh = new SolidBrush(Color.FromArgb(18, 0, 0, 0));
                g.FillRectangle(sh, new Rectangle(4, 4, card.Width - 2, card.Height - 2));
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, card.Width - 4, card.Height - 4));
                using var ab = new SolidBrush(accent);
                g.FillRectangle(ab, new Rectangle(0, 0, card.Width - 4, 7));
                using var bp = new Pen(Color.FromArgb(215, 225, 215), 1f);
                g.DrawRectangle(bp, new Rectangle(0, 0, card.Width - 5, card.Height - 5));
            };

            var badge = new Label { Text = $"{cat.QuestionCount} Questions", Font = new Font("Segoe UI", 8, FontStyle.Bold), BackColor = hasQ ? accent : Color.FromArgb(170, 140, 80), ForeColor = Color.White, AutoSize = false, Size = new Size(98, 22), Location = new Point(card.Width - 112, 14), TextAlign = ContentAlignment.MiddleCenter };
            var lblIco = new Label { Text = cat.Icon, Font = new Font("Segoe UI Emoji", 30), AutoSize = true, BackColor = Color.Transparent, ForeColor = accent, Location = new Point(14, 20) };
            var lblName = new Label { Text = cat.Name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(25, 35, 25), Size = new Size(270, 46), Location = new Point(12, 88) };
            var lblDesc = new Label { Text = cat.Description, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 115, 100), Size = new Size(270, 38), Location = new Point(12, 134) };

            Button btnStart;
            if (hasQ) {
                btnStart = new Button { Text = "START QUIZ ▶", Size = new Size(274, 38), Location = new Point(13, 188), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                btnStart.FlatAppearance.BorderSize = 0;
                btnStart.Click += (s, e) => StartQuiz(cat);
                card.Click += (s, e) => StartQuiz(cat);
            } else {
                btnStart = new Button { Text = "VIEW DETAILS ℹ", Size = new Size(274, 38), Location = new Point(13, 188), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(195, 168, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnStart.FlatAppearance.BorderSize = 0;
            }

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(246, 252, 246);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
            card.Controls.AddRange(new Control[] { badge, lblIco, lblName, lblDesc, btnStart });
            return card;
        }

        private void StartQuiz(QuizCategory cat)
        {
            Hide();
            var qf = new QuizForm(user, cat);
            qf.FormClosed += (s, e) => { Show(); LoadCards(); };
            qf.Show();
        }

        private void ShowHistory(object s, EventArgs e)
        {
            using var f = new HistoryForm(user.FullName, DatabaseManager.GetUserResults(user.Email));
            f.ShowDialog(this);
        }

        static Button HdrBtn(string t, Color bg, Point loc, int w, int h) {
            var b = new Button { Text = t, Size = new Size(w, h), Location = loc, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 1; b.FlatAppearance.BorderColor = Color.FromArgb(100, 200, 130); return b; }
    }

    public class HistoryForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);

        public HistoryForm(string name, DataTable dt)
        {
            Text = $"{name} - My Quiz History";
            Size = new Size(880, 580); MinimumSize = new Size(700, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true;
            BackColor = Color.FromArgb(238, 248, 238);

            var hdr = new Panel { Dock = DockStyle.Top, Height = 56 };
            hdr.Paint += (s, e) => {
                using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = "🏆  My Quiz History", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 16) });

            // DataGridView with both scrollbars
            var grid = new DataGridView {
                Dock = DockStyle.Fill,
                DataSource = dt,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 34 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,  // fit to content
                ScrollBars = ScrollBars.Both   // both H and V scrollbars
            };
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = DG, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(6, 0, 0, 0) };
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(238, 250, 238) };
            grid.DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = Color.FromArgb(175, 220, 175), SelectionForeColor = Color.FromArgb(15, 55, 15), Padding = new Padding(4, 0, 0, 0) };
            grid.CellFormatting += (s, e) => {
                if (e.ColumnIndex < 0 || e.Value == null) return;
                if (grid.Columns[e.ColumnIndex].Name == "Grade") {
                    string g = e.Value.ToString();
                    e.CellStyle.ForeColor = g == "F" ? Color.Red : g == "D" ? Color.OrangeRed : g == "C" ? Color.DarkOrange : Color.FromArgb(15, 110, 40);
                    e.CellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                }
            };

            Controls.Add(grid); Controls.Add(hdr);
        }
    }
}
