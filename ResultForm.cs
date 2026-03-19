using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuizApp
{
    public class ResultForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);

        private User user; private QuizCategory cat;
        private int score, total; private List<Question> qs; private int[] ans;

        public ResultForm(User u, QuizCategory c, int sc, int tot, List<Question> q, int[] a)
        { user = u; cat = c; score = sc; total = tot; qs = q; ans = a; BuildUI(); }

        private void BuildUI()
        {
            double pct = (double)score / total * 100;
            string grade = pct>=90?"A+":pct>=80?"A":pct>=70?"B":pct>=60?"C":pct>=50?"D":"F";
            Color gc = pct>=70?Color.FromArgb(15,115,45):pct>=50?Color.FromArgb(170,125,0):Color.FromArgb(175,30,30);
            string emoji = pct>=90?"🏆":pct>=70?"🎉":pct>=50?"😊":"😔";

            Text = $"Result - {cat.Name}"; Size = new Size(860, 740);
            MinimumSize = new Size(720, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true;
            BackColor = Color.FromArgb(244, 250, 244);
            AutoScroll = false;

            // ── Header ──────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 155 };
            hdr.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Vertical);
                g.FillRectangle(br, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = emoji, Font = new Font("Segoe UI Emoji", 28), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(365, 12) });
            hdr.Controls.Add(new Label { Text = "QUIZ COMPLETE!", Font = new Font("Georgia", 18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(270, 68) });
            hdr.Controls.Add(new Label { Text = $"{cat.Icon}  {cat.Name}", Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(185, 240, 195), AutoSize = true, BackColor = Color.Transparent, Location = new Point(295, 110) });
            hdr.Controls.Add(new Label { Text = $"👤 {user.FullName}  |  {user.Email}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(165, 215, 175), AutoSize = true, BackColor = Color.Transparent, Location = new Point(270, 134) });
            hdr.Resize += (s, e) => {
                foreach (Control c in hdr.Controls)
                    if (c is Label lbl && lbl.AutoSize)
                        lbl.Location = new Point((hdr.Width - lbl.PreferredWidth) / 2, lbl.Location.Y);
            };

            // ── Score Cards ──────────────────────────────────────────
            var cardsPanel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(240, 250, 240) };
            cardsPanel.Controls.Add(SCard("SCORE", $"{score}/{total}", gc, 15, 9, 162, 72));
            cardsPanel.Controls.Add(SCard("GRADE", grade, gc, 185, 9, 122, 72));
            cardsPanel.Controls.Add(SCard("PERCENTAGE", $"{pct:F1}%", gc, 315, 9, 162, 72));
            cardsPanel.Controls.Add(SCard("STATUS", pct >= 50 ? "PASS ✅" : "FAIL ❌", gc, 485, 9, 175, 72));
            cardsPanel.Resize += (s, e) => {
                int w = Math.Max(100, (cardsPanel.Width - 60) / 4);
                for (int i = 0; i < cardsPanel.Controls.Count; i++)
                    if (cardsPanel.Controls[i] is Panel sc) { sc.Width = w; sc.Location = new Point(15 + i * (w + 12), 9); }
            };

            // ── Review Header ────────────────────────────────────────
            var revH = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(228, 244, 228) };
            revH.Controls.Add(new Label { Text = "Answer Review:", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(14, 9) });

            // ── Review scroll panel — BOTH scrollbars ────────────────
            var revScroll = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,          // vertical + horizontal as needed
                BackColor = Color.White
            };

            int yp = 10;
            for (int i = 0; i < qs.Count; i++) {
                bool correct = ans[i] == qs[i].CorrectIndex;
                bool answered = ans[i] != -1;
                Color rowBg = correct ? Color.FromArgb(238, 255, 238) : Color.FromArgb(255, 238, 238);
                string icon2 = correct ? "✅" : answered ? "❌" : "⬜";

                var row = new Panel { Size = new Size(800, 48), Location = new Point(10, yp), BackColor = rowBg };
                row.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(215, 228, 215)), new Rectangle(0, 0, row.Width - 1, row.Height - 1));

                string qTxt = qs[i].Text.Length > 65 ? qs[i].Text.Substring(0, 65) + "..." : qs[i].Text;
                row.Controls.Add(new Label { Text = $"{icon2}  Q{i + 1}. {qTxt}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 40, 20), Size = new Size(505, 44), Location = new Point(8, 4), TextAlign = ContentAlignment.MiddleLeft });

                string ca = qs[i].Options[qs[i].CorrectIndex];
                string caShort = ca.Length > 35 ? ca.Substring(0, 35) + "..." : ca;
                row.Controls.Add(new Label { Text = $"✓ {caShort}", Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.FromArgb(10, 115, 45), Size = new Size(260, 44), Location = new Point(518, 4), TextAlign = ContentAlignment.MiddleLeft });

                revScroll.Controls.Add(row);
                yp += 55;
            }

            // Keep rows full width on resize
            revScroll.Resize += (s, e) => {
                int rw = Math.Max(600, revScroll.ClientSize.Width - 20);
                foreach (Control c in revScroll.Controls)
                    if (c is Panel rr) rr.Width = rw;
            };

            // ── Footer ───────────────────────────────────────────────
            var ftr = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(232, 244, 232) };
            ftr.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(185, 215, 185)), 0, 0, ftr.Width, 0);

            var bRet = FB($"🔄 Retry", DG, 15, 12, 140);
            bRet.Click += (s, e) => { new QuizForm(user, cat).Show(); Close(); };
            var bBk = FB("📚 Change Subject", Color.FromArgb(28, 68, 148), 163, 12, 175);
            bBk.Click += (s, e) => Close();
            var bHist = FB("📊 My History", MG, 346, 12, 145);
            bHist.Click += (s, e) => { using var hf = new HistoryForm(user.FullName, DatabaseManager.GetUserResults(user.Email)); hf.ShowDialog(this); };
            var bLO = FB("→ Logout", Color.FromArgb(175, 30, 30), 700, 12, 110);
            bLO.Click += (s, e) => Application.Exit();

            ftr.Controls.AddRange(new Control[] { bRet, bBk, bHist, bLO });
            ftr.Resize += (s, e) => bLO.Location = new Point(ftr.Width - 125, 12);

            // Add in correct dock order
            Controls.Add(revScroll);   // Fill
            Controls.Add(revH);        // Top
            Controls.Add(cardsPanel);  // Top
            Controls.Add(hdr);         // Top
            Controls.Add(ftr);         // Bottom
        }

        private Panel SCard(string title, string val, Color c, int x, int y, int w, int h)
        {
            var p = new Panel { Size = new Size(w, h), Location = new Point(x, y), BackColor = Color.White };
            p.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(205, 228, 205)), new Rectangle(0, 0, p.Width - 1, p.Height - 1));
                e.Graphics.FillRectangle(new SolidBrush(c), new Rectangle(0, 0, p.Width, 4));
            };
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(75, 100, 75), Size = new Size(w - 6, 22), Location = new Point(3, 8), TextAlign = ContentAlignment.MiddleCenter });
            p.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = c, Size = new Size(w - 6, 35), Location = new Point(3, 30), TextAlign = ContentAlignment.MiddleCenter });
            return p;
        }

        private Button FB(string t, Color c, int x, int y, int w)
        {
            var b = new Button { Text = t, Size = new Size(w, 36), Location = new Point(x, y), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b;
        }
    }
}
