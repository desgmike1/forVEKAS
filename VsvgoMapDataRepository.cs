namespace SBR.DAS.MapView.SLSide
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using Dictionaries;
    using Model;

    #endregion

    public class VsvgoMapDataRepository : IMapViewDataRepository
    {
        public IEnumerable<VsvgoRegionInfo> GetSubregionsData(DateTime date, int interval, int? powerSysID,
                                                              bool ShowNotPublic)
        {
            using (var ctx = new VsvgoRegionDataDataContext())
            {
                var result = ctx.data_vsvgo_mapv_GetSubregionData2(date, interval, powerSysID).Select(x => new VsvgoRegionInfo
                    {
                        PowerSysId = x.POWER_SYS_ID,
                        SubjectId = x.SUBJECT_ID,
                        Price = x.AVG_PRICE ?? 0,
                        Consum = x.CONSUM ?? 0,
                        Name = x.NAME
                    }).ToList();
                return result;
            }
        }

        public RegionPathMarkup GetRegionPathMarkup(int? powerSystemID, string subjectID)
        {
            using (var context = new VsvgoRegionDataDataContext())
            {
                // var dlo = new DataLoadOptions();
                var entity = context.RegionPathMarkups.FirstOrDefault(r =>
                                                                          r.POWER_SYS_ID == (powerSystemID ?? 0) &&
                                                                          r.SUBJECT_ID == (subjectID ?? ""));
                if (entity == null)
                {
                    return new RegionPathMarkup();
                }

                return new RegionPathMarkup
                {
                    SubjectId = entity.SUBJECT_ID,
                    PowerSysId = entity.POWER_SYS_ID,
                    BgType = entity.BACKGROUND_TYPE,
                    BoundaryPoints = entity.BOUNDARY_POINTS,
                    BoundaryType = entity.BOUNDARY_TYPE,
                    PathMarkup = entity.PATH_MARKUP,
                    PathmarkupFile = entity.PATH_MARKUP_FILE
                };
            }
        }

        public List<RegionPathMarkup> GetSubregionsPathMarkup(int? powerSystemID, string subjectID)
        {
            using (var ctx = new VsvgoRegionDataDataContext())
            {
                int currentVersion = new DictionariesRepository().GetCurrentDictVersion().VERSION_NUMBER;
                IEnumerable<RegionPathMarkups> result;
                Func<RegionPathMarkups, RegionPathMarkup> map = x => new RegionPathMarkup
                {
                    BgType = x.BACKGROUND_TYPE,
                    SubjectId = x.SUBJECT_ID,
                    PowerSysId = x.POWER_SYS_ID,
                    BoundaryPoints = x.BOUNDARY_POINTS,
                    BoundaryType = x.BACKGROUND_TYPE,
                    PathMarkup = x.PATH_MARKUP,
                    PathmarkupFile = x.PATH_MARKUP_FILE
                };
                if (powerSystemID == null && subjectID == null) // РФ
                {
                    result = ctx.RegionPathMarkups.Where(x => x.POWER_SYS_ID != 0 && x.SUBJECT_ID == string.Empty);
                }
                else if (powerSystemID != null && subjectID == null) // ОЭС
                {
                    result = ctx.RegionPathMarkups.Where(x => x.POWER_SYS_ID == powerSystemID.Value && x.SUBJECT_ID != "");
                }
                else
                {
                    result = new List<RegionPathMarkups>().AsQueryable();
                }
                result =
                    new List<RegionPathMarkups>().AsQueryable()
                                                 .Where(x => x.MapRegions.FirstOrDefault().VERSION_NUMBER == currentVersion);

                return result.Select(map).ToList();
            }
        }

        public byte[] LoadRegionBackground(DateTime date, int interval, int? powerSystemID, string subjectID)
        {
            using (var ctx = new VsvgoRegionDataDataContext())
            {
                var result = ctx.Backgrounds.Where(r =>
                                                   r.M_DATE == date
                                                   && r.INTERVAL == interval
                                                   && r.POWER_SYS_ID == (powerSystemID ?? 0)
                                                   && r.SUBJECT_ID == (subjectID ?? string.Empty)
                                                   && r.BACKGROUND_TYPE == (int)MapTypes.Vsvgo)
                                                   .OrderByDescending(x => x.VERSION_NUMBER)
                                                   .Take(1)
                                                   .FirstOrDefault();

                return result == null ? null : result.BACKGROUDN_IMAGE.ToArray();
            }
        }
    }
}