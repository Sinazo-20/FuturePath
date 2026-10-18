using System;
using System.Drawing;
using System.Windows.Forms;

namespace FuturePath.Presentation
{
    
    public class BaseForm : Form
    {
        // The bold font used for labels and buttons (like the example form).
        private static readonly Font BoldFont = new Font("Arial", 9F, FontStyle.Bold);

        // The middle area. If a screen has more rows than fit, this area gets a scroll bar
        // while the buttons at the bottom stay where they are.
        private readonly Panel content = new Panel();

        private readonly Label errorLabel = new Label();   // shows error messages above the buttons

        private int nextY = 25;               // how far down the next row goes
        private int nextButtonRight = 670;    // where the right edge of the next bottom button goes

        public BaseForm(string title)
        {
            Text = "Future Path - " + title;                 // the text in the window's title bar
            ClientSize = new Size(700, 560);                 // window size
            StartPosition = FormStartPosition.CenterScreen;  // open in the middle of the screen
            FormBorderStyle = FormBorderStyle.FixedSingle;   // the window cannot be stretched
            MaximizeBox = false;

            content.Location = new Point(0, 0);
            content.Size = new Size(700, 450);
            content.AutoScroll = true;

            errorLabel.Location = new Point(60, 455);
            errorLabel.Size = new Size(580, 40);
            errorLabel.ForeColor = Color.Firebrick;

            Controls.Add(content);
            Controls.Add(errorLabel);
        }

        // ---------- Building the screen ----------

        // Adds a row: a bold label on the left and an input (text box, drop-down...) on the right.
        protected void AddRow(string labelText, Control input)
        {
            content.Controls.Add(new Label { Text = labelText, Font = BoldFont, AutoSize = true, Location = new Point(60, nextY + 3) });
            input.Location = new Point(230, nextY);
            input.Width = 250;
            content.Controls.Add(input);
            nextY += 45;   // move down for the next row
        }

        // Adds one control on its own row (used for notes, lists and big buttons).
        protected void AddWide(Control control)
        {
            if (control is Label label) label.MaximumSize = new Size(580, 0);   // long notes wrap onto more lines
            control.Location = new Point(60, nextY);
            content.Controls.Add(control);
            int height = control.AutoSize ? control.PreferredSize.Height : control.Height;
            nextY += height + 12;
        }

        // Creates a plain bold button that runs "onClick" when pressed.
        protected static Button NewButton(string text, EventHandler onClick, int width = 100)
        {
            var button = new Button { Text = text, Font = BoldFont, Width = width, Height = 32 };
            button.Click += onClick;
            return button;
        }

        // Creates a button and puts it in the row along the bottom.
        // Buttons are placed from the RIGHT, so the first one added appears on the far right.
        // (We add "Next" first, then "Back", then "Cancel", so they read Cancel, Back, Next from left to right.)
        protected Button AddButton(string text, EventHandler onClick, int width = 100)
        {
            Button button = NewButton(text, onClick, width);
            button.Location = new Point(nextButtonRight - button.Width, 505);
            Controls.Add(button);
            nextButtonRight -= button.Width + 20;
            return button;
        }

        // Creates a plain read-only table (grid) for showing rows of results.
        // The user can look at it and scroll, but cannot type into it.
        protected static DataGridView NewGrid(object data, int height)
        {
            var grid = new DataGridView
            {
                DataSource = data,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,   // columns share the width
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,     // rows grow so long text is fully visible
                Width = 580,
                Height = height
            };
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;       // long text wraps onto more lines

            // If the table has a small "ID" column, make it narrow so the other columns get more room.
            // A table only gets its columns once it is on the screen, so we do this each time the data
            // is (re)loaded, and only if the "ID" column exists.
            void NarrowIdColumn()
            {
                DataGridViewColumn? idColumn = grid.Columns["ID"];
                if (idColumn != null) idColumn.FillWeight = 12;
            }
            grid.DataBindingComplete += (s, e) => NarrowIdColumn();
            NarrowIdColumn();

            return grid;
        }

        // Fills a drop-down list from a list of items.
        //   display = the property shown to the user (e.g. "SubjectName")
        //   value   = the property that identifies the item (e.g. "SubjectID")
        // Nothing is selected at first, so the user has to choose.
        protected static void Bind(ComboBox box, object items, string display, string value)
        {
            box.DropDownStyle = ComboBoxStyle.DropDownList;   // the user can only pick from the list
            box.DataSource = null;
            box.DisplayMember = display;
            box.ValueMember = value;
            box.DataSource = items;
            box.SelectedIndex = -1;
        }

        // Returns the ID of the item chosen in a drop-down, or 0 if nothing is chosen.
        protected static int SelectedId(ComboBox box)
        {
            return box.SelectedValue is int id ? id : 0;
        }

        // Asks the user "are you sure?" with Yes and No buttons. Returns true only if they click Yes.
        // Used before important changes, as our specification requires.
        protected static bool Confirm(string message)
        {
            return MessageBox.Show(message, "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // Shows an error message above the buttons. Pass "" to clear it.
        protected void ShowError(string message)
        {
            errorLabel.Text = message;
        }

        // ---------- Moving between screens ----------

        // Opens the next screen and hides this one. When the next screen comes back:
        //   "Back" was pressed      -> this screen is shown again (with everything still filled in).
        //   "Cancel" was pressed    -> we go back too, all the way to the Welcome screen.
        protected void GoTo(Form next)
        {
            Hide();
            DialogResult result;
            using (next) { result = next.ShowDialog(); }

            if (result == DialogResult.Abort && Modal)
                DialogResult = DialogResult.Abort;   // pass "go to the Welcome screen" further back
            else
                Show();                              // come back to this screen
        }

        // Closes this screen and returns to the one before it.
        protected void GoBack()
        {
            DialogResult = DialogResult.Cancel;
        }

        // Closes this screen and every screen before it, returning to the Welcome screen.
        protected void GoToWelcome()
        {
            DialogResult = DialogResult.Abort;
        }
    }
}