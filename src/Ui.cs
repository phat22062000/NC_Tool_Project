// Cac control giao dien tu ve: theme sang/toi, nut bo goc, the file, o so lieu, tab, xem truoc thu phong, ma NC to mau
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NCTool
{
    static class Theme
    {
        public static bool Dark;
        public static Color Page, Card, Panel, Border, BorderStrong, Text, Muted, Accent, AccentBg, Success, SuccessBg, Warn, WarnBg, Danger, DangerBg, Grid;
        public static void Apply(bool dark)
        {
            Dark = dark;
            if (!dark)
            {
                Page = Color.FromArgb(245, 246, 248); Card = Color.White; Panel = Color.FromArgb(250, 251, 252);
                Border = Color.FromArgb(222, 226, 233); BorderStrong = Color.FromArgb(190, 196, 207);
                Text = Color.FromArgb(28, 34, 48); Muted = Color.FromArgb(102, 112, 133);
                Accent = Color.FromArgb(29, 95, 209); AccentBg = Color.FromArgb(230, 241, 251);
                Success = Color.FromArgb(23, 128, 61); SuccessBg = Color.FromArgb(234, 243, 222);
                Warn = Color.FromArgb(133, 79, 11); WarnBg = Color.FromArgb(250, 238, 218);
                Danger = Color.FromArgb(163, 45, 45); DangerBg = Color.FromArgb(252, 235, 235);
                Grid = Color.FromArgb(236, 239, 244);
            }
            else
            {
                Page = Color.FromArgb(20, 23, 29); Card = Color.FromArgb(29, 34, 43); Panel = Color.FromArgb(24, 28, 35);
                Border = Color.FromArgb(50, 58, 71); BorderStrong = Color.FromArgb(78, 88, 104);
                Text = Color.FromArgb(231, 234, 240); Muted = Color.FromArgb(154, 163, 178);
                Accent = Color.FromArgb(91, 148, 245); AccentBg = Color.FromArgb(30, 50, 80);
                Success = Color.FromArgb(74, 222, 128); SuccessBg = Color.FromArgb(24, 50, 34);
                Warn = Color.FromArgb(251, 191, 36); WarnBg = Color.FromArgb(60, 45, 15);
                Danger = Color.FromArgb(248, 113, 113); DangerBg = Color.FromArgb(62, 28, 28);
                Grid = Color.FromArgb(34, 40, 50);
            }
        }
        public static Color PartColor(string col)
        {
            if (col == "green") return Dark ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);
            if (col == "red") return Dark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
            return Dark ? Color.FromArgb(200, 206, 218) : Color.FromArgb(70, 80, 95);
        }
        public static Color CodeColor(int code)
        {
            if (code == 9017) return Color.FromArgb(220, 38, 38);
            if (code == 9016) return Color.FromArgb(147, 51, 234);
            return Color.FromArgb(37, 99, 235);
        }
    }

    static class Gfx
    {
        public static GraphicsPath RR(RectangleF r, float rad)
        {
            var p = new GraphicsPath(); float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
        // Bieu tuong CAM laser: dau cat laser, tia, tia lua va tam kim loai dang bi cat
        public static void DrawLaser(Graphics g, float x, float y, float size, Color body, Color beam)
        {
            var old = g.Transform; var mode = g.SmoothingMode; g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(x, y); g.ScaleTransform(size / 32f, size / 32f);
            using (var br = new SolidBrush(body)) g.FillPolygon(br, new[] { new PointF(10.5f, 4), new PointF(21.5f, 4), new PointF(18f, 13), new PointF(14f, 13) });
            using (var pen = new Pen(beam, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round }) g.DrawLine(pen, 16, 14, 16, 22);
            using (var pen = new Pen(beam, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 16, 22, 10.5f, 18.5f); g.DrawLine(pen, 16, 22, 21.5f, 18.5f);
                g.DrawLine(pen, 16, 22, 8, 22.5f); g.DrawLine(pen, 16, 22, 24, 22.5f);
            }
            using (var pen = new Pen(body, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 4, 25, 13.2f, 25); g.DrawLine(pen, 18.8f, 25, 28, 25);
            }
            g.Transform = old; g.SmoothingMode = mode;
        }
        public static readonly StringFormat Center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public static readonly StringFormat Left = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
    }

    class RoundButton : Control
    {
        static Image xlsxIcon; static bool xlsxTried;
        static Image XlsxIcon
        {
            get
            {
                if (!xlsxTried)
                {
                    xlsxTried = true;
                    try { using (var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NCTool.xlsx_icon.png")) if (s != null) xlsxIcon = Image.FromStream(s); } catch { }
                }
                return xlsxIcon;
            }
        }
        public bool Primary; public int Glyph, LeadIcon; bool hover, down;   // LeadIcon: 0 khong, 1 bieu tuong file Excel ben trai chu   // Glyph: 0 chu, 1 mat troi, 2 mat trang
        public RoundButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.Hand; Height = 34;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent != null ? Parent.BackColor : Theme.Page);
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            Color fill, fg, bd;
            if (!Enabled) { fill = Theme.Panel; fg = Theme.Muted; bd = Theme.Border; }
            else if (Primary)
            {
                fill = down ? ControlPaint.Dark(Theme.Accent, 0.1f) : hover ? ControlPaint.Light(Theme.Accent, 0.1f) : Theme.Accent;
                fg = Theme.Dark ? Color.FromArgb(10, 18, 32) : Color.White; bd = fill;
            }
            else { fill = down ? Theme.Border : hover ? Theme.Panel : Theme.Card; fg = Theme.Text; bd = Theme.BorderStrong; }
            using (var p = Gfx.RR(r, 8)) using (var b = new SolidBrush(fill)) using (var pen = new Pen(bd)) { g.FillPath(b, p); g.DrawPath(pen, p); }
            if (Glyph == 0 && LeadIcon == 1)
            {
                // bieu tuong file XLSX (anh nhung trong exe, nen trong suot)
                var sz = g.MeasureString(Text, Font); float ih = Height - 4, iw = ih, gap = 6, x0 = (Width - (iw + gap + sz.Width)) / 2;
                Image xi = XlsxIcon;
                if (xi != null)
                {
                    var oi = g.InterpolationMode; var op = g.PixelOffsetMode; g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    var dst = new Rectangle((int)x0, (int)((Height - ih) / 2), (int)iw, (int)ih);
                    if (Enabled)   // to mau bieu tuong theo mau nhan cua giao dien (Theme.Accent)
                    {
                        Color ac = Theme.Accent;
                        using (var ia = new System.Drawing.Imaging.ImageAttributes())
                        {
                            ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][] { new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { ac.R / 255f, ac.G / 255f, ac.B / 255f, 0, 1 } }));
                            g.DrawImage(xi, dst, 0, 0, xi.Width, xi.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                    else using (var ia = new System.Drawing.Imaging.ImageAttributes())
                    {
                        ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][] { new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { 0, 0, 0, .6f, 0 }, new float[] { 0, 0, 0, 0, 1 } }));
                        g.DrawImage(xi, dst, 0, 0, xi.Width, xi.Height, GraphicsUnit.Pixel, ia);
                    }
                    g.InterpolationMode = oi; g.PixelOffsetMode = op;
                }
                using (var br = new SolidBrush(fg)) g.DrawString(Text, Font, br, new RectangleF(x0 + iw + gap, 0, sz.Width + 6, Height), Gfx.Left);
                return;
            }            if (Glyph == 0) { using (var br = new SolidBrush(fg)) g.DrawString(Text, Font, br, new RectangleF(0, 0, Width, Height), Gfx.Center); return; }
            float cx = Width / 2f, cy = Height / 2f;
            if (Glyph == 1)   // mat troi
            {
                using (var br = new SolidBrush(fg)) g.FillEllipse(br, cx - 5, cy - 5, 10, 10);
                using (var pen = new Pen(fg, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    for (int i = 0; i < 8; i++)
                    {
                        double a = i * Math.PI / 4;
                        g.DrawLine(pen, cx + (float)Math.Cos(a) * 8, cy + (float)Math.Sin(a) * 8, cx + (float)Math.Cos(a) * 11, cy + (float)Math.Sin(a) * 11);
                    }
            }
            else   // mat trang luoi liem
            {
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(cx - 8, cy - 8, 16, 16);
                    using (var cut = new GraphicsPath())
                    {
                        cut.AddEllipse(cx - 2, cy - 11, 16, 16);
                        using (var r1 = new Region(path)) using (var br = new SolidBrush(fg)) { r1.Exclude(cut); g.FillRegion(br, r1); }
                    }
                }
            }
        }
    }

    class Chip : Control
    {
        public Chip() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true); Height = 30; }
        public void SetText(string t)
        {
            Text = t; Visible = !string.IsNullOrEmpty(t);
            using (var g = CreateGraphics()) Width = (int)g.MeasureString(t, Font).Width + 34;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent != null ? Parent.BackColor : Theme.Page);
            using (var p = Gfx.RR(new RectangleF(0, 0, Width - 1, Height - 1), Height / 2f)) using (var b = new SolidBrush(Theme.SuccessBg)) g.FillPath(b, p);
            using (var b = new SolidBrush(Theme.Success)) { g.FillEllipse(b, 10, Height / 2f - 3, 6, 6); g.DrawString(Text, Font, b, new RectangleF(20, 0, Width - 20, Height), Gfx.Left); }
        }
    }

    class FileList : ListBox
    {
        public FileList()
        {
            DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 74; BorderStyle = BorderStyle.None;
            SelectionMode = SelectionMode.MultiExtended; IntegralHeight = false; DoubleBuffered = true;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(Theme.Page)) g.FillRectangle(b, e.Bounds);
            if (e.Index < 0) return;
            var f = Items[e.Index] as FileItem; if (f == null) return;
            bool sel = (e.State & DrawItemState.Selected) != 0, err = f.Result == null;
            var r = new RectangleF(e.Bounds.X + 1, e.Bounds.Y + 4, e.Bounds.Width - 3, e.Bounds.Height - 9);
            using (var p = Gfx.RR(r, 9))
            {
                using (var b = new SolidBrush(sel ? Theme.AccentBg : Theme.Card)) g.FillPath(b, p);
                using (var pen = new Pen(err ? Theme.Danger : sel ? Theme.Accent : Theme.Border, sel || err ? 1.6f : 1f)) g.DrawPath(pen, p);
            }
            float cy = r.Y + r.Height / 2;
            Color ic = err ? Theme.Danger : Theme.Success;
            using (var b = new SolidBrush(ic)) g.FillEllipse(b, r.X + 12, cy - 12, 24, 24);
            using (var pen = new Pen(Theme.Card, 2f))
            {
                if (err) { pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; g.DrawLine(pen, r.X + 24 - 4.5f, cy - 4.5f, r.X + 24 + 4.5f, cy + 4.5f); g.DrawLine(pen, r.X + 24 + 4.5f, cy - 4.5f, r.X + 24 - 4.5f, cy + 4.5f); }
                else g.DrawLines(pen, new[] { new PointF(r.X + 18.5f, cy), new PointF(r.X + 22.5f, cy + 4.5f), new PointF(r.X + 29.5f, cy - 4.5f) });
            }
            var tr = new RectangleF(r.X + 48, r.Y + 6, r.Width - 54, r.Height / 2 - 4);
            var sr = new RectangleF(r.X + 48, r.Y + r.Height / 2 - 2, r.Width - 54, r.Height / 2 - 4);
            using (var fb = new Font(Font.FontFamily, 14f, FontStyle.Bold)) using (var b = new SolidBrush(Theme.Text)) g.DrawString(f.Base, fb, b, tr, Gfx.Left);
            string sub = err ? (f.Row == null && f.Prims != null ? "Mã không có trong Excel" : (f.Error ?? "Lỗi")) : f.Row.Group + " · " + f.Row.MaterialText + " · t" + f.Row.Thick.ToString("0.##", CultureInfo.InvariantCulture);
            using (var sf = new Font(Font.FontFamily, 11f)) using (var b = new SolidBrush(err ? Theme.Danger : Theme.Muted)) g.DrawString(sub, sf, b, sr, Gfx.Left);
        }
    }

    class DropZone : Control
    {
        public DropZone() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true); Cursor = Cursors.Hand; }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent.BackColor);
            using (var p = Gfx.RR(new RectangleF(1, 1, Width - 3, Height - 3), 10)) using (var pen = new Pen(Theme.BorderStrong) { DashStyle = DashStyle.Dash }) g.DrawPath(pen, p);
            using (var f = new Font("Segoe UI", 11f)) using (var b = new SolidBrush(Theme.Muted)) g.DrawString("Kéo thả DXF vào đây\nhoặc bấm để chọn file", f, b, new RectangleF(0, 0, Width, Height), Gfx.Center);
        }
    }

    class Tiles : Control
    {
        public string[] Labels = { "Kích Thước Phôi", "Độ Dài Cắt", "Chạy Không", "Số Lần Cắt" };
        public string[] Values = { "—", "—", "—", "—" };
        public Tiles() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent.BackColor);
            int n = 4, gap = 8; float w = (Width - gap * (n - 1)) / (float)n;
            using (var lf = new Font(Font.FontFamily, 11f, FontStyle.Bold)) using (var vf = new Font(Font.FontFamily, 17f, FontStyle.Bold))
                for (int i = 0; i < n; i++)
                {
                    var r = new RectangleF(i * (w + gap), 0, w, Height - 2);
                    using (var p = Gfx.RR(r, 8)) { using (var b = new SolidBrush(Theme.Card)) g.FillPath(b, p); using (var pen = new Pen(Theme.Border)) g.DrawPath(pen, p); }
                    using (var b = new SolidBrush(Theme.Dark ? Color.FromArgb(125, 175, 255) : Color.FromArgb(29, 95, 209))) g.DrawString(Labels[i], lf, b, new RectangleF(r.X + 14, r.Y + 8, r.Width - 18, 22), Gfx.Left);
                    using (var b = new SolidBrush(Theme.Text)) g.DrawString(Values[i], vf, b, new RectangleF(r.X + 14, r.Y + 34, r.Width - 18, 34), Gfx.Left);
                }
        }
    }

    class TabBar : Control
    {
        public string[] Items = { "Xem Trước", "Mã NC" };
        public int Index; public event EventHandler Changed;
        public TabBar() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true); Cursor = Cursors.Hand; }
        float[] Edges(Graphics g)
        {
            var e = new float[Items.Length + 1]; float x = 4;
            for (int i = 0; i < Items.Length; i++) { e[i] = x; x += g.MeasureString(Items[i], Font).Width + 26; }
            e[Items.Length] = x; return e;
        }
        protected override void OnMouseDown(MouseEventArgs ev)
        {
            using (var g = CreateGraphics())
            {
                var ed = Edges(g);
                for (int i = 0; i < Items.Length; i++) if (ev.X >= ed[i] && ev.X < ed[i + 1]) { Index = i; Invalidate(); if (Changed != null) Changed(this, EventArgs.Empty); }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.Clear(Parent.BackColor);
            using (var pen = new Pen(Theme.Border)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            var ed = Edges(g);
            for (int i = 0; i < Items.Length; i++)
            {
                bool on = i == Index; var r = new RectangleF(ed[i], 0, ed[i + 1] - ed[i], Height - 1);
                using (var f = new Font(Font, on ? FontStyle.Bold : FontStyle.Regular)) using (var b = new SolidBrush(on ? Theme.Text : Theme.Muted)) g.DrawString(Items[i], f, b, r, Gfx.Center);
                if (on) using (var b = new SolidBrush(Theme.Accent)) g.FillRectangle(b, r.X + 6, Height - 3, r.Width - 12, 3);
            }
        }
    }

    class Msg { public int Kind; public string Text; }   // 0 thong tin, 1 canh bao, 2 loi, 3 thanh cong (xanh la)
    class WarnList : ListBox
    {
        public WarnList() { Font = new Font("Segoe UI", 10f); DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 30; BorderStyle = BorderStyle.None; IntegralHeight = false; DoubleBuffered = true; SelectionMode = SelectionMode.None; }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(Theme.Page)) g.FillRectangle(b, e.Bounds);
            if (e.Index < 0) return;
            var m = Items[e.Index] as Msg; if (m == null) return;
            Color bg = m.Kind == 2 ? Theme.DangerBg : m.Kind == 1 ? Theme.WarnBg : m.Kind == 3 ? Theme.SuccessBg : Theme.Card;
            Color fg = m.Kind == 2 ? Theme.Danger : m.Kind == 1 ? Theme.Warn : m.Kind == 3 ? Theme.Success : Theme.Muted;
            var r = new RectangleF(e.Bounds.X, e.Bounds.Y + 2, e.Bounds.Width - 2, e.Bounds.Height - 4);
            using (var p = Gfx.RR(r, 7)) using (var b = new SolidBrush(bg)) g.FillPath(b, p);
            using (var b = new SolidBrush(fg))
            {
                g.FillEllipse(b, r.X + 10, r.Y + r.Height / 2 - 4.5f, 9, 9);
                g.DrawString(m.Text, Font, b, new RectangleF(r.X + 28, r.Y, r.Width - 32, r.Height), Gfx.Left);
            }
        }
    }

    class PreviewPanel : Control
    {
        public Result R; public string Placeholder = "";
        double zoom = 1; PointF pan; Point last; bool drag; double mx, my; bool hasMouse;
        public PreviewPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }
        public void ResetView() { zoom = 1; pan = PointF.Empty; Invalidate(); }
        double Sc0() { var pv = R.Preview; double w = Math.Max(pv.X1 - pv.X0, 1e-6), h = Math.Max(pv.Y1 - pv.Y0, 1e-6); const int m = 36; return Math.Max(0.0001, Math.Min((Width - 2 * m) / w, (Height - 2 * m) / h)); }
        PointF ToPx(double x, double y)
        {
            var pv = R.Preview; double sc = Sc0() * zoom, midx = (pv.X0 + pv.X1) / 2, midy = (pv.Y0 + pv.Y1) / 2;
            return new PointF((float)(Width / 2.0 + (x - midx) * sc + pan.X), (float)(Height / 2.0 - (y - midy) * sc + pan.Y));
        }
        void ToModel(int px, int py, out double x, out double y)
        {
            var pv = R.Preview; double sc = Sc0() * zoom, midx = (pv.X0 + pv.X1) / 2, midy = (pv.Y0 + pv.Y1) / 2;
            x = midx + (px - Width / 2.0 - pan.X) / sc; y = midy - (py - Height / 2.0 - pan.Y) / sc;
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (R == null) return;
            double x, y; ToModel(e.X, e.Y, out x, out y);
            zoom = Math.Max(0.2, Math.Min(60, zoom * (e.Delta > 0 ? 1.2 : 1 / 1.2)));
            var pv = R.Preview; double sc = Sc0() * zoom, midx = (pv.X0 + pv.X1) / 2, midy = (pv.Y0 + pv.Y1) / 2;
            pan = new PointF((float)(e.X - Width / 2.0 - (x - midx) * sc), (float)(e.Y - Height / 2.0 + (y - midy) * sc));
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e) { Focus(); drag = true; last = e.Location; Cursor = Cursors.SizeAll; }
        protected override void OnMouseUp(MouseEventArgs e) { drag = false; Cursor = Cursors.Default; }
        protected override void OnMouseLeave(EventArgs e) { hasMouse = false; Invalidate(); }
        protected override void OnDoubleClick(EventArgs e) { ResetView(); }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (R == null) return;
            if (drag) { pan = new PointF(pan.X + e.X - last.X, pan.Y + e.Y - last.Y); last = e.Location; }
            ToModel(e.X, e.Y, out mx, out my); hasMouse = true; Invalidate();
        }
        static double NiceStep(double minModel)
        {
            double[] s = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000 };
            foreach (double v in s) if (v >= minModel) return v; return 10000;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Theme.Card);
            if (R == null)
            {
                using (var f = new Font("Segoe UI", 13f)) using (var b = new SolidBrush(Theme.Muted)) g.DrawString(Placeholder, f, b, new RectangleF(0, 0, Width, Height), Gfx.Center);
                return;
            }
            var pv = R.Preview; double sc = Sc0() * zoom;
            // luoi
            double step = NiceStep(20 / sc), x0, y0, x1, y1; ToModel(0, Height, out x0, out y0); ToModel(Width, 0, out x1, out y1);
            using (var pen = new Pen(Theme.Grid))
            {
                for (double x = Math.Floor(x0 / step) * step; x <= x1; x += step) { var p = ToPx(x, 0); g.DrawLine(pen, p.X, 0, p.X, Height); }
                for (double y = Math.Floor(y0 / step) * step; y <= y1; y += step) { var p = ToPx(0, y); g.DrawLine(pen, 0, p.Y, Width, p.Y); }
            }
            foreach (PLine l in pv.Lines)
                using (var pen = new Pen(Theme.PartColor(l.Col), 1.5f)) g.DrawLines(pen, l.Pts.Select(p => ToPx(p[0], p[1])).ToArray());
            foreach (Circ c in pv.Circles)
            {
                var p = ToPx(c.Cx, c.Cy); float r = (float)(c.R * sc);
                using (var pen = new Pen(Theme.PartColor(c.Col), 1.5f)) g.DrawEllipse(pen, p.X - r, p.Y - r, 2 * r, 2 * r);
            }
            foreach (Pnt pt in pv.Points) { var p = ToPx(pt.X, pt.Y); using (var b = new SolidBrush(Theme.PartColor(pt.Col))) g.FillEllipse(b, p.X - 3, p.Y - 3, 6, 6); }
            using (var f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                for (int i = 0; i < pv.Steps.Count; i++)
                {
                    Step s = pv.Steps[i]; var p = ToPx(s.X, s.Y);
                    using (var b = new SolidBrush(Color.FromArgb(225, Theme.CodeColor(s.Code)))) g.FillEllipse(b, p.X - 9, p.Y - 9, 18, 18);
                    g.DrawString((i + 1).ToString(), f, Brushes.White, new RectangleF(p.X - 12, p.Y - 10, 24, 20), Gfx.Center);
                }
            using (var f = new Font(Font.FontFamily, 8.5f)) using (var b = new SolidBrush(Theme.Muted))
            {
                g.DrawString("Cuộn để thu phóng · kéo để di chuyển · bấm đúp để đặt lại", f, b, new RectangleF(12, 6, Width - 24, 18), Gfx.Left);
                if (hasMouse) g.DrawString("x " + mx.ToString("0.0", CultureInfo.InvariantCulture) + "   y " + my.ToString("0.0", CultureInfo.InvariantCulture), f, b, new RectangleF(0, 6, Width - 12, 18), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                // chu thich
                float lx = 12, ly = Height - 20; string[][] lg = { new[] { "L", "Biên Dạng Ngoài Và Lỗ" }, new[] { "green", "Rạch" }, new[] { "red", "Kẻ Dấu" } };
                foreach (var it in lg)
                {
                    using (var bb = new SolidBrush(Theme.PartColor(it[0]))) g.FillEllipse(bb, lx, ly + 4, 8, 8);
                    g.DrawString(it[1], f, b, lx + 12, ly); lx += 22 + g.MeasureString(it[1], f).Width;
                }
                int[] codes = { 9010, 9016 }; string[] names = { "Through Hole", "Piercing" };
                for (int k = 0; k < codes.Length; k++)
                {
                    using (var bb = new SolidBrush(Theme.CodeColor(codes[k]))) g.FillEllipse(bb, lx, ly + 4, 8, 8);
                    g.DrawString(names[k], f, b, lx + 12, ly); lx += 22 + g.MeasureString(names[k], f).Width;
                }
            }
        }
    }

    static class NcRtf
    {
        static string Esc(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '\\' || c == '{' || c == '}') sb.Append('\\').Append(c);
                else if (c > 126) sb.Append("\\u").Append((int)(short)c).Append('?');
                else sb.Append(c);
            }
            return sb.ToString();
        }
        static string Col(Color c) { return "\\red" + c.R + "\\green" + c.G + "\\blue" + c.B + ";"; }
        static readonly Regex Tok = new Regex(@"(M98P\d+)|(G0[0-3]|G4[01])|(M(?:121|199|30|66))", RegexOptions.Compiled);

        public static string Build(string text)
        {
            var sb = new StringBuilder("{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0 Consolas;}}{\\colortbl;" +
                Col(Theme.Text) + Col(Theme.Muted) + Col(Theme.Accent) + Col(Theme.Dark ? Color.FromArgb(251, 146, 60) : Color.FromArgb(194, 98, 12)) + "}\\f0\\fs24\\cf1 ");
            foreach (string line in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
            {
                string e = Esc(line);
                if (line.StartsWith("(") || line.StartsWith("#") || line.StartsWith("G92") || line == "G90") sb.Append("{\\cf2 ").Append(e).Append("}");
                else sb.Append(Tok.Replace(e, m => "{\\cf" + (m.Groups[1].Success ? 4 : m.Groups[2].Success ? 3 : 2) + " " + m.Value + "}"));
                sb.Append("\\par ");
            }
            return sb.Append("}").ToString();
        }
    }

    // Hop thoai thong bao dong bo mau voi giao dien (thay MessageBox he thong)
    class ThemedDialog : Form
    {
        readonly string title, msg; readonly bool success; readonly RoundButton btn = new RoundButton();
        public static void Show(IWin32Window owner, string title, string msg, bool success)
        {
            using (var d = new ThemedDialog(title, msg, success)) d.ShowDialog(owner);
        }
        public ThemedDialog(string title, string msg, bool success)
        {
            this.title = title; this.msg = msg; this.success = success;
            FormBorderStyle = FormBorderStyle.None; StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false; KeyPreview = true;
            BackColor = Theme.Card; Font = new Font("Segoe UI", 10.5f); DoubleBuffered = true;
            const int w = 640;
            using (var g = CreateGraphics()) { var sz = g.MeasureString(msg, Font, w - 120); ClientSize = new Size(w, Math.Max(210, (int)sz.Height + 160)); }
            btn.Text = "Đóng"; btn.Primary = true; btn.Width = 120; btn.Height = 42; btn.Font = new Font("Segoe UI", 11f);
            btn.Left = w - btn.Width - 24; btn.Top = ClientSize.Height - btn.Height - 20; btn.Click += delegate { Close(); };
            Controls.Add(btn);
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) Close(); };
            Shown += delegate { btn.Focus(); };
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Theme.BorderStrong, 1.5f)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            Color c = success ? Theme.Success : Theme.Danger, bg = success ? Theme.SuccessBg : Theme.DangerBg;
            using (var b = new SolidBrush(bg)) g.FillEllipse(b, 28, 28, 52, 52);
            using (var b = new SolidBrush(c)) g.FillEllipse(b, 36, 36, 36, 36);
            using (var pen = new Pen(Theme.Card, 3.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                if (success) g.DrawLines(pen, new[] { new PointF(45, 54.5f), new PointF(52, 61.5f), new PointF(64, 47) });
                else { g.DrawLine(pen, 47, 47, 61, 61); g.DrawLine(pen, 61, 47, 47, 61); }
            }
            using (var f = new Font("Segoe UI", 15f, FontStyle.Bold)) using (var b = new SolidBrush(Theme.Text)) g.DrawString(title, f, b, new RectangleF(100, 30, Width - 124, 32), Gfx.Left);
            using (var b = new SolidBrush(Theme.Muted)) g.DrawString(msg, Font, b, new RectangleF(100, 72, Width - 124, Height - 150));
        }
    }}
