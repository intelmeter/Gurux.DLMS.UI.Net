// Modified for the Intelmeter fork, 2026-10-16: .NET 10 migration, UI modernization and compatibility fixes. GPL-2.0-only; original notices retained.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Gurux.DLMS.UI
{
    // Apply point-based typography once; WinForms owns subsequent DPI scaling.
    public static class ApplicationTypography
    {
        public const float BodyFontPoints = 9f;
        public static readonly Font BodyFont = new Font("Segoe UI", BodyFontPoints, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly ConditionalWeakTable<Control, object> Styled = new();
        private static readonly Dictionary<string, Font> Fonts = new();
        private static bool installed;
        public static void Install()
        {
            if (installed) return;
            installed = true;
            Application.Idle += (_, _) =>
            {
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray()) Apply(form);
            };
        }
        private static Font Target(Font source)
        {
            bool mono = source.Name.IndexOf("Consol", StringComparison.OrdinalIgnoreCase) >= 0
                || source.Name.IndexOf("Courier", StringComparison.OrdinalIgnoreCase) >= 0;
            string family = mono ? source.Name : "Segoe UI";
            // A new control can inherit an already DPI-scaled Segoe UI font.
            // Preserve that size rather than replacing it with the startup size.
            float size = source.Name == "Segoe UI" ? source.SizeInPoints : Math.Max(BodyFontPoints, source.SizeInPoints);
            string key = family + "|" + size + "|" + source.Style;
            if (!Fonts.TryGetValue(key, out Font font))
                Fonts.Add(key, font = new Font(family, size, source.Style, GraphicsUnit.Point));
            return font;
        }
        public static void Apply(Control root)
        {
            if (root.IsDisposed || Styled.TryGetValue(root, out _)) return;
            var pending = new List<(Control Control, Font Font)>();
            void Capture(Control control)
            {
                if (Styled.TryGetValue(control, out _)) return;
                Styled.Add(control, new object());
                pending.Add((control, Target(control.Font)));
                foreach (Control child in control.Controls) Capture(child);
                if (control.ContextMenuStrip != null) Capture(control.ContextMenuStrip);
            }
            Capture(root);
            foreach (var entry in pending)
            {
                Control control = entry.Control;
                control.Font = entry.Font;
                control.ControlAdded += (_, e) => Apply(e.Control);
                control.ContextMenuStripChanged += (_, _) =>
                {
                    if (control.ContextMenuStrip != null) Apply(control.ContextMenuStrip);
                };
                if (control is ToolStrip strip)
                {
                    strip.ItemAdded += (_, _) => StyleItems(strip);
                    StyleItems(strip);
                }
                if (control is DataGridView grid)
                {
                    grid.FontChanged += (_, _) => UpdateGrid(grid);
                    grid.ColumnAdded += (_, _) => UpdateGrid(grid);
                    UpdateGrid(grid);
                }
            }
        }
        private static void StyleItems(ToolStrip strip)
        {
            foreach (ToolStripItem item in strip.Items)
            {
                item.ResetFont();
                if (item is ToolStripDropDownItem drop)
                {
                    // Auto-generated dropdowns inherit the owner item font.
                    // Explicit item fonts would scale independently on opening.
                    if (!Styled.TryGetValue(drop.DropDown, out _))
                    {
                        Styled.Add(drop.DropDown, new object());
                        drop.DropDown.ItemAdded += (_, _) => StyleItems(drop.DropDown);
                    }
                    StyleItems(drop.DropDown);
                }
            }
        }
        private static void UpdateGrid(DataGridView grid)
        {
            if (grid.IsDisposed) return;
            grid.DefaultCellStyle.Font = grid.Font;
            grid.ColumnHeadersDefaultCellStyle.Font = grid.Font;
            grid.RowHeadersDefaultCellStyle.Font = grid.Font;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                int width = TextRenderer.MeasureText(column.HeaderText, grid.Font).Width
                    + (int)Math.Ceiling(24 * grid.DeviceDpi / 96f);
                column.MinimumWidth = Math.Max(2, width);
            }
        }
    }
}
