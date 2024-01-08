using Excel_URL_Checker.DTOs;
using Excel_URL_Checker.Interfaces;
using OfficeOpenXml;
using System.Linq;

namespace Excel_URL_Checker.Services
{
    public class CompareService : ICompareService
    {
        public async Task<List<ChildrenDTO>> CompareData(List<ExcelDTO> Data, int Similarity)
        {
            if (Data.Count > 0)
            {

                var result = await Task.Run(() =>
                {
                    return Data.AsParallel().Select(i =>
                   {
                       var keysGroup = new List<string>();

                       keysGroup.Add(i.Key);

                       var similarities = Data.AsParallel().Where(x =>
                       {
                           if (x.Key == i.Key) return false;
                           var DataOrganic = x.organic.Split(",").ToList();
                           var objOrganic = i.organic.Split(",").ToList();
                           var similaritesPercentage = ((100 / objOrganic.Count()) * DataOrganic.Intersect(objOrganic).Count());
                           return similaritesPercentage >= Similarity;

                       }).Select(c =>
                       {
                           keysGroup.Add(c.Key);
                           return new ChildrenDTO()
                           {

                               ID = Guid.NewGuid(),
                               Key = c.Key,
                               Difficulty = c.Difficulty,
                               SearchRate = c.SearchRate,
                               organic = c.organic,
                               ParentSearchLinkLength = i.organic.Split(",").Length,
                               SimilarityLinks = c.organic.Split(",").ToList().Intersect(i.organic.Split(",").ToList()).ToList(),
                               Similarity = ((100 / i.organic.Split(",").ToList().Count()) * c.organic.Split(",").ToList().Intersect(i.organic.Split(",").ToList()).Count())
                           };
                       }).ToList();

                       var ChildrensMaxDifficulty = similarities.Count() > 0 ? similarities.Select(i => i).Max(i => i.Difficulty) : int.Parse("0");

                       return new ChildrenDTO()
                       {
                           ID = Guid.NewGuid(),
                           GroupKeys = keysGroup,
                           Key = i.Key,
                           Difficulty = i.Difficulty != null ? Math.Max(ChildrensMaxDifficulty ?? default(int), i?.Difficulty ?? default(int)) : ChildrensMaxDifficulty,
                           SearchRate = similarities.Select(i => i).Sum(i => i.SearchRate) + i.SearchRate,
                           organic = i.organic,
                           SimilarityChildrens = similarities,
                       };
                   }).DistinctBy(x => x.GroupKeys, new ListStringEqualityComparer()).ToList();
                });
                return result;
            }

            return new List<ChildrenDTO>();

        }

    }
}
public class ListStringEqualityComparer : IEqualityComparer<List<string>>
{
    public bool Equals(List<string> x, List<string> y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            return false;

        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<string> obj)
    {
        unchecked
        {
            int hash = 17;

            foreach (var item in obj)
            {
                hash = hash * 23 + item.GetHashCode();
            }

            return hash;
        }
    }
}
public class ChildrenDTO : ExcelDTO
{
    public Guid ID { get; set; }
    public List<string> GroupKeys { get; set; }
    public List<ChildrenDTO>? SimilarityChildrens { get; set; }
    public int ParentSearchLinkLength { get; set; }
    public int Similarity { get; set; }
    public List<string> SimilarityLinks { get; set; }
    public List<string> OrganicList
    {
        get
        {
            if (string.IsNullOrEmpty(this.organic)) return new List<string>();
            return this.organic.Split(',').ToList();
        }

    }
    public int SearchLinkLength
    {
        get
        {
            return OrganicList.Count;
        }

    }
}

public class GeneratedSimilarity
{
    public List<ChildrenDTO> Childrens { get; set; }
    public int? SearchRate { get; set; }
    public int? Difficulty { get; set; }

}