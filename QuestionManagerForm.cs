using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuizApp
{
    public class AddEditQuestionForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);
        public Question Result { get; private set; }
        private Question existing;
        private TextBox txtQ, txtA, txtB, txtC, txtD, txtExp;
        private RadioButton[] rbs = new RadioButton[4];
        private ComboBox cmbCat;
        private CheckBox chkActive;

        public AddEditQuestionForm(Question edit = null) { existing = edit; BuildUI(); if (existing != null) Populate(); }

        private void BuildUI()
        {
            bool em = existing != null;
            Text = em ? "Edit Question" : "Add New Question";
            Size = new Size(700, 695); MinimumSize = new Size(620, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true; BackColor = Color.White;

            var hdr = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = DG };
            hdr.Paint += (s, e) => { using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal); e.Graphics.FillRectangle(br, hdr.ClientRectangle); };
            hdr.Controls.Add(new Label { Text = em ? "Edit Question" : "Add New Question", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 14) });

            void L(string t, int y) => Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(30, 60, 30), AutoSize = true, Location = new Point(20, y) });

            L("Subject/Category", 62);
            cmbCat = new ComboBox { Size = new Size(650, 32), Location = new Point(20, 82), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var c in DatabaseManager.GetAllCategories()) cmbCat.Items.Add(c);
            cmbCat.DisplayMember = "Name"; cmbCat.ValueMember = "Id";
            if (cmbCat.Items.Count > 0) cmbCat.SelectedIndex = 0;

            L("Question", 122);
            txtQ = new TextBox { Size = new Size(650, 70), Location = new Point(20, 142), Font = new Font("Segoe UI", 11), Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Enter the question text here..." };

            L("Options (Select the radio button for the correct answer):", 222);

            Color[] bgs = { Color.FromArgb(232, 242, 255), Color.FromArgb(232, 252, 242), Color.FromArgb(255, 246, 230), Color.FromArgb(255, 232, 238) };
            Color[] fgs = { Color.FromArgb(28, 68, 148), Color.FromArgb(10, 120, 65), Color.FromArgb(165, 105, 10), Color.FromArgb(145, 20, 45) };
            string[] lls = { "A", "B", "C", "D" };
            TextBox[] opts = new TextBox[4];

            for (int i = 0; i < 4; i++) {
                int idx = i;
                var row = new Panel { Size = new Size(655, 44), Location = new Point(20, 244 + i * 50), BackColor = bgs[i] };
                row.Paint += (s, e) => { e.Graphics.DrawRectangle(new Pen(Color.FromArgb(195, 210, 195)), new Rectangle(0, 0, row.Width - 1, row.Height - 1)); };

                var lLet = new Label { Text = lls[i], Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = fgs[i], Size = new Size(34, 38), Location = new Point(8, 4), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
                opts[i] = new TextBox { Size = new Size(470, 28), Location = new Point(46, 8), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.None, BackColor = bgs[i], PlaceholderText = $"Option {lls[i]} text..." };

                rbs[i] = new RadioButton { Size = new Size(100, 34), Location = new Point(528, 5), Tag = idx, BackColor = bgs[i] };
                UpdateRbText(i, false);

                rbs[i].Click += (s, e) => SelectOpt(idx);
                row.Click += (s, e) => SelectOpt(idx);
                lLet.Click += (s, e) => SelectOpt(idx);
                opts[i].Click += (s, e) => SelectOpt(idx);

                row.Controls.AddRange(new Control[] { lLet, opts[i], rbs[i] });
                Controls.Add(row);
            }
            txtA = opts[0]; txtB = opts[1]; txtC = opts[2]; txtD = opts[3];
            SelectOpt(0);

            L("Explanation (Optional):", 446);
            txtExp = new TextBox { Size = new Size(650, 42), Location = new Point(20, 466), Font = new Font("Segoe UI", 10), Multiline = true, PlaceholderText = "Explain the correct answer here..." };

            chkActive = new CheckBox { Text = "Include in Quiz (Active)", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(10, 115, 45), Checked = true, AutoSize = true, Location = new Point(20, 520) };

            var bAdd = BG(em ? "Save Changes" : "Add Question", DG, new Point(20, 568), 205, 46); bAdd.Click += Save;
            var bCnl = BG("CANCEL", Color.FromArgb(185, 30, 30), new Point(235, 568), 140, 46); bCnl.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { hdr, cmbCat, txtQ, txtExp, chkActive, bAdd, bCnl });
        }

        private void UpdateRbText(int idx, bool selected)
        {
            if (rbs[idx] == null) return;
            rbs[idx].Text = selected ? "● Correct" : "○ Incorrect";
            rbs[idx].ForeColor = selected ? Color.FromArgb(10, 115, 45) : Color.FromArgb(140, 30, 30);
            rbs[idx].Font = new Font("Segoe UI", 9, selected ? FontStyle.Bold : FontStyle.Regular);
        }

        private void SelectOpt(int idx)
        {
            for (int i = 0; i < 4; i++) { if (rbs[i] == null) continue; rbs[i].Checked = (i == idx); UpdateRbText(i, i == idx); }
        }

        private void Populate()
        {
            txtQ.Text = existing.Text; txtA.Text = existing.Options[0]; txtB.Text = existing.Options[1]; txtC.Text = existing.Options[2]; txtD.Text = existing.Options[3];
            txtExp.Text = existing.Explanation; chkActive.Checked = existing.IsActive;
            SelectOpt(existing.CorrectIndex);
            for (int i = 0; i < cmbCat.Items.Count; i++) if (((QuizCategory)cmbCat.Items[i]).Id == existing.CategoryId) { cmbCat.SelectedIndex = i; break; }
        }

        private void Save(object s, EventArgs e)
        {
            if (cmbCat.SelectedItem == null) { MessageBox.Show("Category chunein!"); return; }
            if (string.IsNullOrWhiteSpace(txtQ.Text) || string.IsNullOrWhiteSpace(txtA.Text) || string.IsNullOrWhiteSpace(txtB.Text) || string.IsNullOrWhiteSpace(txtC.Text) || string.IsNullOrWhiteSpace(txtD.Text))
            { MessageBox.Show("Sawal aur tamam options bharein!"); return; }
            int cor = 0; for (int i = 0; i < 4; i++) if (rbs[i].Checked) { cor = i; break; }
            var cat = (QuizCategory)cmbCat.SelectedItem;
            Result = new Question { Id = existing?.Id ?? 0, CategoryId = cat.Id, CategoryName = cat.Name, Text = txtQ.Text.Trim(), Options = new[] { txtA.Text.Trim(), txtB.Text.Trim(), txtC.Text.Trim(), txtD.Text.Trim() }, CorrectIndex = cor, Explanation = txtExp.Text.Trim(), IsActive = chkActive.Checked };
            DialogResult = DialogResult.OK; Close();
        }

        static Button BG(string t, Color c, Point loc, int w, int h) { var b = new Button { Text = t, Size = new Size(w, h), Location = loc, Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
    }

    public class QuestionManagerForm : Form
    {
        static readonly Color DG = Color.FromArgb(15, 75, 40);
        static readonly Color MG = Color.FromArgb(25, 105, 55);
        private DataGridView dgv;
        private ComboBox cmbCat;
        private Label lblCount;

        public QuestionManagerForm()
        {
            Text = "Question Manager"; Size = new Size(1140, 700); MinimumSize = new Size(920, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable; MaximizeBox = true;
            BackColor = Color.FromArgb(244, 250, 244);

            var hdr = new Panel { Dock = DockStyle.Top, Height = 56 };
            hdr.Paint += (s, e) => { using var br = new LinearGradientBrush(hdr.ClientRectangle, DG, MG, LinearGradientMode.Horizontal); e.Graphics.FillRectangle(br, hdr.ClientRectangle); };
            hdr.Controls.Add(new Label { Text = "Question Manager – Select Subject, then Add / Edit / Delete", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(18, 17) });

            var tb = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(232, 246, 232) };
            tb.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(195, 220, 195)), 0, tb.Height - 1, tb.Width, tb.Height - 1);

            tb.Controls.Add(new Label { Text = "Subject:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(12, 15) });
            cmbCat = new ComboBox { Size = new Size(265, 30), Location = new Point(80, 9), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCat.DisplayMember = "Name"; cmbCat.ValueMember = "Id";
            cmbCat.SelectedIndexChanged += OnCatChange;
            tb.Controls.Add(cmbCat);

            Button B(string t, Color c, int x, int w) { var b = new Button { Text = t, Size = new Size(w, 30), Location = new Point(x, 9), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
            var bA = B("+ Add Question", Color.FromArgb(15, 115, 55), 355, 135); bA.Click += BtnAdd;
            var bE = B("Edit", Color.FromArgb(28, 68, 148), 498, 80); bE.Click += BtnEdit;
            var bT = B("Activate/Deactivate", Color.FromArgb(172, 124, 0), 586, 155); bT.Click += (s, e) => { int id = GetId(); if (id < 0) return; DatabaseManager.ToggleQuestionStatus(id); LoadGrid(); };
            var bD = B("Delete", Color.FromArgb(175, 28, 28), 749, 80); bD.Click += BtnDel;
            lblCount = new Label { Text = "", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = DG, AutoSize = true, Location = new Point(845, 16) };
            var bC = B("Close", Color.FromArgb(95, 105, 95), 1040, 88); bC.Click += (s, e) => Close();
            tb.Controls.AddRange(new Control[] { bA, bE, bT, bD, lblCount, bC });

            dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(205, 225, 205), Font = new Font("Segoe UI", 10), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells, ColumnHeadersHeight = 38, ScrollBars = ScrollBars.Both };
            dgv.RowTemplate.Height = 32;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = DG, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(6, 0, 0, 0) };
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(238, 250, 238) };
            dgv.DefaultCellStyle = new DataGridViewCellStyle { SelectionBackColor = Color.FromArgb(175, 220, 175), SelectionForeColor = Color.FromArgb(15, 50, 15), Padding = new Padding(4, 0, 0, 0) };
            dgv.CellFormatting += (s, e) => {
                if (e.ColumnIndex < 0 || e.Value == null) return;
                string cn = dgv.Columns[e.ColumnIndex].Name;
                if (cn == "Status") { e.CellStyle.ForeColor = e.Value.ToString().Contains("Active") ? Color.FromArgb(10, 110, 40) : Color.Red; e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); }
                if (cn == "Sahi") { e.CellStyle.ForeColor = DG; e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); }
            };
            dgv.DoubleClick += BtnEdit;

            Controls.Add(dgv); Controls.Add(tb); Controls.Add(hdr);
            LoadCatList();
        }

        private void LoadCatList()
        {
            cmbCat.SelectedIndexChanged -= OnCatChange;
            cmbCat.Items.Clear();
            foreach (var c in DatabaseManager.GetAllCategories()) cmbCat.Items.Add(c);
            cmbCat.SelectedIndexChanged += OnCatChange;
            if (cmbCat.Items.Count > 0) { cmbCat.SelectedIndex = 0; LoadGrid(); }
        }

        private void OnCatChange(object s, EventArgs e) => LoadGrid();

        private void LoadGrid()
        {
            if (cmbCat.SelectedItem == null) return;
            dgv.DataSource = DatabaseManager.GetQuestionsByCategoryTable(((QuizCategory)cmbCat.SelectedItem).Id);
            lblCount.Text = $"Total Questions: {dgv.Rows.Count}";
        }

        private int GetId() { if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Ek sawal select karein!"); return -1; } return Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value); }

        private void BtnAdd(object s, EventArgs e) { using var f = new AddEditQuestionForm(); if (f.ShowDialog() == DialogResult.OK && f.Result != null && DatabaseManager.AddQuestion(f.Result)) { LoadGrid(); MessageBox.Show("Sawal add ho gaya!"); } }
        private void BtnEdit(object s, EventArgs e) { int id = GetId(); if (id < 0) return; var q = DatabaseManager.GetQuestionById(id); if (q == null) return; using var f = new AddEditQuestionForm(q); if (f.ShowDialog() == DialogResult.OK && f.Result != null && DatabaseManager.UpdateQuestion(f.Result)) { LoadGrid(); MessageBox.Show("Update ho gaya!"); } }
        private void BtnDel(object s, EventArgs e) { int id = GetId(); if (id < 0) return; if (MessageBox.Show("Delete ho jayega!", "Delete?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) if (DatabaseManager.DeleteQuestion(id)) { LoadGrid(); MessageBox.Show("Delete ho gaya!"); } }
    }
}
