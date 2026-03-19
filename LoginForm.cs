using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuizApp
{
    public class LoginForm : Form
    {
        static readonly Color C1 = Color.FromArgb(15, 75, 40);
        static readonly Color C2 = Color.FromArgb(25, 105, 55);
        static readonly Color Gold = Color.FromArgb(185, 145, 55);
        static readonly Color LGold = Color.FromArgb(215, 175, 80);
        static readonly Color Cream = Color.FromArgb(252, 250, 244);

        private TextBox txtEmail, txtPassword;
        private CheckBox chkShow;
        private Button btnLogin, btnRegister;

        public LoginForm() { DatabaseManager.InitializeDatabase(); BuildUI(); }

        private void BuildUI()
        {
            Text = "Quiz Master"; Size = new Size(980, 640);
            MinimumSize = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true; BackColor = Cream;

            // ── LEFT panel ──────────────────────────────────────────
            var left = new Panel { Dock = DockStyle.Left, Width = 430, BackColor = C1 };
            left.Paint += (s, e) => PaintLeft(e.Graphics, left.Width, left.Height);

            // Leaf decoration panels (top-right corner of left)
            // Drawn in PaintLeft

            // Logo box (rounded gold square with pencil-doc icon)
            var logo = new Panel { Size = new Size(105, 105), BackColor = Color.Transparent };
            logo.Paint += PaintLogo;

            // Reposition logo on resize
            left.Resize += (s, e) => logo.Location = new Point((left.Width - 105) / 2, 140);
            logo.Location = new Point(163, 140);

            var lblApp = MkLabel("Quiz Master", "Georgia", 24, FontStyle.Bold, LGold);
            lblApp.BackColor = Color.Transparent;
            left.Resize += (s, e) => lblApp.Location = new Point((left.Width - lblApp.PreferredWidth) / 2, 262);
            lblApp.Location = new Point(125, 262);

            var lblSub = MkLabel("Test your knowledge with our\ninteractive quiz system", "Segoe UI", 10.5f, FontStyle.Regular, Color.FromArgb(185, 230, 200));
            lblSub.BackColor = Color.Transparent; lblSub.TextAlign = ContentAlignment.MiddleCenter;
            left.Resize += (s, e) => lblSub.Location = new Point((left.Width - 260) / 2, 310);
            lblSub.Location = new Point(85, 310); lblSub.Size = new Size(260, 48);

            left.Controls.AddRange(new Control[] { logo, lblApp, lblSub });

            // ── RIGHT panel ─────────────────────────────────────────
            var right = new Panel { Dock = DockStyle.Fill, BackColor = Cream, AutoScroll = true };

            var lblW = MkLabel("Welcome Back!", "Georgia", 26, FontStyle.Bold, Color.FromArgb(70, 45, 15));
            lblW.Location = new Point(70, 80);

            var lblSub2 = MkLabel("Login to start your quiz", "Segoe UI", 11, FontStyle.Regular, Color.FromArgb(140, 110, 60));
            lblSub2.Location = new Point(72, 122);

            // Email
            right.Controls.Add(MkLabel("Email Address", "Segoe UI", 10, FontStyle.Bold, Color.FromArgb(60, 40, 10)));
            right.Controls[right.Controls.Count - 1].Location = new Point(72, 172);
            txtEmail = MkTxt(new Point(72, 196), 420, "yourname@email.com");

            // Password
            right.Controls.Add(MkLabel("Password", "Segoe UI", 10, FontStyle.Bold, Color.FromArgb(60, 40, 10)));
            right.Controls[right.Controls.Count - 1].Location = new Point(72, 252);
            txtPassword = MkTxt(new Point(72, 276), 420, "");
            txtPassword.PasswordChar = '●';

            chkShow = new CheckBox { Text = "Show Password", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(100, 75, 30), AutoSize = true, Location = new Point(74, 325), BackColor = Color.Transparent };
            chkShow.CheckedChanged += (s, e) => txtPassword.PasswordChar = chkShow.Checked ? '\0' : '●';

            var lnk = new LinkLabel { Text = "Forgot Password?", Font = new Font("Segoe UI", 9.5f), AutoSize = true, Location = new Point(390, 327), LinkColor = Color.FromArgb(120, 90, 20) };
            lnk.Click += (s, e) => MessageBox.Show("Admin se contact karein.", "Forgot Password");

            btnLogin = MkBtn("LOGIN", C1, new Point(72, 368), 420, 50);
            btnLogin.Click += DoLogin;

            var lblOr = MkLabel("— OR —", "Segoe UI", 9, FontStyle.Regular, Color.FromArgb(160, 140, 90));
            lblOr.Location = new Point(218, 434);

            btnRegister = new Button { Text = "CREATE NEW ACCOUNT", Size = new Size(420, 50), Location = new Point(72, 458), Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Cream, ForeColor = C1, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRegister.FlatAppearance.BorderColor = C1; btnRegister.FlatAppearance.BorderSize = 2;
            btnRegister.Click += (s, e) => new RegisterForm().ShowDialog(this);

            right.Controls.AddRange(new Control[] { lblW, lblSub2, txtEmail, txtPassword, chkShow, lnk, btnLogin, lblOr, btnRegister });

            Controls.Add(right); Controls.Add(left);
            AcceptButton = btnLogin;

            Resize += (s, e) => {
                left.Width = Math.Min(430, ClientSize.Width * 43 / 100);
                logo.Location = new Point((left.Width - 105) / 2, 140);
                lblApp.Location = new Point((left.Width - lblApp.PreferredWidth) / 2, 262);
                lblSub.Location = new Point((left.Width - 260) / 2, 310);
            };
        }

        private static void PaintLeft(Graphics g, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new LinearGradientBrush(new Rectangle(0, 0, w, h), C1, C2, LinearGradientMode.Vertical);
            g.FillRectangle(br, 0, 0, w, h);
            // Decorative leaf circles top-right
            using var lp = new Pen(Color.FromArgb(35, 255, 255, 255), 1.5f);
            g.DrawEllipse(lp, w - 100, -50, 180, 180);
            g.DrawEllipse(lp, w - 60, -20, 120, 120);
            // Bottom left circle
            g.DrawEllipse(lp, -40, h - 140, 200, 200);
            g.DrawEllipse(lp, -20, h - 100, 130, 130);
            // Puzzle/leaf small decorations
            using var sp = new Pen(Color.FromArgb(20, 255, 255, 255), 1f);
            for (int i = 0; i < 6; i++) g.DrawEllipse(sp, 15 + i * 12, h - 60 + (i % 2) * 10, 30, 30);
        }

        private static void PaintLogo(object s, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(2, 2, 100, 100);
            using var gb = new LinearGradientBrush(r, LGold, Gold, 45f);
            FillRR(g, gb, r, 18);
            using var gp = new Pen(Color.FromArgb(215, 165, 40), 1.5f); DrawRR(g, gp, r, 18);
            // Document icon
            using var wb = new SolidBrush(Color.FromArgb(55, 35, 8));
            var docR = new Rectangle(22, 20, 45, 58);
            FillRR(g, wb, docR, 6);
            using var linePen = new Pen(LGold, 2.5f);
            g.DrawLine(linePen, 30, 35, 57, 35);
            g.DrawLine(linePen, 30, 44, 57, 44);
            g.DrawLine(linePen, 30, 53, 50, 53);
            // Pencil
            using var pencilBr = new SolidBrush(LGold);
            PointF[] pencil = { new PointF(52, 58), new PointF(72, 38), new PointF(78, 44), new PointF(58, 64) };
            g.FillPolygon(pencilBr, pencil);
            g.DrawPolygon(new Pen(Color.FromArgb(155, 115, 20), 1f), pencil);
            g.FillEllipse(new SolidBrush(Color.FromArgb(200, 150, 30)), 50, 62, 8, 8);
        }

        private void DoLogin(object s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            { MessageBox.Show("Email aur password dono bharein!", "Ghalti", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var user = DatabaseManager.AuthenticateUser(txtEmail.Text.Trim(), txtPassword.Text);
            if (user != null) {
                Hide();
                Form next = user.IsAdmin ? (Form)new AdminDashboard(user) : new CategorySelectForm(user);
                next.FormClosed += (s2, a) => Close(); next.Show();
            } else { MessageBox.Show("Email ya password galat hai!", "Login Fail", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPassword.Clear(); txtPassword.Focus(); }
        }

        // ── helpers ─────────────────────────────────────────────────
        static Label MkLabel(string t, string font, float sz, FontStyle fs, Color fc) =>
            new Label { Text = t, Font = new Font(font, sz, fs), ForeColor = fc, AutoSize = true, BackColor = Color.Transparent };
        static TextBox MkTxt(Point loc, int w, string ph) =>
            new TextBox { Size = new Size(w, 38), Location = loc, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, PlaceholderText = ph };
        static Button MkBtn(string t, Color bg, Point loc, int w, int h) {
            var b = new Button { Text = t, Size = new Size(w, h), Location = loc, Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(25, 105, 55);
            b.MouseLeave += (s, e) => b.BackColor = bg; return b; }
        static void FillRR(Graphics g, Brush b, Rectangle r, int rad) { using var p = RRP(r, rad); g.FillPath(b, p); }
        static void DrawRR(Graphics g, Pen p, Rectangle r, int rad) { using var path = RRP(r, rad); g.DrawPath(p, path); }
        static GraphicsPath RRP(Rectangle r, int rad) {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90); p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90); p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure(); return p; }
    }
}
