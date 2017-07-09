﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GW2_Live
{
    public partial class Form1 : Form
    {
        private class TileRequestState
        {
            public HttpWebRequest request;
            public Graphics canvas;
            public Rectangle src;
            public Rectangle dst;
        }

        static readonly string[] ProcessNames = new string[] { "Gw2-64", "Gw2" };

        const int TileZoom = 7;
        const int TileWidth = 256;
        const float MetersPerInch = 0.0254f;
        const int MapEditZoom = 4;

        const int WM_HOTKEY_MSG_ID = 0x0312;

        ProcessHandler proc;
        MumbleHandler mumble;
        CharacterIdentity identity;

        string mapName;
        double mapX0;
        double mapY0;
        double mapX1;
        double mapY1;
        Rectangle mapRect;
        Timer mapUpdateTimer;

        Bitmap bitmap;
        object canvasLock = new object();
        int numTilesLeft;

        HotkeyHandler hotkeyHandler;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            liveStartButton.Enabled = false;
            editButton.Enabled = false;
            saveButton.Enabled = false;
            resetButton.Enabled = false;
            hotkeyHandler = new HotkeyHandler(this, Keys.Add, hotkeyLabel);

            Task.Run(async () =>
            {
                SetupProcess();
                await SetupMumble();
                await LoadMap();
                LoadTiles();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            mumble.Dispose();
        }

        private void SetupProcess()
        {
            startLabel.SetPropertyThreadSafe("Text", "Finding GW2 process . . .");
            planningLabel.SetPropertyThreadSafe("Text", "Finding GW2 process . . .");

            var processExceptions = new List<Exception>();
            foreach (string processName in ProcessNames)
            {
                try
                {
                    proc = new ProcessHandler(processName);
                }
                catch (Exception e)
                {
                    processExceptions.Add(e);
                }
            }

            if (proc == null)
            {
                throw new AggregateException("The game process could not be found", processExceptions);
            }
        }

        private async Task SetupMumble()
        {
            startLabel.SetPropertyThreadSafe("Text", "Waiting for Mumble Link . . .");
            planningLabel.SetPropertyThreadSafe("Text", "Waiting for Mumble Link . . .");

            mumble = new MumbleHandler();
            await mumble.WaitForActive();

            identity = mumble.GetIdentity();

            startLabel.SetPropertyThreadSafe("Text", $"Mumble Link active for {identity.name}");
            liveStartButton.SetPropertyThreadSafe("Enabled", true);
        }

        private async Task LoadMap()
        {
            planningLabel.SetPropertyThreadSafe("Text", $"Loading map {identity.map_id} for {identity.name} . . .");

            int wContinent;
            int hContinent;
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.guildwars2.com/v2/continents/1");
            using (var res = await req.GetResponseAsync())
            {
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    var json = await reader.ReadToEndAsync();
                    var continent = JsonConvert.DeserializeObject<JObject>(json);
                    wContinent = (int)continent["continent_dims"][0];
                    hContinent = (int)continent["continent_dims"][1];
                }
            }

            req = (HttpWebRequest)WebRequest.Create($"https://api.guildwars2.com/v2/maps/{identity.map_id}");
            using (var res = await req.GetResponseAsync())
            {
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    var json = await reader.ReadToEndAsync();
                    var map = JsonConvert.DeserializeObject<JObject>(json);

                    mapName = (string)map["name"];
                    int x0 = (int)map["continent_rect"][0][0];
                    int y0 = (int)map["continent_rect"][0][1];
                    int x1 = (int)map["continent_rect"][1][0];
                    int y1 = (int)map["continent_rect"][1][1];

                    mapX0 = (double)x0 / wContinent;
                    mapY0 = (double)y0 / hContinent;
                    mapX1 = (double)x1 / wContinent;
                    mapY1 = (double)y1 / wContinent;

                    int mapRectX0 = (int)map["map_rect"][0][0];
                    int mapRectY0 = (int)map["map_rect"][0][1];
                    int mapRectX1 = (int)map["map_rect"][1][0];
                    int mapRectY1 = (int)map["map_rect"][1][1];

                    mapRect = new Rectangle(mapRectX0, mapRectY0, mapRectX1 - mapRectX0, mapRectY1 - mapRectY0);
                }
            }
            
            mapView.Plan = await Plan.LoadOrCreate(identity.map_id);

            startLabel.SetPropertyThreadSafe("Text", $"{identity.name} active in {mapName}");
        }

        private void LoadTiles()
        {
            // Only load the tiles encapsulating the map coordinates.
            double numTilesAcross = Math.Pow(2, TileZoom);
            int tileX0 = (int)Math.Floor(mapX0 * numTilesAcross);
            int tileY0 = (int)Math.Floor(mapY0 * numTilesAcross);
            int tileX1 = (int)Math.Floor(mapX1 * numTilesAcross);
            int tileY1 = (int)Math.Floor(mapY1 * numTilesAcross);
            int widthInTiles = (tileX1 - tileX0 + 1);
            int heightInTiles = (tileY1 - tileY0 + 1);
            numTilesLeft = widthInTiles * heightInTiles;

            int mapPixelWidth = (int)((mapX1 - mapX0) * numTilesAcross * TileWidth);
            int mapPixelHeight = (int)((mapY1 - mapY0) * numTilesAcross * TileWidth);
            int imageWidth = Math.Max(mapPixelWidth, mapPixelHeight);

            planningLabel.SetPropertyThreadSafe("Text", $"Loading {numTilesLeft} tiles for {mapName} . . .");

            bitmap = new Bitmap(mapPixelWidth, mapPixelHeight);
            var canvas = Graphics.FromImage(bitmap);

            int dstX0 = 0;
            int dstY0 = 0;
            int dstX1 = 0;
            int dstY1 = 0;

            for (int j = tileY0; j <= tileY1; ++j)
            {
                dstX0 = 0;

                for (int i = tileX0; i <= tileX1; ++i)
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://tiles.guildwars2.com/1/1/{TileZoom}/{i}/{j}.jpg");

                    int srcX0 = (int)Math.Max(0, numTilesAcross * TileWidth * mapX0 - i * TileWidth);
                    int srcY0 = (int)Math.Max(0, numTilesAcross * TileWidth * mapY0 - j * TileWidth);
                    int srcX1 = (int)Math.Min(TileWidth, numTilesAcross * TileWidth * mapX1 - i * TileWidth);
                    int srcY1 = (int)Math.Min(TileWidth, numTilesAcross * TileWidth * mapY1 - j * TileWidth);
                    int srcWidth = srcX1 - srcX0;
                    int srcHeight = srcY1 - srcY0;

                    int dstWidth = srcWidth;
                    int dstHeight = srcHeight;
                    dstX1 = dstX0 + dstWidth;
                    dstY1 = dstY0 + dstHeight;

                    var state = new TileRequestState()
                    {
                        request = req,
                        canvas = canvas,
                        src = new Rectangle(srcX0, srcY0, srcWidth, srcHeight),
                        dst = new Rectangle(dstX0, dstY0, dstWidth, dstHeight)
                    };
                    req.BeginGetResponse(HandleTileResponse, state);

                    dstX0 = dstX1;
                }

                dstY0 = dstY1;
            }
        }

        private void HandleTileResponse(IAsyncResult result)
        {
            TileRequestState state = result.AsyncState as TileRequestState;
            using (var response = state.request.EndGetResponse(result) as HttpWebResponse)
            {
                lock (canvasLock)
                {
                    using (var image = Image.FromStream(response.GetResponseStream()))
                    {
                        state.canvas.DrawImage(image, state.dst, state.src, GraphicsUnit.Pixel);
                        state.canvas.Save();
                        mapView.SetPropertyThreadSafe("Image", bitmap);
                        mapView.Invalidate();
                    }

                    planningLabel.SetPropertyThreadSafe("Text", $"Loading {numTilesLeft} tiles . . .");

                    if (--numTilesLeft <= 0)
                    {
                        state.canvas.Dispose();

                        planningLabel.SetPropertyThreadSafe("Visible", false);
                        editButton.SetPropertyThreadSafe("Enabled", true);
                    }
                }
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            editButton.Enabled = false;
            saveButton.Enabled = true;
            resetButton.Enabled = true;

            hotkeyHandler.ListenForHotkey();
            UpdateMap();

            mapUpdateTimer = new Timer();
            mapUpdateTimer.Tick += new EventHandler(UpdateMapEvent);
            mapUpdateTimer.Interval = 100;
            mapUpdateTimer.Start();

            mapView.IsEditing = true;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            EndEdit();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            EndEdit();
        }

        private void EndEdit()
        {
            mapView.IsEditing = false;

            mapUpdateTimer.Stop();
            mapUpdateTimer.Dispose();

            hotkeyHandler.StopListeningForHotkey();
            mapView.Reset();

            editButton.Enabled = true;
            saveButton.Enabled = false;
            resetButton.Enabled = false;
        }

        private void UpdateMapEvent(Object myObject, EventArgs myEventArgs)
        {
            UpdateMap();
        }

        private void UpdateMap()
        {
            GetPlayerPosition(out float x, out float y);
            mapView.SetPlayerPosition(x, y, mumble.GetVx(), mumble.GetVy());
            mapView.SetScale(MapEditZoom);
            mapView.Invalidate();
        }

        private void GetPlayerPosition(out float x, out float y)
        {
            x = (mumble.GetX() / MetersPerInch - mapRect.X) / mapRect.Width;
            y = 1 - (mumble.GetY() / MetersPerInch - mapRect.Y) / mapRect.Height;
        }

        private void hotkeyLabel_Click(object sender, EventArgs e)
        {
            hotkeyHandler.ListenForConfigure();
        }

        private void HandleHotkey()
        {
            if (mapView.IsEditing)
            {
                GetPlayerPosition(out float x, out float y);
                mapView.Plan.AddPoint(x, y);
                mapView.Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY_MSG_ID)
                HandleHotkey();
            base.WndProc(ref m);
        }

        private void mapView_Click(object sender, EventArgs e)
        {
            if (mapView.IsEditing)
            {
                MouseEventArgs me = (MouseEventArgs)e;

                if (me.Button == MouseButtons.Left)
                {
                    if (me.Clicks == 1)
                    {
                        mapView.Select(me.X, me.Y);
                    }
                    else
                    {
                        mapView.SpecialSelect(me.X, me.Y);
                    }
                }
                else
                {
                    mapView.Remove(me.X, me.Y);
                }
            }
        }
    }
}
