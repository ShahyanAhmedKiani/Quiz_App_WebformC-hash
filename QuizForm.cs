using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuizApp
{
    public class QuizForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);

        private User user; private QuizCategory cat;
        private List<Question> questions; private int[] answers;
        private int idx = 0; private System.Windows.Forms.Timer timer;
        private int timeLeft = 600;

        private Label lblQ, lblNum, lblTimer;
        private RadioButton[] rbs = new RadioButton[4];
        private Panel[] optRows = new Panel[4];
        private Button btnPrev, btnNext, btnSub;
        private ProgressBar pgBar;
        private Panel optCont; // scrollable options container

        public QuizForm(User u, QuizCategory c)
        {
            user = u; cat = c;
            questions = QuizData.GetQuestions(c.Id);
            if (questions.Count == 0) { MessageBox.Show("Koi sawal nahi!"); Load += (s, e) => Close(); return; }
            answers = new int[questions.Count]; for (int i = 0; i < answers.Length; i++) answers[i] = -1;
            BuildUI(); LoadQ(0); StartTimer();
        }

        private void BuildUI()
        {
            Text = $"Quiz: {cat.Name}"; Size = new Size(900, 720); MinimumSize = new Size(740, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true;
            BackColor = Color.FromArgb(244, 250, 244);
            AutoScroll = false; // form itself no scroll — inner panels handle it

            // ── Header ──────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 70 };
            hdr.Paint += (s, e) => {
                using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = $"👤 {user.FullName}  |  {user.Email}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(185, 235, 200), AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 10) });
            hdr.Controls.Add(new Label { Text = $"{cat.Icon}  {cat.Name}", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 35) });
            lblTimer = new Label { Text = "⏱ 10:00", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(155, 240, 155), AutoSize = true, BackColor = Color.Transparent, Location = new Point(740, 24) };
            hdr.Controls.Add(lblTimer);
            hdr.Resize += (s, e) => lblTimer.Location = new Point(hdr.Width - 130, 24);

            // ── Progress strip ───────────────────────────────────────
            var pgStrip = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(232, 244, 232) };
            lblNum = new Label { Text = $"Question 1 / {questions.Count}", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(14, 10) };
            pgBar = new ProgressBar { Size = new Size(560, 12), Location = new Point(200, 11), Minimum = 0, Maximum = questions.Count, Value = 1, Style = ProgressBarStyle.Continuous };
            pgStrip.Controls.AddRange(new Control[] { lblNum, pgBar });
            pgStrip.Resize += (s, e) => pgBar.Width = Math.Max(100, pgStrip.Width - 260);

            // ── Question card ────────────────────────────────────────
            var qCard = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White };
            qCard.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(195, 220, 195)), 0, qCard.Height - 1, qCard.Width, qCard.Height - 1);
            lblQ = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12.5f), ForeColor = Color.FromArgb(20, 40, 20), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(18, 8, 18, 8) };
            qCard.Controls.Add(lblQ);

            // ── Footer ───────────────────────────────────────────────
            var ftr = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(232, 244, 232) };
            ftr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(185, 215, 185)), 0, 0, ftr.Width, 0);
            btnPrev = FB("◀ Previous", Color.FromArgb(90, 110, 90), 15, 12, 130);
            btnPrev.Enabled = false;
            btnPrev.Click += (s, e) => { if (idx > 0) { idx--; LoadQ(idx); } };
            btnNext = FB("Next ▶", DG, 155, 12, 120);
            btnNext.Click += (s, e) => { if (idx < questions.Count - 1) { idx++; LoadQ(idx); } };
            btnSub = FB("✓ Submit Quiz", Color.FromArgb(15, 125, 60), 680, 12, 165);
            btnSub.Visible = false;
            btnSub.Click += DoSubmit;
            ftr.Controls.AddRange(new Control[] { btnPrev, btnNext, btnSub });
            ftr.Resize += (s, e) => btnSub.Location = new Point(ftr.Width - 180, 12);

            // ── Options — scrollable panel ───────────────────────────
            // ScrollablePanel with both H and V scrollbars
            optCont = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 250, 244),
                AutoScroll = true,   // shows scrollbars when content overflows
                Padding = new Padding(18, 14, 18, 14)
            };

            Color[] bgs = { Color.FromArgb(232,242,255), Color.FromArgb(232,252,242), Color.FromArgb(255,246,230), Color.FromArgb(255,232,238) };
            Color[] acc = { Color.FromArgb(28,68,148), Color.FromArgb(10,120,65), Color.FromArgb(165,105,10), Color.FromArgb(145,20,45) };
            string[] lls = { "A","B","C","D" };

            for (int i = 0; i < 4; i++) {
                int ii = i;
                optRows[i] = new Panel {
                    Size = new Size(820, 80),
                    Location = new Point(0, i * 90),
                    BackColor = bgs[i],
                    Cursor = Cursors.Hand
                };
                optRows[i].Paint += (s, e) => {
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(195, 212, 195)),
                        new Rectangle(0, 0, optRows[ii].Width - 1, optRows[ii].Height - 1));
                };

                var lLet = new Label { Text = lls[i], Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = acc[i], Size = new Size(44, 74), Location = new Point(12, 4), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };

                rbs[i] = new RadioButton {
                    Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(20, 40, 20),
                    Size = new Size(730, 70), Location = new Point(60, 5),
                    BackColor = Color.Transparent, Tag = ii
                };
                rbs[i].Click += (s, e) => SelOpt(ii);
                optRows[i].Click += (s, e) => SelOpt(ii);
                lLet.Click += (s, e) => SelOpt(ii);

                optRows[i].Controls.AddRange(new Control[] { lLet, rbs[i] });
                optCont.Controls.Add(optRows[i]);
            }

            // Resize: keep option rows full width
            optCont.Resize += (s, e) => {
                int newW = Math.Max(300, optCont.ClientSize.Width - 36);
                for (int i = 0; i < 4; i++) {
                    if (optRows[i] != null) {
                        optRows[i].Width = newW;
                        if (rbs[i] != null) rbs[i].Width = newW - 66;
                    }
                }
            };

            // Add in correct dock order
            Controls.Add(optCont);   // Fill — must be added first
            Controls.Add(qCard);     // Top
            Controls.Add(pgStrip);   // Top
            Controls.Add(hdr);       // Top
            Controls.Add(ftr);       // Bottom
        }

        private void SelOpt(int i)
        {
            answers[idx] = i;
            for (int j = 0; j < 4; j++) rbs[j].Checked = (j == i);
        }

        private void LoadQ(int i)
        {
            var q = questions[i];
            lblNum.Text = $"Question {i + 1} / {questions.Count}";
            lblQ.Text = q.Text;
            pgBar.Value = i + 1;
            for (int j = 0; j < 4; j++) {
                rbs[j].Text = "  " + q.Options[j];
                rbs[j].Checked = (answers[i] == j);
            }
            btnPrev.Enabled = i > 0;
            btnNext.Visible = i < questions.Count - 1;
            btnSub.Visible = i == questions.Count - 1;
            // Scroll back to top on question change
            optCont.AutoScrollPosition = new Point(0, 0);
        }

        private void DoSubmit(object s, EventArgs e)
        {
            int un = 0; foreach (int a in answers) if (a == -1) un++;
            if (un > 0 && MessageBox.Show($"{un} sawal ka jawab nahi. Submit karein?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No) return;
            Submit();
        }

        private void Submit()
        {
            timer?.Stop();
            int sc = 0;
            for (int i = 0; i < questions.Count; i++) if (answers[i] == questions[i].CorrectIndex) sc++;
            DatabaseManager.SaveResult(user.Email, user.FullName, cat.Id, cat.Name, sc, questions.Count);
            var rf = new ResultForm(user, cat, sc, questions.Count, questions, answers);
            rf.FormClosed += (s2, a) => Close();
            Hide(); rf.Show();
        }

        private void StartTimer()
        {
            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) => {
                timeLeft--;
                lblTimer.Text = $"⏱ {timeLeft / 60:00}:{timeLeft % 60:00}";
                if (timeLeft <= 60) lblTimer.ForeColor = Color.FromArgb(255, 100, 80);
                if (timeLeft <= 0) { timer.Stop(); MessageBox.Show("Waqt khatam!"); Submit(); }
            };
            timer.Start();
        }

        private Button FB(string t, Color c, int x, int y, int w)
        {
            var b = new Button { Text = t, Size = new Size(w, 36), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b;
        }
    }
}
