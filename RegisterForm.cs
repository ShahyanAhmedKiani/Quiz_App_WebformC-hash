using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuizApp
{
    public class RegisterForm : Form
    {
        static readonly Color C1 = Color.FromArgb(15, 75, 40);
        static readonly Color C2 = Color.FromArgb(25, 105, 55);
        static readonly Color Gold = Color.FromArgb(185, 145, 55);
        static readonly Color LGold = Color.FromArgb(215, 175, 80);
        static readonly Color Cream = Color.FromArgb(252, 250, 244);

        private TextBox txtName, txtEmail, txtPass, txtConfirm;

        public RegisterForm() { BuildUI(); }

        private void BuildUI()
        {
            Text = "Quiz Master - Create Account";
            Size = new Size(980, 700); MinimumSize = new Size(820, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true; BackColor = Cream;

            // Left panel (same green with leaf decoration + logo)
            var left = new Panel { Dock = DockStyle.Left, Width = 430, BackColor = C1 };
            left.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(left.ClientRectangle, C1, C2, LinearGradientMode.Vertical);
                g.FillRectangle(br, left.ClientRectangle);
                // Leaf branch top-right
                using var lp = new Pen(Color.FromArgb(40, 255, 255, 255), 1.5f);
                g.DrawEllipse(lp, left.Width - 110, -60, 200, 200);
                g.DrawEllipse(lp, left.Width - 75, -30, 140, 140);
                g.DrawEllipse(lp, -50, left.Height - 150, 220, 220);
            };

            var logo = new Panel { Size = new Size(105, 105), BackColor = Color.Transparent, Location = new Point(163, 120) };
            logo.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(2, 2, 100, 100);
                using var gb = new LinearGradientBrush(r, LGold, Gold, 45f);
                FillRR(g, gb, r, 18);
                using var wb = new SolidBrush(Color.FromArgb(55, 35, 8));
                var docR = new Rectangle(22, 20, 45, 58); FillRR(g, wb, docR, 6);
                using var lnp = new Pen(LGold, 2.5f);
                g.DrawLine(lnp, 30, 35, 57, 35); g.DrawLine(lnp, 30, 44, 57, 44); g.DrawLine(lnp, 30, 53, 50, 53);
                PointF[] penc = { new PointF(52, 58), new PointF(72, 38), new PointF(78, 44), new PointF(58, 64) };
                g.FillPolygon(new SolidBrush(LGold), penc);
                g.FillEllipse(new SolidBrush(Color.FromArgb(200, 150, 30)), 50, 62, 8, 8);
            };

            var lblApp = new Label { Text = "Quiz Master", Font = new Font("Georgia", 22, FontStyle.Bold), ForeColor = LGold, AutoSize = true, BackColor = Color.Transparent, Location = new Point(110, 238) };
            var lblSub = new Label { Text = "Test your knowledge with our\ninteractive quiz system", Font = new Font("Segoe UI", 10.5f), ForeColor = Color.FromArgb(185, 230, 200), Size = new Size(260, 50), BackColor = Color.Transparent, Location = new Point(85, 282), TextAlign = ContentAlignment.MiddleCenter };
            left.Controls.AddRange(new Control[] { logo, lblApp, lblSub });

            // Right panel - the form
            var right = new Panel { Dock = DockStyle.Fill, BackColor = Cream, AutoScroll = true };

            var lblTitle = new Label { Text = "Create Your Account", Font = new Font("Georgia", 22, FontStyle.Bold), ForeColor = Color.FromArgb(140, 100, 20), AutoSize = true, Location = new Point(60, 40) };

            // 4 fields
            (string lbl, string ph, bool isPass, int y)[] fields = {
                ("Full name",         "Enter your Full name",       false, 95),
                ("Email Address",     "e.g. yourname@email.com",    false, 185),
                ("Password",          "Enter your password",         true,  275),
                ("Confirm password",  "Enter your password",         true,  365),
            };

            var boxes = new TextBox[4];
            for (int i = 0; i < 4; i++) {
                right.Controls.Add(new Label { Text = fields[i].lbl, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(50, 40, 10), AutoSize = true, Location = new Point(60, fields[i].y) });
                boxes[i] = new TextBox { Size = new Size(420, 40), Location = new Point(60, fields[i].y + 26), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 245, 235), PlaceholderText = fields[i].ph };
                if (fields[i].isPass) boxes[i].PasswordChar = '●';
                // Rounded border illusion
                right.Controls.Add(boxes[i]);
            }
            txtName = boxes[0]; txtEmail = boxes[1]; txtPass = boxes[2]; txtConfirm = boxes[3];

            var chkRem = new CheckBox { Text = "Remember Me", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(80, 60, 20), AutoSize = true, Location = new Point(62, 468), BackColor = Color.Transparent };

            var btnSI = MkBtn("SIGN IN", C1, new Point(62, 510), 195, 50);
            btnSI.Click += DoRegister;

            var btnCnl = MkBtn("CANCEL", Gold, new Point(268, 510), 195, 50);
            btnCnl.Click += (s, e) => Close();

            right.Controls.AddRange(new Control[] { lblTitle, chkRem, btnSI, btnCnl });
            Controls.Add(right); Controls.Add(left);
        }

        private void DoRegister(object s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
            { MessageBox.Show("Sab fields bharein!"); return; }
            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { MessageBox.Show("Sahi email likhein!"); return; }
            if (txtPass.Text.Length < 6) { MessageBox.Show("Password 6+ chars ka hona chahiye!"); return; }
            if (txtPass.Text != txtConfirm.Text) { MessageBox.Show("Passwords match nahi!"); return; }
            if (DatabaseManager.RegisterUser(txtEmail.Text.Trim(), txtPass.Text, txtName.Text.Trim()))
            { MessageBox.Show("Account ban gaya! Ab login karein.", "Mubarak Ho!"); Close(); }
            else MessageBox.Show("Yeh email pehle se registered hai!");
        }

        static Button MkBtn(string t, Color bg, Point loc, int w, int h) {
            var b = new Button { Text = t, Size = new Size(w, h), Location = loc, Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b; }
        static void FillRR(Graphics g, Brush br, Rectangle r, int rad) { using var p = RRP(r, rad); g.FillPath(br, p); }
        static GraphicsPath RRP(Rectangle r, int rad) {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90); p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90); p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure(); return p; }
    }
}
