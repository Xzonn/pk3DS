﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using pk3DS.Properties;

namespace pk3DS.Subforms
{
    public partial class MapPermView : Form
    {
        public MapPermView()
        {
            InitializeComponent();
        }

        private ToolTip mapCoord = new ToolTip();
        public int mapScale = -1;
        public int DrawMap = -1;
        public void drawMap(int Map)
        {
            DrawMap = Map;
            PB_Map.Image = (CHK_AutoDraw.Checked) ? getMapImage() : null;
        }
        public Image getMapImage(bool crop = false, bool entity = true)
        {
            // Load MM
            byte[][] MM = CTR.mini.unpackMini(File.ReadAllBytes(OWSE.MapMatrixes[DrawMap]), "MM");
            var mm = new MapMatrix(MM[0]);

            // Load GR TileMaps
            for (int i = 0; i < mm.EntryList.Length; i++)
            {
                if (mm.EntryList[i] == 0xFFFF) // Mystery Zone
                    continue;
                byte[][] GR = CTR.mini.unpackMini(File.ReadAllBytes(OWSE.MapGRs[mm.EntryList[i]]), "GR");
                mm.Entries[i] = new MapMatrix.Entry(GR[0]);
            }
            mapScale = (int)NUD_Scale.Value;
            Image img = mm.Preview(mapScale, (int)NUD_Flavor.Value);

            if (entity && mapScale == 8)
            {
                const float opacity = 0.5f;
                // Overlay every... overworld entity
                foreach (var e in OWSE.CurrentZone.Entities.Furniture)
                {
                    int x = e.X;
                    int y = e.Y;
                    for (int sx = 0; sx < e.WX; sx++) // Stretch X
                        for (int sy = 0; sy < e.WY; sy++) // Stretch Y
                            try { Util.LayerImage(img, Resources.F, (x+sx)*mapScale, (y+sy)*mapScale, opacity); }
                    catch { }
                }
                foreach (var e in OWSE.CurrentZone.Entities.NPCs)
                {
                    int x = e.X;
                    int y = e.Y;
                    try { Util.LayerImage(img, Resources.N, x * mapScale, y * mapScale, opacity); }
                    catch { } 
                }
                foreach (var e in OWSE.CurrentZone.Entities.Warps)
                {
                    int x = (int)e.pX; // shifted warps look weird
                    int y = (int)e.pY; // shifted warps look weird
                    for (int sx = 0; sx < e.Width; sx++) // Stretch X
                        for (int sy = 0; sy < e.Height; sy++) // Stretch Y
                            try { Util.LayerImage(img, Resources.W, ((x+sx)*mapScale), ((y+sy)*mapScale), opacity); }
                    catch { } 
                }
                foreach (var e in OWSE.CurrentZone.Entities.Triggers1)
                {
                    int x = e.X;
                    int y = e.Y;
                    for (int sx = 0; sx < e.Width; sx++) // Stretch X
                        for (int sy = 0; sy < e.Height; sy++) // Stretch Y
                            try { Util.LayerImage(img, Resources.T1, (x + sx) * mapScale, (y + sy) * mapScale, opacity); }
                    catch { }
                }
                foreach (var e in OWSE.CurrentZone.Entities.Triggers2)
                {
                    int x = e.X;
                    int y = e.Y;
                    for (int sx = 0; sx < e.Width; sx++) // Stretch X
                        for (int sy = 0; sy < e.Height; sy++) // Stretch Y
                            try { Util.LayerImage(img, Resources.T2, (x + sx) * mapScale, (y + sy) * mapScale, opacity); }
                    catch { }
                }
            }
            if (crop)
                img = Util.TrimBitmap((Bitmap)img);
            OWSE.mm = mm;
            return img;
        }

        // UI
        private void hoverMap(object sender, MouseEventArgs e)
        {
            if (mapScale < 0)
                return;

            if (PB_Map.Image == null)
                return;

            int X = e.X / (mapScale);
            int Y = e.Y / (mapScale);

            int entryX = X/40;
            int entryY = Y/40;

            int entry = entryY*(PB_Map.Image.Width/40/mapScale) + entryX;
            int epX = X%40;
            int epY = Y%40;
            int tile = epY * 40 + epX;
            try
            {
                var tileVal = (OWSE.mm.Entries[entry] == null)
                    ? "No Tile"
                    : OWSE.mm.Entries[entry].Tiles[tile].ToString("X8");

                L_MapCoord.Text = String.Format("V:0x{3}{2}X:{0,3}  Y:{1,3}", X, Y, Environment.NewLine, tileVal);
            }
            catch { } 
        }
        private void B_Redraw_Click(object sender, EventArgs e)
        {
            if (DrawMap != -1)
                PB_Map.Image = getMapImage();
        }

        private void MapPermView_FormClosing(object sender, FormClosingEventArgs e)
        {
            CHK_AutoDraw.Checked = false;
            Hide();
            e.Cancel = true;
        }
    }
}
