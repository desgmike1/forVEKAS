using System;
using System.Drawing;
using SBR.DAS.MapViewNew.DB;
using SBR.DAS.MapViewNew.Dtos;
using SBR.DAS.MapViewNew.Enums;

namespace SBR.Web.WebApi.Models.Map
{
    public class MapBackgroundModel : MapModelBase
    {
        public MapBackgroundModel(MapSettingsModel settings)
            : base(settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            LoadData(settings);
        }

        private void LoadData(MapSettingsModel settings)
        {
            MapBordersDto borders = Repository.GetBorder(settings.Date, settings.Hour, settings.PowerSystemId, settings.SubjectId);

            MapBackgroundDto background = Repository.GetBackground(settings.Date, settings.Hour, settings.PowerSystemId, settings.SubjectId);
            Bitmap backgroundImage;
            if ((background == null) || (background.BackgroundImage == null))
            {
                int width;
                if (borders != null)
                {
                    width = (int)(DefaultHeight / MapHelper.GetHeightToWidthRatio(borders.BoundaryPoints.Split(';')));
                }
                else
                {
                    width = DefaultHeight;
                }
                backgroundImage = new Bitmap(100, 100);
            }
            else
            {
                backgroundImage = MapHelper.CropBorders(MapHelper.BitmapFromBytes(background.BackgroundImage), settings.GetLevel() == MapLevels.Russia);
            }
            BackgroundBytes = MapHelper.BytesFromBitmap(backgroundImage);
        }

        public const int DefaultHeight = 300;

        public string BackgroundMimeType { get { return "image/png"; } }

        public string BackgroundFileName { get { return "mapBackground.png"; } }

        public byte[] BackgroundBytes { get; private set; }
    }
}