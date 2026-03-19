using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QuizApp
{
    public class AdminDashboard : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);
        static readonly Color Cream = Color.FromArgb(248, 252, 248);

        private User admin;
        private DataGridView dgvAll, dgvSummary;
        private Label lblAtt, lblStu, lblAvg, lblPass;
        private TextBox txtSearch;
        private ComboBox cmbSub;
        // Tab underline tracking
        private int activeTab = 0;
        private Panel[] tabBtns = new Panel[4];
        private Panel contentArea;

        public AdminDashboard(User a) { admin = a; BuildUI(); LoadData(); }

        private void BuildUI()
        {
            Text = "Admin Dashboard - Quiz Master";
            Size = new Size(1120, 740); MinimumSize = new Size(920, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true; BackColor = Cream;

            // ── Header ──────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 64 };
            hdr.Paint += (s, e) => {
                using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, hdr.ClientRectangle);
            };
            var lblCrown = new Label { Text = "♛", Font = new Font("Segoe UI", 18), ForeColor = Color.FromArgb(210, 170, 55), AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 18) };
            var lblT = new Label { Text = "Admin Dashboard - Quiz Master", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(50, 19) };
            var lblEM = new Label { Text = $"Logged in: {admin.Email}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(185, 235, 200), AutoSize = true, BackColor = Color.Transparent };
            var btnLO = new Button { Text = "→ LOGOUT", Size = new Size(110, 34), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(185, 35, 35), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLO.FlatAppearance.BorderSize = 0; btnLO.Click += (s, e) => Close();
            hdr.Controls.AddRange(new Control[] { lblCrown, lblT, lblEM, btnLO });
            hdr.Resize += (s, e) => { btnLO.Location = new Point(hdr.Width - 125, 15); lblEM.Location = new Point(hdr.Width - 280, 23); };
            lblEM.Location = new Point(850, 23); btnLO.Location = new Point(990, 15);

            // ── Stat cards row ───────────────────────────────────────
            var statsBar = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Cream };
            statsBar.Controls.Add(StatCard("📊 Total Attempts", "0", Color.FromArgb(28, 75, 155), Color.FromArgb(230, 238, 252), 15, out lblAtt));
            statsBar.Controls.Add(StatCard("👥 Active Students", "0", Color.FromArgb(15, 120, 65), Color.FromArgb(228, 248, 235), 290, out lblStu));
            statsBar.Controls.Add(StatCard("A  Average Score", "0%", Color.FromArgb(175, 125, 0), Color.FromArgb(252, 244, 220), 565, out lblAvg));
            statsBar.Controls.Add(StatCard("✓  Pass Rate", "0%", Color.FromArgb(0, 135, 155), Color.FromArgb(220, 246, 250), 840, out lblPass));
            statsBar.Resize += (s, e) => ResizeStatCards(statsBar);

            // ── Tab strip ───────────────────────────────────────────
            var tabStrip = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };
            tabStrip.Paint += (s, e) => {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(200, 220, 200)), 0, tabStrip.Height - 1, tabStrip.Width, tabStrip.Height - 1);
            };
            string[] tabNames = { "All Results", "Subject-wise Summary", "Category Management", "Question Management" };
            for (int i = 0; i < 4; i++) {
                int idx = i;
                var tp = new Panel { Size = new Size(190, 46), Location = new Point(i * 194 + 8, 0), BackColor = Color.White, Cursor = Cursors.Hand, Tag = idx };
                int ti = i;
                tp.Paint += (s, e) => {
                    var g = e.Graphics;
                    bool sel = activeTab == ti;
                    var lbl = tabNames[ti];
                    using var f = new Font("Segoe UI", 10, sel ? FontStyle.Bold : FontStyle.Regular);
                    using var b = new SolidBrush(sel ? DG : Color.FromArgb(90, 100, 90));
                    var tsz = g.MeasureString(lbl, f);
                    g.DrawString(lbl, f, b, (tp.Width - tsz.Width) / 2, (tp.Height - tsz.Height) / 2 - 2);
                    if (sel) g.FillRectangle(new SolidBrush(DG), 0, tp.Height - 3, tp.Width, 3);
                };
                tp.Click += (s, e) => { activeTab = idx; tabStrip.Invalidate(); foreach (var tb in tabBtns) tb?.Invalidate(); SwitchTab(idx); };
                tabBtns[i] = tp;
                tabStrip.Controls.Add(tp);
            }

            // ── Content area ─────────────────────────────────────────
            contentArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = false };

            Controls.Add(contentArea); Controls.Add(tabStrip); Controls.Add(statsBar); Controls.Add(hdr);
            SwitchTab(0);
        }

        private void SwitchTab(int idx)
        {
            contentArea.Controls.Clear();
            switch (idx) {
                case 0: contentArea.Controls.Add(BuildTab0()); break;
                case 1: contentArea.Controls.Add(BuildTab1()); break;
                case 2: contentArea.Controls.Add(BuildTab2()); break;
                case 3: contentArea.Controls.Add(BuildTab3()); break;
            }
        }

        private Panel BuildTab0()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            // Toolbar
            var tb = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(238, 248, 238) };
            tb.Controls.Add(new Label { Text = "Search:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(12, 16) });
            txtSearch = new TextBox { Size = new Size(200, 28), Location = new Point(72, 12), Font = new Font("Segoe UI", 10), PlaceholderText = "Enter Name or Email..." };
            txtSearch.TextChanged += (s, e) => FilterResults();
            tb.Controls.Add(new Label { Text = "Subject:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(288, 16) });
            cmbSub = new ComboBox { Size = new Size(210, 28), Location = new Point(350, 12), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSub.Items.Add("All Subjects");
            foreach (var c in DatabaseManager.GetAllCategories()) cmbSub.Items.Add(c.Name);
            cmbSub.SelectedIndex = 0; cmbSub.SelectedIndexChanged += (s, e) => FilterResults();
            var bRef = GB("↺ Refresh", Color.FromArgb(15, 115, 60), 575, 10, 110); bRef.Click += (s, e) => LoadData();
            var bExp = GB("⬇ Export CSV", Color.FromArgb(15, 115, 60), 695, 10, 125); bExp.Click += ExportCSV;
            tb.Controls.AddRange(new Control[] { txtSearch, cmbSub, bRef, bExp });
            // Grid
            dgvAll = BuildDGV(); dgvAll.Dock = DockStyle.Fill;
            dgvAll.CellFormatting += FormatGrade;
            p.Controls.Add(dgvAll); p.Controls.Add(tb);
            dgvAll.DataSource = DatabaseManager.GetAllResults();
            return p;
        }

        private Panel BuildTab1()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            dgvSummary = BuildDGV(); dgvSummary.Dock = DockStyle.Fill;
            dgvSummary.DataSource = DatabaseManager.GetResultsByCategory();
            p.Controls.Add(dgvSummary); return p;
        }

        private Panel BuildTab2()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var dgvC = BuildDGV(); dgvC.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvC.Location = new Point(0, 48); dgvC.Size = new Size(contentArea.Width, contentArea.Height - 48);
            dgvC.CellFormatting += (s, e) => {
                if (e.ColumnIndex < 0 || e.Value == null) return;
                if (dgvC.Columns[e.ColumnIndex].Name == "Status") {
                    e.CellStyle.ForeColor = e.Value.ToString().Contains("Active") ? Color.FromArgb(15, 110, 40) : Color.Red;
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            };
            Action refresh = () => {
                var dt = new DataTable();
                dt.Columns.Add("Id"); dt.Columns.Add("Icon"); dt.Columns.Add("Category Name");
                dt.Columns.Add("Description"); dt.Columns.Add("Active Questions"); dt.Columns.Add("Status");
                foreach (var c in DatabaseManager.GetAllCategories())
                    dt.Rows.Add(c.Id, c.Icon, c.Name, c.Description, c.QuestionCount, c.IsActive ? "✅ Active" : "❌ Band");
                dgvC.DataSource = dt;
                if (dgvC.Columns.Count > 0) { if (dgvC.Columns.Contains("Id")) dgvC.Columns["Id"].Width = 45; if (dgvC.Columns.Contains("Icon")) dgvC.Columns["Icon"].Width = 55; }
                RefreshSubDropdown();
            };
            var tb = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.FromArgb(238, 248, 238) };
            var bA = GB("+ New Category", Color.FromArgb(15, 115, 60), 8, 8, 148);
            bA.Click += (s, e) => { using var f = new AddEditCategoryForm(); if (f.ShowDialog() == DialogResult.OK && DatabaseManager.AddCategory(f.Result)) refresh(); };
            var bE = GB("✎ Edit", Color.FromArgb(28, 68, 148), 164, 8, 86);
            bE.Click += (s, e) => { if (dgvC.SelectedRows.Count == 0) return; int id = Convert.ToInt32(dgvC.SelectedRows[0].Cells["Id"].Value); var cat = DatabaseManager.GetAllCategories().Find(c => c.Id == id); using var f = new AddEditCategoryForm(cat); if (f.ShowDialog() == DialogResult.OK && DatabaseManager.UpdateCategory(f.Result)) refresh(); };
            var bT = GB("⇄ Active/Band", Color.FromArgb(175, 118, 0), 258, 8, 125);
            bT.Click += (s, e) => { if (dgvC.SelectedRows.Count == 0) return; DatabaseManager.ToggleCategoryStatus(Convert.ToInt32(dgvC.SelectedRows[0].Cells["Id"].Value)); refresh(); };
            var bD = GB("🗑 Delete", Color.FromArgb(175, 28, 28), 391, 8, 88);
            bD.Click += (s, e) => {
                if (dgvC.SelectedRows.Count == 0) return;
                int id = Convert.ToInt32(dgvC.SelectedRows[0].Cells["Id"].Value);
                string nm = dgvC.SelectedRows[0].Cells["Category Name"].Value.ToString();
                if (MessageBox.Show($"'{nm}' aur uske tamam sawal delete honge!", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    if (DatabaseManager.DeleteCategory(id)) refresh();
            };
            tb.Controls.AddRange(new Control[] { bA, bE, bT, bD });
            p.Controls.Add(dgvC); p.Controls.Add(tb);
            refresh(); return p;
        }

        private Panel BuildTab3()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var btn = new Button { Text = "❓  Open Question Manager", Size = new Size(320, 52), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.FromArgb(65, 38, 128), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { using var qf = new QuestionManagerForm(); qf.ShowDialog(this); };
            p.Controls.Add(btn);
            p.Controls.Add(new Label { Text = "Click here to add, edit, or delete questions.\nBoth categories and questions are managed from the admin side.", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true });
            p.Resize += (s, e) => {
                btn.Location = new Point((p.Width - btn.Width) / 2, p.Height / 2 - 60);
                if (p.Controls.Count > 1) p.Controls[1].Location = new Point((p.Width - p.Controls[1].Width) / 2, p.Height / 2 + 8);
            };
            return p;
        }

        private Panel StatCard(string title, string val, Color accent, Color bg, int x, out Label valLbl)
        {
            var card = new Panel { Size = new Size(255, 76), Location = new Point(x, 10), BackColor = bg };
            card.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillRectangle(new SolidBrush(bg), new Rectangle(0, 0, card.Width, card.Height));
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(60, accent)), new Rectangle(0, 0, card.Width, 4));
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(200, accent.R, accent.G, accent.B), 1), new Rectangle(0, 0, card.Width - 1, card.Height - 1));
            };
            var lt = new Label { Text = title, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(accent.R / 2, accent.G / 2, accent.B / 2), AutoSize = true, Location = new Point(12, 9), BackColor = Color.Transparent };
            valLbl = new Label { Text = val, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = accent, AutoSize = true, Location = new Point(10, 30), BackColor = Color.Transparent };
            card.Controls.AddRange(new Control[] { lt, valLbl }); return card;
        }

        private void ResizeStatCards(Panel bar)
        {
            int w = (bar.Width - 60) / 4;
            for (int i = 0; i < bar.Controls.Count; i++)
                if (bar.Controls[i] is Panel sc) { sc.Width = w; sc.Location = new Point(15 + i * (w + 15), 10); }
        }

        private DataGridView BuildDGV()
        {
            var g = new DataGridView { ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(205, 225, 205), Font = new Font("Segoe UI", 10), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells, ColumnHeadersHeight = 38, ScrollBars = ScrollBars.Both };
            g.RowTemplate.Height = 32;
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = DG, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(6, 0, 0, 0) };
            g.EnableHeadersVisualStyles = false;
            g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(238, 250, 238) };
            g.DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = Color.FromArgb(175, 220, 175), SelectionForeColor = Color.FromArgb(15, 50, 15), Padding = new Padding(4, 0, 0, 0) };
            return g;
        }

        private void FormatGrade(object s, DataGridViewCellFormattingEventArgs e)
        {
            var g = (DataGridView)s;
            if (e.ColumnIndex < 0 || e.Value == null || g.Columns.Count == 0) return;
            if (g.Columns[e.ColumnIndex].Name == "Grade") {
                string gr = e.Value.ToString();
                e.CellStyle.ForeColor = gr == "F" ? Color.Red : gr == "D" ? Color.OrangeRed : gr == "C" ? Color.DarkOrange : Color.FromArgb(15, 115, 45);
                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private Button GB(string t, Color c, int x, int y, int w) {
            var b = new Button { Text = t, Size = new Size(w, 30), Location = new Point(x, y), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b; }

        private void LoadData()
        {
            var dt = DatabaseManager.GetAllResults();
            if (dgvAll != null) dgvAll.DataSource = dt;
            if (dgvSummary != null) dgvSummary.DataSource = DatabaseManager.GetResultsByCategory();
            int tot = dt.Rows.Count;
            lblAtt.Text = tot.ToString();
            if (tot > 0) {
                lblStu.Text = dt.AsEnumerable().Select(r => r["Email"].ToString()).Distinct().Count().ToString();
                lblAvg.Text = $"{dt.AsEnumerable().Average(r => Convert.ToDouble(r["Percentage %"])):F1}%";
                int passed = dt.AsEnumerable().Count(r => Convert.ToDouble(r["Percentage %"]) >= 50);
                lblPass.Text = $"{(double)passed / tot * 100:F0}%";
            }
        }

        private void FilterResults()
        {
            var dt = DatabaseManager.GetAllResults();
            string srch = txtSearch?.Text.ToLower() ?? "";
            string cat = cmbSub?.SelectedIndex > 0 ? cmbSub.SelectedItem.ToString() : "";
            var rows = dt.AsEnumerable().Where(r =>
                (string.IsNullOrEmpty(srch) || r["Student"].ToString().ToLower().Contains(srch) || r["Email"].ToString().ToLower().Contains(srch)) &&
                (string.IsNullOrEmpty(cat) || r["Subject"].ToString() == cat));
            if (dgvAll != null) dgvAll.DataSource = rows.Any() ? rows.CopyToDataTable() : dt.Clone();
        }

        private void RefreshSubDropdown()
        {
            if (cmbSub == null) return;
            int sel = cmbSub.SelectedIndex;
            cmbSub.Items.Clear(); cmbSub.Items.Add("All Subjects");
            foreach (var c in DatabaseManager.GetAllCategories()) cmbSub.Items.Add(c.Name);
            cmbSub.SelectedIndex = Math.Min(sel, cmbSub.Items.Count - 1);
        }

        private void ExportCSV(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"quiz_results_{DateTime.Now:yyyyMMdd_HHmm}.csv" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                var dt = DatabaseManager.GetAllResults();
                using var w = new System.IO.StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
                w.WriteLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(c => $"\"{c.ColumnName}\"")));
                foreach (DataRow row in dt.Rows) w.WriteLine(string.Join(",", row.ItemArray.Select(i => $"\"{i}\"")));
                MessageBox.Show("Export ho gaya! ✅", "Kamyab");
            } catch (Exception ex) { MessageBox.Show(ex.Message, "Ghalti"); }
        }
    }

    public class AddEditCategoryForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        public QuizCategory Result { get; private set; }
        private QuizCategory existing;
        private TextBox txtName, txtDesc;
        private ComboBox cmbIcon;

        public AddEditCategoryForm(QuizCategory edit = null)
        {
            existing = edit;
            Text = edit != null ? "Edit Category" : "New Category";
            Size = new Size(520, 370); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = false; BackColor = Color.White;

            var hdr = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = DG };
            hdr.Controls.Add(new Label { Text = edit != null ? "Edit Category" : "New Subject / Category", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(16, 14) });

            void L(string t, int y) => Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(35, 65, 35), AutoSize = true, Location = new Point(20, y) });

            L("Category / Subject Name:", 65); txtName = TB(new Point(20, 85), 470, "e.g. Compiler Construction");
            L("Icon (Emoji):", 127); cmbIcon = new ComboBox { Size = new Size(130, 32), Location = new Point(20, 147), Font = new Font("Segoe UI", 13), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIcon.Items.AddRange(new object[] { "💻","🔀","⚙️","🧩","📊","🌐","🔬","📐","🗄️","🤖","📝","🔐","📡","🧮","📚","🖧","🎓" });
            cmbIcon.SelectedIndex = 0;
            L("Description:", 188); txtDesc = TB(new Point(20, 208), 470, "Brief description...");

            var bS = BtnG("SAVE", DG, new Point(20, 262), 185, 44); bS.Click += (s, e) => { if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Naam likhein!"); return; } Result = new QuizCategory { Id = existing?.Id ?? 0, Name = txtName.Text.Trim(), Description = txtDesc.Text.Trim(), Icon = cmbIcon.SelectedItem?.ToString() ?? "📚", IsActive = true }; DialogResult = DialogResult.OK; Close(); };
            var bC = BtnG("CANCEL", Color.FromArgb(185, 140, 50), new Point(215, 262), 140, 44); bC.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { hdr, txtName, cmbIcon, txtDesc, bS, bC });
            if (existing != null) { txtName.Text = existing.Name; txtDesc.Text = existing.Description; for (int i = 0; i < cmbIcon.Items.Count; i++) if (cmbIcon.Items[i].ToString() == existing.Icon) { cmbIcon.SelectedIndex = i; break; } }
        }

        static TextBox TB(Point loc, int w, string ph) => new TextBox { Size = new Size(w, 32), Location = loc, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = ph };
        static Button BtnG(string t, Color c, Point loc, int w, int h) { var b = new Button { Text = t, Size = new Size(w, h), Location = loc, Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
    }
}
