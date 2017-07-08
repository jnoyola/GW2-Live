using System;
using System.Windows.Forms;

/* Plan
 * 1. Purchase Steel Mining Pick (check character api to see if at least 100 count is already owned/equipped)
 *   a. Move to merchant
 *   b. Find vendor window
 *     1. Take screenshot
 *     2. Press 'F'
 *     3. Take screenshot
 *     4. Find largest rectangle where every single contained pixel was changed
 *   c. Find Steel Mining Pick (3rd from bottom)
 *     1. Move mouse to bottom center of vendor window (a few pixels up to make sure it's inside)
 *     2. Take screenshot
 *     3. Move mouse up a few pixels
 *     4. Wait for highlight animation
 *     5. Take screenshot
 *     6. Check for a 3x100 row of entirely new pixels to the right of the mouse
 *     7. If found, this is a new vendor item from the bottom.
 *        If not found, this is the same vendor item or not an item at all yet.
 *     8. Repeat 2-8 until 3 new vendor items are found.
 *   d. Double click to purchase
 * 2. Equip Steel Mining Pick (check character api to see if it's already equipped)
 *   a. Find inventory window
 *     1. Take screenshot
 *     2. Press 'I'
 *     3. Take screenshot
 *     4. Find largest rectangle where every single contained pixel was changed
 *   b. Find search box
 *     1. Move mouse to top center of inventory window (a few pixels down to make sure it's inside)
 *     2. Take screenshot
 *     3. Click
 *     4. Take screenshot
 *     5. Check for new light pixel between the left edge of the inventory window and the mouse
 *     6. If found, verify a short column of new light pixels are there. This is where the cursor is.
 *        If not found, move the mouse down a few pixels and repeat 2-6.
 *   c. Type "Steel Mining Pick"
 *   d. Move the mouse to where the cursor was found, right a few pixels, and down a few pixels (measure an approximate distance for this)
 *   e. Double click to equip
 *   f. Verify that a Steel Mining Pick is now equipped.
 * 3.
 * 
 * 
 * Architecture
 * - MapImage
 *   - Can be passed to MapView or AI
 * - Plan
 *   - LoadFromFile
 * - New controls
 *   - Record button
 *     - Warning that it will overwrite old route and that it must start at the vendor
 *   - Waypoint text box
 *   - 
 * 
 */

namespace GW2_Live
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Task.Run(async () =>
            //{
            //    for (int i = 0; i < 10; ++i)
            //    {
            //        await Task.Delay(2000);
            //        var b = await proc.TakeScreenshot();
            //        b.Save($"c:\\users\\Jonathan\\Desktop\\shots\\test{i}.png", System.Drawing.Imaging.ImageFormat.Png);
            //    }
            //}).GetAwaiter().GetResult();

            //var imgpath = @"C:\Users\Jonathan\Desktop\test4.png";
            //using (var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default))
            //{
            //    using (var img = Pix.LoadFromFile(imgpath))
            //    {
            //        using (var page = engine.Process(img))
            //        {
            //            var p0 = page.GetHOCRText(0);
            //            Console.Write(p0);
            //            // text variable contains a string with all words found
            //        }
            //    }
            //}

            //SetupProcess();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
